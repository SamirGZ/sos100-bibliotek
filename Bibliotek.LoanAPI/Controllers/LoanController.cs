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
        var alreadyBorrowed = await _context.Loans
            .AnyAsync(l => l.UserId == loan.UserId && l.BookTitle == loan.BookTitle && !l.IsReturned);

        if (alreadyBorrowed) return BadRequest("Boken är redan lånad.");

        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;
        
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        await SendNotification(loan.UserId, $"Nytt lån: {loan.BookTitle}. Återlämnas {loan.ReturnDate.ToShortDateString()}.");

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
            await SendNotification(loan.UserId, $"Boken '{loan.BookTitle}' har återlämnats.");
        }

        return NoContent();
    }

    // Gemensam metod för att skicka notiser säkert
    private async Task SendNotification(int userId, string message)
    {
        try 
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var notice = new { UserId = userId, Message = message };
            await client.PostAsJsonAsync(NotificationsApiUrl, notice);
        }
        catch (Exception ex) { Console.WriteLine($">>> NOTIFICATION ERROR: {ex.Message}"); }
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();
        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}