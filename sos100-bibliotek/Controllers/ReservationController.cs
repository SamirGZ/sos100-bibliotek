using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class ReservationsController : Controller
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        var reservations = await _reservationService.GetReservationsAsync();

        var myReservations = reservations
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.ReservationDate)
            .ToList();

        return View(myReservations);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int bookId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        var reservation = new ReservationViewModel
        {
            ItemId = bookId,
            UserId = userId.Value,
            Status = "Active"
        };

        var success = await _reservationService.CreateReservationAsync(reservation);

        if (!success)
        {
            TempData["ErrorMessage"] = "Det gick inte att reservera boken.";
            return RedirectToAction("Index", "Books");
        }

        TempData["SuccessMessage"] = "Boken reserverades och finns nu under Mina reservationer.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _reservationService.DeleteReservationAsync(id);
        return RedirectToAction("Index");
    }
}