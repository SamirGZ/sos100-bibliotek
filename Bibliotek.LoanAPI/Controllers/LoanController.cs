using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory, IConfiguration _config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            this._config = _config;
        }

        [HttpGet]
        public IActionResult GetAllLoans()
        {
            var loans = _context.Loans.ToList(); 
            return Ok(loans);
        }

        [ApiKey]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] Loan newLoan)
        {
            var client = _httpClientFactory.CreateClient();

            // Kontroll mot Catalog API
            try 
            {
                string catalogBaseUrl = _config["ExternalServices:CatalogApi"] ?? "http://localhost:5149/api/item/";
                string catalogUrl = $"{catalogBaseUrl}{newLoan.BookId}";
        
                var catalogResponse = await client.GetAsync(catalogUrl);
                if (!catalogResponse.IsSuccessStatusCode)
                {
                    return BadRequest("Boken hittades inte i Catalog API.");
                }
            }
            catch (Exception)
            {
                return StatusCode(503, "Kunde inte ansluta till Catalog API.");
            }

            // Kontroll mot User API
            try
            {
                string userBaseUrl = _config["ExternalServices:UserApi"] ?? "http://localhost:5027/api/user/";
                string userUrl = $"{userBaseUrl}{newLoan.UserId}";

                var userResponse = await client.GetAsync(userUrl);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return BadRequest("Användaren hittades inte i User API.");
                }
            }
            catch (Exception)
            {
                return StatusCode(503, "Kunde inte ansluta till User API.");
            }

            // Spara lån i lokal databas
            newLoan.LoanDate = DateTime.Now;
            newLoan.ReturnDate = DateTime.Now.AddDays(30);
            newLoan.IsReturned = false;

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync(); 

            return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
        }

        [HttpPut("{id}")]
        public IActionResult ReturnBook(int id)
        {
            var loan = _context.Loans.FirstOrDefault(l => l.Id == id);
            if (loan == null) return NotFound("Lån hittades inte.");

            loan.IsReturned = true;
            loan.ReturnDate = DateTime.Now; 
            _context.SaveChanges();

            return Ok(new { Message = "Boken återlämnad!", Loan = loan });
        }
    }
}