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
    
    // FIX: Lagt till 's' i slutet (notifications) för att matcha hans controller
    private const string NotificationsApiUrl = "http://localhost:5235/api/notifications";

    public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }

    [HttpPost] // CREATE + NOTIFY
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        // 1. Spara i din databas
        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;
        
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // 2. Skicka till klasskamratens NotificationsService
        try 
        {
            var client = _httpClientFactory.CreateClient();
            var notice = new { 
                UserId = loan.UserId, 
                Message = $"Du har lånat '{loan.BookTitle}'. Återlämnas senast {loan.ReturnDate.ToShortDateString()}." 
            };
            
            var response = await client.PostAsJsonAsync(NotificationsApiUrl, notice);

            if (response.IsSuccessStatusCode)
                Console.WriteLine($">>> SUCCESS: Notis skickad till {NotificationsApiUrl}");
            else
                Console.WriteLine($">>> API ERROR {response.StatusCode}: Han nåddes men nekade JSON-datan (kolla hans [FromBody]).");
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($">>> CONNECTION ERROR: Kunde inte nå {NotificationsApiUrl}. Kontrollera port 5235.");
        }

        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Loan updated)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        loan.IsReturned = updated.IsReturned;
        if (!string.IsNullOrEmpty(updated.BookTitle)) loan.BookTitle = updated.BookTitle;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")] // DELETE + NOTIFY
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        var userId = loan.UserId;
        var bookTitle = loan.BookTitle;

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();

        try 
        {
            var client = _httpClientFactory.CreateClient();
            var notice = new { UserId = userId, Message = $"Lånet för '{bookTitle}' har avbrutits." };
            await client.PostAsJsonAsync(NotificationsApiUrl, notice);
            Console.WriteLine(">>> SUCCESS: Avbryt-notis skickad.");
        }
        catch (Exception ex) 
        {
            Console.WriteLine($">>> ERROR: Kunde inte skicka avbryt-notis: {ex.Message}");
        }

        return NoContent();
    }
}