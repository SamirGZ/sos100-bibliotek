using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class NotificationController : Controller
{
    private readonly NotificationService _service;

    public NotificationController(NotificationService service)
    {
        _service = service;
    }
    
    /// Hämtar alla notifikationer och visar dem i vyn.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Notera: Rollhantering bör ske via en dedikerad login-tjänst i produktion
        HttpContext.Session.SetString("Role", "admin");
        
        var notifications = await _service.GetNotificationsAsync();
        return View(notifications);
    }
    
    /// Skapar en ny notifikation för en specifik användare.
    [HttpPost]
    public async Task<IActionResult> Create(int userId, string message)
    {
        await _service.CreateNotificationAsync(message, userId);
        return RedirectToAction(nameof(Index));
    }
    
    /// Uppdaterar status för en notifikation till läst.
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }

    
    /// Tar bort en specifik notifikation permanent.
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteNotificationAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    /// Triggar en kontroll mot LoanAPI för att identifiera och avisera försenade lån.
    [HttpPost]
    public async Task<IActionResult> CheckOverdueLoans()
    {
        await _service.CheckOverdueLoansAsync();
        return RedirectToAction(nameof(Index));
    }
}