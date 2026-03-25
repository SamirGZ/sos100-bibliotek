using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Controllers;

public class LoginController : Controller
{
    private readonly UserApiService _userApiService;

    public LoginController(UserApiService userApiService) => _userApiService = userApiService;

    [HttpGet]
    public IActionResult Index() => View();
    
    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {
        var user = await _userApiService.LoginAndGetUserAsync(username, password);
    
        if (user != null)
        {
            // Nu kommer user.Id att ha rätt siffra (t.ex. 4) istället för 0
            HttpContext.Session.SetInt32("UserId", user.Id); 
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }
        
        ViewBag.Error = "Ogiltigt användarnamn eller lösenord.";
        return View();
    }
    
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); 
        return RedirectToAction("Index", "Home");
    }
}