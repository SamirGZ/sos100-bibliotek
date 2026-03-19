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

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReservationViewModel reservation)
    {
        var json = JsonSerializer.Serialize(reservation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync("api/reservations", content);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _httpClient.DeleteAsync($"api/reservations/{id}");
        return RedirectToAction("Index");
    }
}