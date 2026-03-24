using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class RegisterController : Controller
{
    private readonly UserApiService _userApiService;

    public RegisterController(UserApiService userApiService)
    {
        _userApiService = userApiService;
    }

    [HttpGet]
    public IActionResult Index() => View();
    
    /// Registrerar en ny användare och etablerar en fullständig session.
    [HttpPost]
    public async Task<IActionResult> Index(string username, string password, string email)
    {
        bool success = await _userApiService.RegisterAsync(username, password, email);
        if (!success)
        {
            ViewBag.Error = "Användarnamnet eller e-postadressen är redan registrerad.";
            return View();
        }

        // Efter lyckad registrering hämtas profilen för att erhålla UserId
        var user = await _userApiService.GetUserAsync(username);
        if (user != null)
        {
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("UserId", user.Id);
        }

        return RedirectToAction("Index", "Home");
    }
}