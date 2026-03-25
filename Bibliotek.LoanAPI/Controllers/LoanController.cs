using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;

namespace Bibliotek.LoanAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoanController : ControllerBase
{
    private readonly LoanDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _notificationsApiUrl;

    public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        // Säkrare URL-hantering för Notifications
        var baseNotif = configuration["ServiceUrls:NotificationsApi"]?.TrimEnd('/');
        _notificationsApiUrl = $"{baseNotif}/api/notifications";
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        // Kontrollera om boken redan är utlånad
        var isBookTaken = await _context.Loans.AnyAsync(l => 
            l.BookTitle == loan.BookTitle && !l.IsReturned);

        if (isBookTaken) return BadRequest("Denna bok är tyvärr redan utlånad.");

        // Säkerställ att datum sätts om de saknas
        loan.LoanDate = loan.LoanDate == default ? DateTime.Now : loan.LoanDate;
        loan.ReturnDate = loan.ReturnDate == default ? DateTime.Now.AddDays(30) : loan.ReturnDate;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        await SendNotification(loan.UserId, loan.Username, $"Du har lånat '{loan.BookTitle}'.");

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

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    private async Task SendNotification(int userId, string username, string message)
    {
        try 
        {
            var client = _httpClientFactory.CreateClient();
            var finalUsername = string.IsNullOrEmpty(username) ? "Användare" : username;

            await client.PostAsJsonAsync(_notificationsApiUrl, new { 
                UserId = userId, 
                Username = finalUsername, 
                Message = message 
            });
        }
        catch (Exception ex) 
        { 
            // Logga felet men låt inte hela lånet krascha om notisen misslyckas
            Console.WriteLine($">>> NOTIFICATION ERROR: {ex.Message}"); 
        }
    }
}