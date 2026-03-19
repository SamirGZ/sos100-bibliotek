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

    public async Task<IActionResult> Index()
    {
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
}