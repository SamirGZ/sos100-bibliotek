using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;
using System.Linq;

namespace sos100_bibliotek.Controllers;

public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private static bool IsAdmin(HttpContext ctx) =>
        string.Equals(ctx.Session.GetString("Role"), "admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(ctx.Session.GetString("Username"), "admin", StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Index", "Login");

        // Ingen automatisk CheckOverdue här – admin kan trigga manuellt via knappen i vyn.
        var allNotifications = await _notificationService.GetNotificationsAsync();

        var displayList = IsAdmin(HttpContext)
            ? allNotifications.OrderByDescending(n => n.CreatedAt).ToList()
            : allNotifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToList();

        return View(displayList);
    }

    [HttpPost]
    public async Task<IActionResult> CheckOverdueLoans()
    {
        if (!IsAdmin(HttpContext))
            return RedirectToAction(nameof(Index));

        try
        {
            await _notificationService.CheckOverdueLoansAsync();
            TempData["Message"] = "Påminnelser har kontrollerats / skickats.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead([FromForm] int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var admin = IsAdmin(HttpContext);
        if (userId == null) return RedirectToAction("Index", "Login");

        try
        {
            var notif = await _notificationService.GetNotificationByIdAsync(id);
            if (notif == null)
            {
                TempData["Error"] = "Notisen hittades inte.";
                return RedirectToAction(nameof(Index));
            }

            if (!admin && notif.UserId != userId)
            {
                TempData["Error"] = "Du har inte behörighet.";
                return RedirectToAction(nameof(Index));
            }

            await _notificationService.MarkAsReadAsync(id);
            TempData["Message"] = "Markerad som läst.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var admin = IsAdmin(HttpContext);
        if (userId == null) return RedirectToAction("Index", "Login");

        try
        {
            var notif = await _notificationService.GetNotificationByIdAsync(id);
            if (notif == null)
            {
                TempData["Error"] = "Notisen hittades inte.";
                return RedirectToAction(nameof(Index));
            }

            if (!admin && notif.UserId != userId)
            {
                TempData["Error"] = "Du har inte behörighet.";
                return RedirectToAction(nameof(Index));
            }

            await _notificationService.DeleteNotificationAsync(id);
            TempData["Message"] = "Notisen togs bort.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
