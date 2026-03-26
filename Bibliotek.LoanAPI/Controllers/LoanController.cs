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
        
        // Vi hämtar bas-URL från ExternalServices:NotificationApi
        var baseNotif = configuration["ExternalServices:NotificationApi"]?.TrimEnd('/');
        
        if (string.IsNullOrEmpty(baseNotif))
        {
            // Fallback om konfigurationen saknas
            _notificationsApiUrl = "https://app-sos100-notificationservice.azurewebsites.net/api/notifications";
        }
        else
        {
            // Vi tvingar fram plural /api/notifications för att matcha din NotificationsService
            _notificationsApiUrl = $"{baseNotif}/api/notifications";
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        var isBookTaken = await _context.Loans.AnyAsync(l => 
            l.BookTitle == loan.BookTitle && !l.IsReturned);

        if (isBookTaken) return BadRequest("Denna bok är tyvärr redan utlånad.");

        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        await SendNotification(loan.UserId, loan.Username, $"Du har lånat boken: {loan.BookTitle}");

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

        string title = loan.BookTitle;
        int uId = loan.UserId;
        string uName = loan.Username;

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        
        await SendNotification(uId, uName, $"Lånehistoriken för '{title}' har raderats.");

        return NoContent();
    }

    private async Task SendNotification(int userId, string username, string message)
    {
        try 
        {
            using var client = new HttpClient();
            // Denna URL MÅSTE vara exakt den du fick från Azure Portal
            var url = "https://app-sos100-notificationservice.azurewebsites.net/api/notifications";

            var payload = new { 
                UserId = userId, 
                Username = username ?? "Användare", 
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            // Skicka anropet asynkront
            await client.PostAsJsonAsync(url, payload);
        }
        catch (Exception ex) 
        { 
            // Detta skriver till Log Stream om anropet misslyckas
            Console.WriteLine($">>> KUNDE INTE SKICKA NOTIS: {ex.Message}"); 
        }
    }
}