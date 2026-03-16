using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks; 
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using System;
using System.Net.Http;

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

        // Hämtar alla lån för att se listan
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _context.Loans.ToListAsync(); 
            return Ok(loans);
        }

        // Skapar ett nytt lån med alla valideringar
        [ApiKey]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] Loan newLoan)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Kolla först i min egen databas om boken redan är utlånad
            // Om IsReturned är false betyder det att boken inte har kommit tillbaka än
            var isBookOut = await _context.Loans
                .AnyAsync(l => l.BookId == newLoan.BookId && l.IsReturned == false);

            if (isBookOut)
            {
                return BadRequest("Den här boken är redan utlånad till någon annan.");
            }

            // 2. Kontrollera mot Catalog API (port 5149)
            try 
            {
                string catalogBaseUrl = _config["ExternalServices:CatalogApi"] ?? "http://localhost:5149/api/item/";
                string catalogUrl = $"{catalogBaseUrl}{newLoan.BookId}";
        
                var catalogResponse = await client.GetAsync(catalogUrl);
                if (!catalogResponse.IsSuccessStatusCode)
                {
                    return BadRequest("Boken kunde inte hittas i katalogen.");
                }
            }
            catch (Exception)
            {
                return StatusCode(503, "Catalog API svarar inte.");
            }

            // 3. Kontrollera mot User API (port 5027)
            try
            {
                string userBaseUrl = _config["ExternalServices:UserApi"] ?? "http://localhost:5027/api/user/";
                string userUrl = $"{userBaseUrl}{newLoan.UserId}";

                var userResponse = await client.GetAsync(userUrl);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return BadRequest("Användaren hittades inte i systemet.");
                }
            }
            catch (Exception)
            {
                return StatusCode(503, "User API svarar inte.");
            }

            // 4. Kontrollera mot Reservation API (port 5115)
            // Här kollar vi om någon står i kö för boken
            try
            {
                string resBaseUrl = _config["ExternalServices:ReservationApi"] ?? "http://localhost:5115/api/reservation/";
                string resUrl = $"{resBaseUrl}check/{newLoan.BookId}";

                var resResponse = await client.GetAsync(resUrl);
                
                // Om de svarar 409 (Conflict) tolkar vi det som att boken är reserverad
                if (resResponse.StatusCode == System.Net.HttpStatusCode.Conflict) 
                {
                    return BadRequest("Boken är reserverad av en annan person.");
                }
            }
            catch (Exception)
            {
                // Vi loggar felet men tillåter lånet om reservationstjänsten ligger nere (så vi inte låser systemet)
                Console.WriteLine("Kunde inte ansluta till Reservation API.");
            }

            // 5. Allt ser bra ut - Sätt datum och spara i databasen
            newLoan.LoanDate = DateTime.Now;
            newLoan.ReturnDate = DateTime.Now.AddDays(30); // Standard 30 dagar
            newLoan.IsReturned = false;

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync(); 

            return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
        }

        // Återlämna en bok
        [HttpPut("{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound("Lånet hittades inte.");

            // Markera boken som ledig igen
            loan.IsReturned = true;
            loan.ReturnDate = DateTime.Now; 
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Boken har lämnats tillbaka!", Loan = loan });
        }
    }
}