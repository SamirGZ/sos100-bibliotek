using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Models;

namespace NotificationsService.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;
    public NotificationsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Notifications.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Notification n) 
    {
        n.CreatedAt = DateTime.Now; 
        _context.Notifications.Add(n);
        await _context.SaveChangesAsync();
        return Ok(n);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}