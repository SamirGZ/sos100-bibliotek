using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {
        // Vi hämtar hela användaren från UserAPI
        var user = await _userApiService.LoginAsync(username, password);
        
        if (user == null)
        {
            ViewBag.Error = "Ogiltigt användarnamn eller lösenord.";
            return View();
        }

        // Spara i Session för C#-sidan
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetInt32("UserId", user.Id);

        // Skicka med ID och namn till vyn så JavaScript kan spara dem i localStorage
        ViewBag.UserId = user.Id;
        ViewBag.Username = user.Username;

        // VIKTIGT: Vi returnerar View() istället för Redirect så att 
        // JavaScript-koden i Login.cshtml hinner köra och spara ID:t!
        return View();
    }
}