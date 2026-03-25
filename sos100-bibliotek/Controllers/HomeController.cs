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
            
            // Hämtar URL från appsettings.json, annars använd reserven
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
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var username = HttpContext.Session.GetString("Username") ?? "Anonym";
            var client = _httpClientFactory.CreateClient();

            var loanRequest = new { 
                UserId = userId, Username = username, BookTitle = bookTitle,
                IsReturned = false, LoanDate = DateTime.Now, ReturnDate = DateTime.Now.AddDays(30)
            };

            try 
            {
                var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan" : $"{_loanApiUrl}/api/loan";
                var response = await client.PostAsJsonAsync(requestUrl, loanRequest);
                
                if (response.IsSuccessStatusCode) TempData["SuccessMessage"] = $"Boken '{bookTitle}' har lånats!";
                else TempData["ErrorMessage"] = "API svarade med fel vid lån.";
            }
            catch { TempData["ErrorMessage"] = "Kunde inte ansluta till lånetjänsten."; }

            return RedirectToAction("Index", "Books");
        }

        // --- 2. VISA MINA LÅN ---
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
            catch { return View(new List<LoanViewModel>()); }
        }

        // --- 3. ÅTERLÄMNA BOK (Denna saknades!) ---
        [HttpPost]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var client = _httpClientFactory.CreateClient();
            try 
            {
                var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan/{id}" : $"{_loanApiUrl}/api/loan/{id}";
                
                // Vi skickar ett uppdaterat objekt till API:et
                var response = await client.PutAsJsonAsync(requestUrl, new { Id = id, IsReturned = true });
                
                if (response.IsSuccessStatusCode) TempData["SuccessMessage"] = "Boken har återlämnats!";
            }
            catch { TempData["ErrorMessage"] = "Kunde inte kontakta API för återlämning."; }

            return RedirectToAction("MyLoans");
        }

        // --- 4. RADERA LÅN (Denna saknades också!) ---
        [HttpPost] 
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var client = _httpClientFactory.CreateClient();
            try 
            {
                var requestUrl = _loanApiUrl.EndsWith("/") ? $"{_loanApiUrl}api/loan/{id}" : $"{_loanApiUrl}/api/loan/{id}";
                await client.DeleteAsync(requestUrl);
            }
            catch { /* Ignorera fel vid radering */ }

            return RedirectToAction("MyLoans");
        }
    }