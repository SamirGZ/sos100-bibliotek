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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        return n == null ? NotFound() : Ok(n);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Notification n) 
    {
        n.CreatedAt = DateTime.Now; 
        _context.Notifications.Add(n);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = n.Id }, n);
    }

    /// <summary>
    /// GET ingår så att server-klienter (MVC) kan anropa utan POST-body-problem i vissa Azure-miljöer.
    /// </summary>
    [HttpPut("{id:int}/read")]
    [HttpPost("{id:int}/read")]
    [HttpGet("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        n.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(n);
    }

    /// <summary>
    /// GET-alternativ till DELETE (vissa miljöer/proxys hanterar DELETE sämre).
    /// </summary>
    [HttpGet("{id:int}/delete")]
    public async Task<IActionResult> DeleteByGet(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Notification updated)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();

        // Partiell uppdatering: om klienten bara skickar IsRead ska vi inte nollställa övriga fält
        n.IsRead = updated.IsRead;
        if (updated.UserId != 0) n.UserId = updated.UserId;
        if (!string.IsNullOrEmpty(updated.Message)) n.Message = updated.Message;
        if (!string.IsNullOrEmpty(updated.Username)) n.Username = updated.Username;

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