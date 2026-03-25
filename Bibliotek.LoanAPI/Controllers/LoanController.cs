using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using System.Net.Http.Json;

namespace Bibliotek.LoanAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoanController : ControllerBase
{
    private readonly LoanDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string NotificationsApiUrl = "http://localhost:5235/api/notifications";

    public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        // Kontrollera om boken redan är utlånad (global kontroll)
        var isBookTaken = await _context.Loans.AnyAsync(l => 
            l.BookTitle == loan.BookTitle && !l.IsReturned);

        if (isBookTaken) 
        {
            return BadRequest("Denna bok är tyvärr redan utlånad till en annan användare.");
        }

        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Skicka notis med namnet som kom med i 'loan'-objektet från Frontend
        await SendNotification(loan.UserId, loan.Username, $"Du har lånat '{loan.BookTitle}'. Återlämnas senast {loan.ReturnDate.ToShortDateString()}.");

        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Loan updated)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        loan.IsReturned = updated.IsReturned;
        await _context.SaveChangesAsync();

        if (loan.IsReturned)
        {
            // Vi använder det sparade namnet från databasen (loan.Username)
            await SendNotification(loan.UserId, loan.Username, $"Boken '{loan.BookTitle}' har återlämnats.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        await SendNotification(loan.UserId, loan.Username, $"Lånehistoriken för '{loan.BookTitle}' har raderats.");

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DENNA METOD ÄR NU DEN ENDA SOM SKICKAR NOTISER
    private async Task SendNotification(int userId, string username, string message)
    {
        try 
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
        
            // Fallback om username saknas (för gamla rader i databasen)
            var finalUsername = string.IsNullOrEmpty(username) ? "Okänd användare" : username;

            var response = await client.PostAsJsonAsync(NotificationsApiUrl, new { 
                UserId = userId, 
                Username = finalUsername, 
                Message = message 
            });

            // Kontrollera om anropet faktiskt lyckades
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($">>> API REJECTED: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($">>> CONNECTION ERROR: {ex.Message}"); 
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());
}