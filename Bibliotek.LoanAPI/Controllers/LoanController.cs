using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models; // Behövs för att kunna hämta listor (.ToList)
// OBS: Om Rider klagar med röda streck här nere, klicka på det röda och välj "Import missing reference"
// för att hämta in rätt namespace för din LoanDbContext och din Loan-modell.

namespace Bibliotek.LoanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        // 1. Skapa en variabel för databasen
        private readonly LoanDbContext _context;

        // 2. Skapa en konstruktor som tar emot databasen när API:et startar
        public LoanController(LoanDbContext context)
        {
            _context = context;
        }

        // 3. Vår nya endpoint för att hämta alla lån
        [HttpGet]
        public IActionResult GetAllLoans()
        {
            // Här ber vi databasen att hämta alla lån och göra om dem till en lista
            var loans = _context.Loans.ToList(); 
            
            // Om listan är tom, får vi bara tillbaka ett tomt svar: []
            return Ok(loans);
        }
        // Vår nya endpoint för att SKAPA ett lån (POST)
        [HttpPost]
        public IActionResult CreateLoan([FromBody] Loan newLoan)
        {
            // 1. Sätt datum och status automatiskt! Användaren ska inte få bestämma detta.
            newLoan.LoanDate = DateTime.Now; 
            newLoan.ReturnDate = DateTime.Now.AddDays(30); // Lånetid på 30 dagar
            newLoan.IsReturned = false;

            // 2. Spara i databasen
            _context.Loans.Add(newLoan);
            _context.SaveChanges();
            
            // 3. Skicka tillbaka det nyskapade lånet som kvitto
            return Ok(newLoan);
        }
    }
}