using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using System.Net.Http.Json;

namespace sos100_bibliotek.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ApiUrl = "http://localhost:5029/api/loan";

    public HomeController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public IActionResult Index() => View();

    [HttpPost] // CREATE (Anropas från Katalogen)
    public async Task<IActionResult> BorrowBook(int bookId, string bookTitle)
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(ApiUrl, new { UserId = userId, BookId = bookId, BookTitle = bookTitle });
        return RedirectToAction("MyLoans");
    }

    public async Task<IActionResult> MyLoans() // READ
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 1; 
        var client = _httpClientFactory.CreateClient();
        var allLoans = await client.GetFromJsonAsync<List<LoanViewModel>>(ApiUrl);
        return View(allLoans?.Where(l => l.UserId == userId).ToList() ?? new List<LoanViewModel>());
    }

    [HttpPost] // UPDATE (Markera som återlämnad)
    public async Task<IActionResult> ReturnBook(int id)
    {
        var client = _httpClientFactory.CreateClient();
        await client.PutAsJsonAsync($"{ApiUrl}/{id}", new { IsReturned = true });
        return RedirectToAction("MyLoans");
    }

    [HttpPost] // DELETE (Ta bort permanent)
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var client = _httpClientFactory.CreateClient();
        await client.DeleteAsync($"{ApiUrl}/{id}");
        return RedirectToAction("MyLoans");
    }
}