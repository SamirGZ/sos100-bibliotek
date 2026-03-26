using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class ReservationsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly CatalogueService _catalogueService;

    public ReservationsController(CatalogueService catalogueService)
    {
        _catalogueService = catalogueService;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5115/");
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

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

        var myReservations = reservations?
            .Where(r => r.UserId == userId)
            .ToList() ?? new List<ReservationViewModel>();

        var books = await _catalogueService.GetBookCatalogue();

        foreach (var reservation in myReservations)
        {
            var book = books.FirstOrDefault(b => b.Id == reservation.ItemId);

            if (book != null)
            {
                reservation.BookTitle = book.Title;
                reservation.BookAuthor = book.Author;
            }
        }

        return View(myReservations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFromBook(int itemId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        var reservation = new ReservationViewModel
        {
            ItemId = itemId,
            UserId = userId.Value,
            Status = "Active"
        };

        var json = JsonSerializer.Serialize(reservation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/reservations", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Kunde inte skapa reservationen.";
            return RedirectToAction("Index", "Books");
        }

        TempData["SuccessMessage"] = "Boken har reserverats!";
        return RedirectToAction("Index");
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