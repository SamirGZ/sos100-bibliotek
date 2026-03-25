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
    private readonly string _notificationsApiUrl; // Flyttad från const till variabel

    public LoanController(LoanDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        // Hämtar adressen från Azure/appsettings
        _notificationsApiUrl = configuration["ServiceUrls:NotificationsApi"] + "/api/notifications";
    }

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
            Console.WriteLine($">>> CONNECTION ERROR: {ex.Message}"); 
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());
}