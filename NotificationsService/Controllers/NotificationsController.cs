using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Models;

namespace NotificationsService.Controllers;

[ApiController]
[Route("api/[controller]")] // Detta gör adressen till: api/notifications
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    // READ - Hämtar alla notiser till hans vy
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _context.Notifications.ToListAsync();
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();
        return Ok(notification);
    }

    // CREATE - Denna tar emot notisen från DITT LoanAPI
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Notification notification) 
    {
        // Om du skickar tom data eller fel format
        if (notification == null) 
        {
            return BadRequest("Data saknas eller är felaktig.");
        }

        // Här sätter vi ett standardvärde om han har det i sin modell
        notification.CreatedAt = DateTime.Now; 
        notification.IsRead = false;

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
    }

    // UPDATE - Om han vill markera en notis som läst
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Notification updated)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.Message = updated.Message;
        notification.IsRead = updated.IsRead;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE - Ta bort en notis
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}