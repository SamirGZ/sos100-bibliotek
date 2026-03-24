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
    public LoanController(LoanDbContext context) => _context = context;

    [HttpGet] // READ (All)
    public async Task<IActionResult> Get() => Ok(await _context.Loans.ToListAsync());

    [HttpGet("{id}")] // READ (Single)
    public async Task<IActionResult> Get(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }

    [HttpPost] // CREATE
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        loan.LoanDate = DateTime.Now;
        loan.ReturnDate = DateTime.Now.AddDays(30);
        loan.IsReturned = false;
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")] // UPDATE
    public async Task<IActionResult> Put(int id, [FromBody] Loan updated)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        loan.IsReturned = updated.IsReturned;
        if (!string.IsNullOrEmpty(updated.BookTitle)) loan.BookTitle = updated.BookTitle;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")] // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}