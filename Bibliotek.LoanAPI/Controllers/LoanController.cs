using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bibliotek.LoanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly LoanDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        /// <summary>
        /// Hämtar samtliga lån inklusive tillhörande händelsehistorik.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _context.Loans.Include(l => l.History).ToListAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Skapar ett nytt lån efter validering mot externa tjänster (User, Catalog, Reservation).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] Loan newLoan)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Intern validering: Kontrollera om boken redan är utlånad i lokal databas
            var isBookOut = await _context.Loans
                .AnyAsync(l => l.BookId == newLoan.BookId && !l.IsReturned);

            if (isBookOut) return BadRequest("Boken är redan utlånad.");

            try 
            {
                // 2. Extern validering: User-API (Port 5027)
                // Integration sker mot AuthController som kräver string (username)
                string validationUser = "Shahin"; 
                var userBaseUrl = _config["ExternalServices:UserApi"].TrimEnd('/');
                var userUrl = $"{userBaseUrl}/{validationUser}";
                
                var userResponse = await client.GetAsync(userUrl);
                if (!userResponse.IsSuccessStatusCode)
                    return BadRequest($"Validering misslyckades: Användaren '{validationUser}' hittades inte i externa systemet.");

                // 3. Extern validering: Catalog-API (Port 5149)
                // Verifierar att boken existerar i det centrala registret
                var catalogBaseUrl = _config["ExternalServices:CatalogApi"].TrimEnd('/');
                var catalogUrl = $"{catalogBaseUrl}/{newLoan.BookId}";
                
                var catalogResponse = await client.GetAsync(catalogUrl);
                if (!catalogResponse.IsSuccessStatusCode)
                    return BadRequest("Boken existerar inte i bibliotekskatalogen.");

                // 4. Extern validering: Reservation-API (Port 5115)
                // Kontrollerar om boken har en aktiv reservation
                var resBaseUrl = _config["ExternalServices:ReservationApi"].TrimEnd('/');
                var resUrl = $"{resBaseUrl}/check/{newLoan.BookId}";
                var resResponse = await client.GetAsync(resUrl);
                
                if (resResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    return BadRequest("Boken är reserverad och kan inte lånas.");
            }
            catch (HttpRequestException ex)
            {
                // Felhantering vid anslutningsproblem till externa mikrotjänster
                return StatusCode(503, $"Anslutningsfel till externa tjänster: {ex.Message}");
            }

            // 5. Persistering av lån och initial händelselogg
            newLoan.LoanDate = DateTime.Now;
            newLoan.ReturnDate = DateTime.Now.AddDays(30);
            newLoan.IsReturned = false;
            
            newLoan.History = new List<LoanEvent> 
            { 
                new LoanEvent { 
                    Description = $"Lån skapat. Validerat mot User: Shahin, Book: {newLoan.BookId}",
                    EventDate = DateTime.Now 
                } 
            };

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync();

            // 6. Asynkront meddelande till Notifications-API (Port 5235)
            try
            {
                var notifyUrl = _config["ExternalServices:NotificationApi"].TrimEnd('/');
                await client.PostAsJsonAsync(notifyUrl, new { 
                    UserId = newLoan.UserId, 
                    Message = $"Bekräftelse: Lån av bok {newLoan.BookId} registrerat. Återlämning: {newLoan.ReturnDate:yyyy-MM-dd}" 
                });
            }
            catch (Exception ex)
            {
                // Loggning av misslyckad notis, påverkar ej låneprocessens utgång
                Console.WriteLine($"Notification service unreachable: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
        }

        /// <summary>
        /// Registrerar återlämning och uppdaterar lånets historik.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans.Include(l => l.History).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound("Låneobjektet hittades inte.");

            loan.IsReturned = true;
            
            // Dokumentation av returhändelse
            loan.History.Add(new LoanEvent { 
                Description = "Återlämning registrerad i systemet.",
                EventDate = DateTime.Now 
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Retur genomförd.", Loan = loan });
        }
    }
}