using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class ReservationsController : Controller
{
    private readonly HttpClient _httpClient;

    public ReservationsController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5115/");
    }

    // 🔵 ADMIN - ser alla reservationer
    public async Task<IActionResult> Index()
    {
        ViewBag.IsAdmin = true;

        var response = await _httpClient.GetAsync("api/reservations");

        if (!response.IsSuccessStatusCode)
        {
            return View(new List<ReservationViewModel>());
        }

        var json = await response.Content.ReadAsStringAsync();

        var reservations = JsonSerializer.Deserialize<List<ReservationViewModel>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return View(reservations ?? new List<ReservationViewModel>());
    }

    // 🟢 USER - ser sina egna
    public async Task<IActionResult> UserReservations(int userId)
    {
        ViewBag.IsAdmin = false;

        var response = await _httpClient.GetAsync($"api/reservations/user/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            return View("Index", new List<ReservationViewModel>());
        }

        var json = await response.Content.ReadAsStringAsync();

        var reservations = JsonSerializer.Deserialize<List<ReservationViewModel>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return View("Index", reservations ?? new List<ReservationViewModel>());
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(int BookId, int UserId, string Status)
    {
        var reservation = new ReservationViewModel
        {
            BookId = BookId,
            UserId = UserId,
            Status = Status,
            ReservationDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(reservation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/reservations", content);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        // 🔥 Efter klick → gå till reservationssidan
        return RedirectToAction("UserReservations", new { userId = UserId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _httpClient.DeleteAsync($"api/reservations/{id}");
        return RedirectToAction("Index");
    }
}