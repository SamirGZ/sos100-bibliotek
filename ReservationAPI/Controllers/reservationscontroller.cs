using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApi.Data;
using ReservationApi.Models;
using System.Net.Http.Json;

namespace ReservationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationDbContext _context;

    public ReservationsController(ReservationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
    {
        var reservations = await _context.Reservations.ToListAsync();
        var result = new List<ReservationDto>();

        using var httpClient = new HttpClient();

        foreach (var r in reservations)
        {
            Book? book = null;
            User? user = null;

            try
            {
                book = await httpClient.GetFromJsonAsync<Book>($"https://localhost:5002/api/books/{r.ItemId}");
            }
            catch
            {
                // Ignorerar fel tillfälligt så sidan inte kraschar
            }

            try
            {
                user = await httpClient.GetFromJsonAsync<User>($"https://localhost:5001/api/users/{r.UserId}");
            }
            catch
            {
                // Ignorerar fel tillfälligt så sidan inte kraschar
            }

            result.Add(new ReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = user?.Name ?? $"User {r.UserId}",

                BookId = r.ItemId,
                BookTitle = book?.Title ?? $"Book {r.ItemId}",
                Author = book?.Author ?? "Unknown",

                ReservationDate = r.ReservationDate,
                Status = r.Status
            });
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound();
        }

        return reservation;
    }

    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
    {
        reservation.ReservationDate = DateTime.UtcNow;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id)
        {
            return BadRequest();
        }

        _context.Entry(reservation).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReservationExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservations.Any(e => e.Id == id);
    }
}