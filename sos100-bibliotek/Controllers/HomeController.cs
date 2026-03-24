using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using System.Net.Http.Json;

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
        if (userId == null) return RedirectToAction("Index");

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(ApiUrl, new { 
            UserId = userId, 
            BookId = bookId, 
            BookTitle = bookTitle 
        });

        if (response.IsSuccessStatusCode)
            TempData["Message"] = $"Lånet för '{bookTitle}' är klart!";
        else
            TempData["Error"] = "Du har redan lånat denna bok.";

        return RedirectToAction("MyLoans");
    }

    public async Task<IActionResult> MyLoans()
    {
        var userId = HttpContext.Session.GetInt32("UserId"); 
        if (userId == null) return RedirectToAction("Index");

        var client = _httpClientFactory.CreateClient();
        try 
        {
            var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(ApiUrl);
            var myLoans = allLoans?.Where(l => l.UserId == userId).ToList() ?? new List<LoanViewModel>();
            return View(myLoans);
        }
        catch { return View(new List<LoanViewModel>()); }
    }

    [HttpPost]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var client = _httpClientFactory.CreateClient();
        
        // Vi skickar med UserId så att notis-tjänsten vet vem som lämnat tillbaka
        var response = await client.PutAsJsonAsync($"{ApiUrl}/{id}", new { 
            IsReturned = true,
            UserId = userId 
        });

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Boken har lämnats tillbaka!";
            
        return RedirectToAction("MyLoans");
    }

    [HttpPost] 
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var client = _httpClientFactory.CreateClient();
        await client.DeleteAsync($"{ApiUrl}/{id}");
        TempData["Message"] = "Lånet har tagits bort.";
        return RedirectToAction("MyLoans");
    }
}