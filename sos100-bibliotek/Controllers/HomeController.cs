using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ApiUrl = "http://localhost:5029/api/loan";

    public HomeController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [HttpGet]
    public IActionResult Index() => View();
    
    [HttpPost]
    public async Task<IActionResult> BorrowBook(int bookId, string bookTitle)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("Username"); // Hämta namnet här!

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://localhost:5029/api/loan", new { 
            UserId = userId, 
            Username = username, // Skicka med namnet till API:et
            BookTitle = bookTitle 
        });
        

        if (response.IsSuccessStatusCode)
        {
            // Lagrar feedback som raderas efter nästa sidvisning
            TempData["SuccessMessage"] = $"Boken '{bookTitle}' har lagts till i dina lån!";
        }
        else
        {
            // Hämtar felmeddelandet direkt från API:et (t.ex. "Boken är redan utlånad")
            var errorReason = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = !string.IsNullOrEmpty(errorReason) ? errorReason : "Kunde inte låna boken.";
        }

        // Skickar tillbaka användaren till boklistan istället för "Mina lån"
        // Ändra från: return RedirectToAction("Index");
        return RedirectToAction("Index", "Books");
    }

    public async Task<IActionResult> MyLoans()
    {
        var userId = HttpContext.Session.GetInt32("UserId"); 
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
        try 
        {
            var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(ApiUrl);
            var myLoans = allLoans?.Where(l => l.UserId == userId).ToList() ?? new List<LoanViewModel>();
            return View(myLoans);
        }
        catch 
        { 
            return View(new List<LoanViewModel>()); 
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("Username"); // Lägg till detta
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
    
        // Skicka med username även vid återlämning för säkerhets skull
        await client.PutAsJsonAsync($"{ApiUrl}/{id}", new { 
            IsReturned = true, 
            UserId = userId,
            Username = username 
        });

        return RedirectToAction("MyLoans");
    }

    [HttpPost] 
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var client = _httpClientFactory.CreateClient();
        await client.DeleteAsync($"{ApiUrl}/{id}");
        return RedirectToAction("MyLoans");
    }
}