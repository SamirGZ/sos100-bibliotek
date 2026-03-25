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
        _loanApiUrl = configuration["ServiceUrls:LoanApi"] + "/api/loan";
    }

    [HttpGet]
    public IActionResult Index() => View();
    
    [HttpPost]
    public async Task<IActionResult> BorrowBook(int bookId, string bookTitle)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("Username");

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(_loanApiUrl, new { 
            UserId = userId, 
            Username = username, 
            BookTitle = bookTitle 
        });
        
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = $"Boken '{bookTitle}' har lånats!";
        else
            TempData["ErrorMessage"] = "Kunde inte låna boken.";

        return RedirectToAction("Index", "Books");
    }

    public async Task<IActionResult> MyLoans()
    {
        var userId = HttpContext.Session.GetInt32("UserId"); 
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
        try 
        {
            var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(_loanApiUrl);
            var myLoans = allLoans?.Where(l => l.UserId == userId).ToList() ?? new();
            return View(myLoans);
        }
        catch { return View(new List<LoanViewModel>()); }
    }

    [HttpPost]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("Username");
        if (userId == null) return RedirectToAction("Index", "Login");

        var client = _httpClientFactory.CreateClient();
        await client.PutAsJsonAsync($"{_loanApiUrl}/{id}", new { 
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
        await client.DeleteAsync($"{_loanApiUrl}/{id}");
        return RedirectToAction("MyLoans");
    }
}