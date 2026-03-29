using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApi.Data;
using ReservationApi.Models;
using ReservationApi.Services;

namespace ReservationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationDbContext _context;
    private readonly IReservationUpstreamClient _upstream;

    public ReservationsController(ReservationDbContext context, IReservationUpstreamClient upstream)
    {
        _context = context;
        _upstream = upstream;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations(CancellationToken cancellationToken)
    {
        var reservations = await _context.Reservations.ToListAsync(cancellationToken);
        var result = new List<ReservationDto>();

        foreach (var r in reservations)
        {
            CatalogueBook? book = null;
            UserApiProfile? user = null;

            try
            {
                book = await _upstream.GetBookAsync(r.ItemId, cancellationToken);
            }
            catch (HttpRequestException)
            {
                // KatalogAPI tillfälligt otillgänglig — visa reservation ändå
            }

            try
            {
                user = await _upstream.GetUserAsync(r.UserId, cancellationToken);
            }
            catch (HttpRequestException)
            {
                // UserAPI otillgänglig
            }

            result.Add(new ReservationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = user?.Username ?? $"User {r.UserId}",

                BookId = r.ItemId,
                BookTitle = book?.Title ?? $"Book {r.ItemId}",
                Author = book?.Author ?? "Unknown",

                ReservationDate = r.ReservationDate,
                Status = r.Status
            });
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
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
    public async Task<ActionResult<Reservation>> CreateReservation([FromBody] CreateReservationDto dto, CancellationToken cancellationToken)
    {
        var user = await _upstream.GetUserAsync(dto.UserId, cancellationToken);
        if (user == null)
            return BadRequest(new { message = "UserId finns inte i UserAPI." });

        var book = await _upstream.GetBookAsync(dto.BookId, cancellationToken);
        if (book == null)
            return BadRequest(new { message = "BookId finns inte i KatalogAPI." });

        var reservation = new Reservation
        {
            UserId = dto.UserId,
            ItemId = dto.BookId,
            ReservationDate = DateTime.UtcNow,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id)
        {
            return BadRequest();
        }

        var user = await _upstream.GetUserAsync(reservation.UserId);
        if (user == null)
            return BadRequest(new { message = "UserId finns inte i UserAPI." });

        var book = await _upstream.GetBookAsync(reservation.ItemId);
        if (book == null)
            return BadRequest(new { message = "ItemId (bok) finns inte i KatalogAPI." });

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

    [HttpDelete("{id:int}")]
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
