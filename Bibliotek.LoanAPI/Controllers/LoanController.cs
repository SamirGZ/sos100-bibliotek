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
    public class CreateLoanRequest
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
    }

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
            var loans = await _context.Loans.Include(l => l.History).ToListAsync();
            return Ok(loans);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequest request)
        {
            if (request == null) return BadRequest(new { message = "Ingen data mottogs." });

            // --- NYTT: VERIFIERA ANVÄNDARE MOT USER-API ---
            var userClient = _httpClientFactory.CreateClient("UserClient");
            try 
            {
                // Vi anropar den nya endpointen i UserAPI (se steg 3 nedan)
                var userCheck = await userClient.GetAsync($"/api/auth/user-by-id/{request.UserId}");
        
                if (!userCheck.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = $"Användare {request.UserId} hittades inte i UserAPI. Lån nekat." });
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new { message = "UserAPI är nere. Kan inte verifiera användaren." });
            }

            // --- FORTSÄTT SOM VANLIGT ---
            var isBookOut = await _context.Loans
                .AnyAsync(l => l.BookId == request.BookId && !l.IsReturned);

            if (isBookOut) return BadRequest(new { message = "Boken är redan utlånad." });

            var newLoan = new Loan
            {
                BookId = request.BookId,
                UserId = request.UserId,
                LoanDate = DateTime.Now,
                ReturnDate = DateTime.Now.AddDays(30),
                IsReturned = false,
                History = new List<LoanEvent>()
            };

            newLoan.History.Add(new LoanEvent { 
                Description = $"Lån skapat (Verifierat mot UserAPI).",
                EventDate = DateTime.Now 
            });

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lånet sparat och verifierat!", loanId = newLoan.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans.Include(l => l.History).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound(new { message = "Lånet hittades inte." });

            loan.IsReturned = true;
            loan.History.Add(new LoanEvent { 
                Description = "Boken återlämnad.",
                EventDate = DateTime.Now 
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Återlämning klar!", loan });
        }
    }
}