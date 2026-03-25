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
        // Kontrollerar om boken redan är utlånad till någon användare genom att matcha titel och IsReturned-status.
        var isBookTaken = await _context.Loans.AnyAsync(l => 
            l.BookTitle == loan.BookTitle && !l.IsReturned);

        if (isBookTaken) 
        {
            return BadRequest("Denna bok är tyvärr redan utlånad till en annan användare.");
        }

        // Initierar lånets metadata. ReturnDate sätts till standard 30 dagar.
        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;
    
        // Persisterar låneobjektet till databasen.
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Skickar asynkron notifiering till NotificationsAPI med kopplat UserId.
        await SendNotification(loan.UserId, $"Du har lånat '{loan.BookTitle}'. Återlämnas senast {loan.ReturnDate.ToShortDateString()}.");

        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Loan updated)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        // Uppdaterar returstatus. UserId behålls från ursprungsobjektet för att säkra notifikationsmottagaren.
        loan.IsReturned = updated.IsReturned;
        await _context.SaveChangesAsync();

        if (loan.IsReturned)
        {
            await SendNotification(loan.UserId, $"Boken '{loan.BookTitle}' har återlämnats.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        await SendNotification(loan.UserId, $"Lånehistoriken för '{loan.BookTitle}' har raderats.");

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task SendNotification(int userId, string message)
    {
        try 
        {
            // Etablerar HTTP-klient med kort timeout för att inte blockera huvudprocessen vid nätverksfel.
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            await client.PostAsJsonAsync(NotificationsApiUrl, new { UserId = userId, Message = message });
        }
        catch (Exception ex) 
        { 
            // Loggar fel internt om notifikationstjänsten ej är tillgänglig.
            Console.WriteLine($">>> NOTIFICATION ERROR: {ex.Message}"); 
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());
}