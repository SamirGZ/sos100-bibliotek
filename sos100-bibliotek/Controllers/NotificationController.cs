using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;

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
        var notifications = await _notificationService.GetNotificationsAsync();
        return View(notifications);
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