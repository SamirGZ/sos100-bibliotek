using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks; 
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;

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
            // Inkluderar historiken i svaret för att se alla händelser kopplade till lånet
            var loans = await _context.Loans.Include(l => l.History).ToListAsync(); 
            return Ok(loans);
        }

        [ApiKey]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] Loan newLoan)
        {
            var client = _httpClientFactory.CreateClient();

            // Kontrollera om boken redan är utlånad lokalt
            var isBookOut = await _context.Loans
                .AnyAsync(l => l.BookId == newLoan.BookId && l.IsReturned == false);

            if (isBookOut) return BadRequest("Boken är redan utlånad.");

            // Validering mot externa tjänster (Catalog, User, Reservation)
            try 
            {
                // Catalog check
                var catalogUrl = $"{_config["ExternalServices:CatalogApi"]}{newLoan.BookId}";
                if (!(await client.GetAsync(catalogUrl)).IsSuccessStatusCode)
                    return BadRequest("Boken hittades inte i katalogen.");

                // User check
                var userUrl = $"{_config["ExternalServices:UserApi"]}{newLoan.UserId}";
                if (!(await client.GetAsync(userUrl)).IsSuccessStatusCode)
                    return BadRequest("Användaren hittades inte.");

                // Reservation check
                var resUrl = $"{_config["ExternalServices:ReservationApi"]}check/{newLoan.BookId}";
                var resResponse = await client.GetAsync(resUrl);
                if (resResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    return BadRequest("Boken är reserverad.");
            }
            catch (Exception)
            {
                return StatusCode(503, "Externa valideringstjänster är otillgängliga.");
            }

            // Initiera lånet
            newLoan.LoanDate = DateTime.Now;
            newLoan.ReturnDate = DateTime.Now.AddDays(30);
            newLoan.IsReturned = false;

            // Skapa en första händelse i historiken för att logga skapandet
            newLoan.History.Add(new LoanEvent { Description = "Lån påbörjat och validerat." });

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync(); 

            // Skicka notis till Notification API
            try
            {
                var notifyUrl = _config["ExternalServices:NotificationApi"];
                await client.PostAsJsonAsync(notifyUrl, new { 
                    UserId = newLoan.UserId, 
                    Message = $"Lån bekräftat för bok {newLoan.BookId}." 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification failed: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans.Include(l => l.History).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound("Lånet hittades inte.");

            loan.IsReturned = true;
            loan.ReturnDate = DateTime.Now;

            // Lägg till en ny händelse i historiken vid återlämning
            loan.History.Add(new LoanEvent { Description = "Boken har lämnats tillbaka." });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Boken återlämnad!", Loan = loan });
        }
    }
}