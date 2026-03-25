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
        
        // 1. Vi försöker hämta från appsettings.json
        // 2. Om den är tom eller innehåller "localhost", använder vi den hårda Azure-länken som reserv
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

    [HttpPost]
    public async Task<IActionResult> BorrowBook(int bookId, string bookTitle)
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var username = HttpContext.Session.GetString("Username") ?? "Anonym";

        var client = _httpClientFactory.CreateClient();

        var loanRequest = new { 
            UserId = userId, 
            Username = username, 
            BookTitle = bookTitle,
            IsReturned = false,
            LoanDate = DateTime.Now,
            ReturnDate = DateTime.Now.AddDays(30),
            History = new List<object>() 
        };

        try 
        {
            // Vi ser till att URL:en slutar rätt för anropet
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan" : $"{_loanApiUrl}/api/loan";
            
            var response = await client.PostAsJsonAsync(requestUrl, loanRequest);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Boken '{bookTitle}' har lånats!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"API svarade med fel: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            // Här loggas det faktiska felet internt i Azure
            TempData["ErrorMessage"] = "Kunde inte ansluta till lånetjänsten i Azure.";
        }

        return RedirectToAction("Index", "Books");
    }

    public async Task<IActionResult> MyLoans()
    {
        var userId = HttpContext.Session.GetInt32("UserId"); 
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
        try 
        {
            var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan" : $"{_loanApiUrl}/api/loan";
            var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(requestUrl);
            var myLoans = allLoans?.Where(l => l.UserId == userId).ToList() ?? new();
            return View(myLoans);
        }
        catch 
        { 
            return View(new List<LoanViewModel>()); 
        }
    }
}