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

        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            // Vi hämtar lån och inkluderar historik-tabellen
            var loans = await _context.Loans.Include(l => l.History).ToListAsync();
            return Ok(loans);
        }

        [HttpPost]
        [ApiKey] // Aktivera denna för att kräva din API-nyckel (Shahin415)
        public async Task<IActionResult> CreateLoan([FromBody] Loan newLoan)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Lokal kontroll: Är boken redan utlånad?
            var isBookOut = await _context.Loans
                .AnyAsync(l => l.BookId == newLoan.BookId && !l.IsReturned);

            if (isBookOut) return BadRequest("Boken är redan utlånad.");

            try 
            {
                // 2. Extern validering: User-API
                string validationUser = "Shahin"; 
                var userBaseUrl = _config["ExternalServices:UserApi"].TrimEnd('/');
                var userUrl = $"{userBaseUrl}/{validationUser}";
                
                var userResponse = await client.GetAsync(userUrl);
                if (!userResponse.IsSuccessStatusCode)
                    return BadRequest($"Användaren '{validationUser}' hittades inte.");

                // 3. Extern validering: Catalog-API
                var catalogBaseUrl = _config["ExternalServices:CatalogApi"].TrimEnd('/');
                var catalogUrl = $"{catalogBaseUrl}/{newLoan.BookId}";
                
                var catalogResponse = await client.GetAsync(catalogUrl);
                if (!catalogResponse.IsSuccessStatusCode)
                    return BadRequest("Boken finns inte i bibliotekskatalogen.");

                // 4. Extern validering: Reservation-API
                var resBaseUrl = _config["ExternalServices:ReservationApi"].TrimEnd('/');
                var resUrl = $"{resBaseUrl}/check/{newLoan.BookId}";
                var resResponse = await client.GetAsync(resUrl);
                
                if (resResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    return BadRequest("Boken är reserverad och kan inte lånas.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Kunde inte nå externa tjänster: {ex.Message}");
            }

            // 5. Skapa lånet och logga i historiken
            newLoan.LoanDate = DateTime.Now;
            newLoan.ReturnDate = DateTime.Now.AddDays(30);
            newLoan.IsReturned = false;
            
            // Säkerställ att History inte är null och lägg till första händelsen
            newLoan.History ??= new List<LoanEvent>();
            newLoan.History.Add(new LoanEvent 
            { 
                Description = $"Lån skapat. Validerat mot User: Shahin, Book: {newLoan.BookId}",
                EventDate = DateTime.Now 
            });

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync();

            // 6. Skicka notis (Notification-API)
            try
            {
                var notifyUrl = _config["ExternalServices:NotificationApi"].TrimEnd('/');
                await client.PostAsJsonAsync(notifyUrl, new { 
                    UserId = newLoan.UserId, 
                    Message = $"Lån bekräftat för bok {newLoan.BookId}." 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notis misslyckades: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans.Include(l => l.History).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound("Lånet hittades inte.");

            loan.IsReturned = true;
            
            // Logga återlämningen i historiken
            loan.History.Add(new LoanEvent { 
                Description = "Boken återlämnad.",
                EventDate = DateTime.Now 
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Återlämning klar!", Loan = loan });
        }
    }
}