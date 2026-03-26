using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _loanApiUrl;

    public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        
        // Tvingar appen att peka på Azure-API:et om vi råkar köra lokalt utan rätt appsettings.
        // Sparade mycket huvudvärk vid deployen!
        var configUrl = configuration["ServiceUrls:LoanApi"];
        if (string.IsNullOrEmpty(configUrl) || configUrl.Contains("localhost"))
        {
            _loanApiUrl = "https://app-sos100-loanservice-dyg8gj9f9csfpd6f5.norwayeast-01.azurewebsites.net";
        }
        else
        {
            _loanApiUrl = configUrl;
        }
    }

    public IActionResult Index() => View();

    // --- 1. LÅNA BOK ---
    [HttpPost]
    public async Task<IActionResult> BorrowBook(int bookId, string bookTitle)
    {
        // Hämtar inloggad användare från Session (som sattes av UserAPI vid inloggning)
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var username = HttpContext.Session.GetString("Username") ?? "Anonym";
        var client = _httpClientFactory.CreateClient();

        // Bygger payloaden som skickas till LoanAPI. 
        var loanRequest = new { 
            UserId = userId, Username = username, BookTitle = bookTitle,
            IsReturned = false, LoanDate = DateTime.Now, ReturnDate = DateTime.Now.AddDays(30)
        };

        try 
        {
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan" : $"{_loanApiUrl}/api/loan";
            var response = await client.PostAsJsonAsync(requestUrl, loanRequest);
            
            // Använder TempData för UI-feedback eftersom det överlever vår RedirectToAction
            if (response.IsSuccessStatusCode) TempData["SuccessMessage"] = $"Boken '{bookTitle}' har lånats!";
            else TempData["ErrorMessage"] = "API svarade med fel vid lån.";
        }
        catch { TempData["ErrorMessage"] = "Kunde inte ansluta till lånetjänsten."; }

        // Skickar tillbaka användaren till katalogen så de ser ändringen
        return RedirectToAction("Index", "Books");
    }

    // --- 2. VISA MINA LÅN ---
    public async Task<IActionResult> MyLoans()
    {
        // Enkel Auth-koll i frontenden. Är du inte inloggad kickas du till login-sidan direkt.
        var userId = HttpContext.Session.GetInt32("UserId"); 
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
        try 
        {
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan" : $"{_loanApiUrl}/api/loan";
            
            // Hämtar ALLA lån från API:et och filtrerar ut användarens lån här i frontenden.
            // Borde egentligen göras i API:et för bättre prestanda vid stora datamängder, men funkar bra för vår MVP.
            var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(requestUrl);
            var myLoans = allLoans?.Where(l => l.UserId == userId).ToList() ?? new();
            
            return View(myLoans);
        }
        catch { return View(new List<LoanViewModel>()); } // Returnerar tom lista vid fel så vyn inte kraschar
    }

    // --- 3. ÅTERLÄMNA BOK ---
    [HttpPost]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var client = _httpClientFactory.CreateClient();
        try 
        {
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan/{id}" : $"{_loanApiUrl}/api/loan/{id}";
            
            // Skickar en PUT med IsReturned = true. Vi raderar alltså inte lånet, 
            // vi gör bara en uppdatering (soft delete-tänk) så API:et kan behålla historiken.
            var response = await client.PutAsJsonAsync(requestUrl, new { Id = id, IsReturned = true });
            
            if (response.IsSuccessStatusCode) TempData["SuccessMessage"] = "Boken har återlämnats!";
        }
        catch { TempData["ErrorMessage"] = "Kunde kontakta API för återlämning."; }

        return RedirectToAction("MyLoans");
    }

    // --- 4. RADERA LÅN ---
    [HttpPost] 
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var client = _httpClientFactory.CreateClient();
        try 
        {
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan/{id}" : $"{_loanApiUrl}/api/loan/{id}";
            await client.DeleteAsync(requestUrl);
        }
        catch { 
            // Sväljer eventuella fel (tom catch) för att inte krascha vyn om API:et hickar vid radering.
        }

        return RedirectToAction("MyLoans");
    }
}