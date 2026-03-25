using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;
using System.Linq; // VIKTIGT: Denna fixar OrderBy och Where!

namespace sos100_bibliotek.Controllers;

public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");
    
        if (userId == null) return RedirectToAction("Index", "Login");

        // Om admin besöker sidan, kolla efter försenade lån
        if (role == "admin")
        {
            await _notificationService.CheckOverdueLoansAsync();
        }

        var allNotifications = await _notificationService.GetNotificationsAsync();
    
        List<NotificationDto> displayList;

        if (role == "admin")
        {
            displayList = allNotifications.OrderByDescending(n => n.CreatedAt).ToList();
        }
        else
        {
            displayList = allNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        return View(displayList);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _notificationService.DeleteNotificationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }
}