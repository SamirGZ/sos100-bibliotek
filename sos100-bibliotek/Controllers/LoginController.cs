using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class LoginController : Controller
{
    private readonly UserApiService _userApiService;

    public LoginController(UserApiService userApiService)
    {
        _userApiService = userApiService;
    }

    [HttpGet]
    public IActionResult Index() => View();
    
    /// Hanterar inloggningsförfrågan och etablerar användarsession.
    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {
        // Hämtar användarobjekt via UserApiService
        var user = await _userApiService.LoginAndGetUserAsync(username, password);
        
        if (user == null)
        {
            ViewBag.Error = "Ogiltigt användarnamn eller lösenord.";
            return View();
        }

        // Lagrar användaridentitet i sessionen för åtkomstkontroll och API-anrop
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetInt32("UserId", user.Id);

        return RedirectToAction("Index", "Home");
    }
    
    /// Avslutar sessionen och omdirigerar användaren till startsidan.
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}