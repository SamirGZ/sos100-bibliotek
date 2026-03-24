using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> MyLoans()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Index", "Login");
        }

        // Exekverar anrop mot LoanAPI för att hämta aktiva lån för specifik användare
        var client = _httpClientFactory.CreateClient("LoanAPI");
        var response = await client.GetAsync($"api/loan/user/{userId}");

        if (response.IsSuccessStatusCode)
        {
            var loans = await response.Content.ReadFromJsonAsync<List<LoanViewModel>>();
            return View(loans);
        }

        return View(new List<LoanViewModel>());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}