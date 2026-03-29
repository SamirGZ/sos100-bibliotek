using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class ReservationsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CatalogueService _catalogueService;
    private readonly UserApiService _userApiService;

    public ReservationsController(
        CatalogueService catalogueService,
        IHttpClientFactory httpClientFactory,
        UserApiService userApiService)
    {
        _catalogueService = catalogueService;
        _httpClientFactory = httpClientFactory;
        _userApiService = userApiService;
    }

    private HttpClient ReservationApiClient => _httpClientFactory.CreateClient("ReservationApi");

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("Username");

        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        var isAdmin = username == "admin";
        ViewBag.IsAdmin = isAdmin;

        var response = await ReservationApiClient.GetAsync("api/reservations");

        if (!response.IsSuccessStatusCode)
        {
            return View(new List<ReservationViewModel>());
        }

        var json = await response.Content.ReadAsStringAsync();

        var reservations = JsonSerializer.Deserialize<List<ReservationViewModel>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ReservationViewModel>();

        List<ReservationViewModel> reservationsToShow;

        if (isAdmin)
        {
            reservationsToShow = reservations;
        }
        else
        {
            reservationsToShow = reservations
                .Where(r => r.UserId == userId.Value)
                .ToList();
        }

        var books = await _catalogueService.GetBookCatalogue();
        var userIds = reservationsToShow.Select(r => r.UserId).Distinct().ToList();
        var userNames = new Dictionary<int, string>();
        foreach (var uid in userIds)
        {
            var profile = await _userApiService.GetUserByIdAsync(uid);
            if (profile != null)
                userNames[uid] = profile.Username;
        }

        foreach (var reservation in reservationsToShow)
        {
            if (userNames.TryGetValue(reservation.UserId, out var un))
                reservation.UserName = un;

            var book = books.FirstOrDefault(b => b.Id == reservation.ItemId);

            if (book != null)
            {
                reservation.BookTitle = book.Title;
                reservation.Author = book.Author;
            }
        }

        return View(reservationsToShow);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFromBook(int itemId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        var response = await ReservationApiClient.PostAsJsonAsync("api/reservations", new
        {
            userId = userId.Value,
            bookId = itemId,
            status = "Active"
        });

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
        var username = HttpContext.Session.GetString("Username");

        if (username != "admin")
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReservationViewModel reservation)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username != "admin")
        {
            return RedirectToAction("Index");
        }

        var response = await ReservationApiClient.PostAsJsonAsync("api/reservations", new
        {
            userId = reservation.UserId,
            bookId = reservation.ItemId,
            status = string.IsNullOrWhiteSpace(reservation.Status) ? "Active" : reservation.Status
        });

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Kunde inte skapa reservationen. Kontrollera att användar-ID och bok-ID finns i UserAPI respektive KatalogAPI.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Reservationen skapades!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await ReservationApiClient.DeleteAsync($"api/reservations/{id}");
        TempData["SuccessMessage"] = "Reservationen togs bort.";
        return RedirectToAction("Index");
    }
}