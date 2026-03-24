using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class ProfileController : Controller
{
    private readonly UserApiService _userApiService;

    public ProfileController(UserApiService userApiService)
    {
        _userApiService = userApiService;
    }
    
    /// Hämtar och visar användarprofilen baserat på sessionens användarnamn.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Index", "Login");

        var user = await _userApiService.GetUserAsync(username);
        if (user == null)
            return RedirectToAction("Index", "Login");

        return View(user);
    }
    
    /// Tar bort användarkontot och rensar aktuell session.
    [HttpPost("delete")]
    public async Task<IActionResult> Delete()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Index", "Login");

        await _userApiService.DeleteUserAsync(username);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");
    }
    
    /// Uppdaterar lösenordet för den inloggade användaren.
    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword(string newPassword)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Index", "Login");

        bool success = await _userApiService.UpdatePasswordAsync(username, newPassword);
        
        // Hämtar aktuell profilinformation för att bibehålla vyn vid svar
        var user = await _userApiService.GetUserAsync(username);

        if (!success)
        {
            ViewBag.Error = "Uppdateringen misslyckades. Vänligen kontrollera uppgifterna.";
            return View("Index", user);
        }

        ViewBag.Success = "Lösenordet har uppdaterats korrekt.";
        return View("Index", user);
    }
}