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
        
        // Hämtar URL från Azure miljövariabler för att slippa hårdkoda i produktion.
        var baseNotif = configuration["ExternalServices:NotificationApi"]?.TrimEnd('/');
        
        if (string.IsNullOrEmpty(baseNotif))
        {
            // Fallback-URL om konfigurationen saknas, förhindrar att API:et kraschar vid uppstart.
            _notificationsApiUrl = "https://app-sos100-notificationservice.azurewebsites.net/api/notifications";
        }
        else
        {
            // Tvingar fram korrekt plural-form (/api/notifications) för att undvika 404-fel i routing.
            _notificationsApiUrl = $"{baseNotif}/api/notifications";
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    // Kräver X-Api-Key från frontenden för att stoppa obehöriga    [ApiKeyAuthorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        // 1. Validering: Säkerställer att ingen annan redan har lånat denna specifika bok.
        var isBookTaken = await _context.Loans.AnyAsync(l => 
            l.BookTitle == loan.BookTitle && !l.IsReturned);

        // Returnerar 400 Bad Request för att frontenden ska kunna visa ett relevant felmeddelande.
        if (isBookTaken) return BadRequest("Denna bok är tyvärr redan utlånad.");

        // 2. Sätter systemgenererad data. Låter inte klienten/användaren styra över datum för att undvika fusk.
        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // 3. Triggar en integration till notifikationstjänsten när lånet sparats korrekt.
        await SendNotification(loan.UserId, loan.Username, $"Du har lånat boken: {loan.BookTitle}");

        // Returnerar 201 Created tillsammans med det nyskapade objektet och dess nya ID.
        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [ApiKeyAuthorize] // Skyddar uppdateringar så ingen kan ändra data utifrån
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Loan updated)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        // Vi uppdaterar endast IsReturned (Soft delete-princip) för att bevara lånehistorik.
        loan.IsReturned = updated.IsReturned;
        await _context.SaveChangesAsync();

        // Skickar enbart en notis om boken faktiskt markeras som återlämnad nu.
        if (loan.IsReturned)
        {
            await SendNotification(loan.UserId, loan.Username, $"Boken '{loan.BookTitle}' har återlämnats.");
        }

        return NoContent(); // 204 No Content är standard för en lyckad PUT utan returdata.
    }

    [ApiKeyAuthorize] // Låst endpoint: Kräver giltig API-nyckel
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        // Måste mellanlagra variablerna innan vi raderar objektet.
        // Om vi raderar direkt kan vi inte läsa ut titeln eller UserId till notifikationen.
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
        // Try-catch är kritiskt här. Om notifikationstjänsten är nere (t.ex. pga Azure Quota) 
        // får det INTE krascha hela låne-processen. Kärnfunktionaliteten (lånet) ska prioriteras.
        try 
        {
            using var client = new HttpClient();
            var url = "https://app-sos100-notificationservice.azurewebsites.net/api/notifications";

            // Bygger DTO-objektet som mottagaren (NotificationsAPI) förväntar sig.
            var payload = new { 
                UserId = userId, 
                Username = username ?? "Användare", 
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await client.PostAsJsonAsync(url, payload);
        }
        catch (Exception ex) 
        { 
            // Loggar felet internt för felsökning via Azure Log Stream.
            Console.WriteLine($">>> KUNDE INTE SKICKA NOTIS: {ex.Message}"); 
        }
    }
}