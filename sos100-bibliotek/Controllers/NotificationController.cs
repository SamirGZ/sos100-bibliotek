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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var notifications = await _service.GetNotificationsAsync();
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteNotificationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CheckOverdueLoans()
    {
        await _service.CheckOverdueLoansAsync();
        return RedirectToAction(nameof(Index));
    }
}