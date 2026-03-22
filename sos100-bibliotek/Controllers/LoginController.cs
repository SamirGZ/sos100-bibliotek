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
        bool success = await _userApiService.LoginAsync(username, password);
        if (!success)
        {
            ViewBag.Error = "Ogiltigt användarnamn eller lösenord.";
            return View();
        }

        HttpContext.Session.SetString("Username", username);
        return RedirectToAction("Index", "Home");
    }
}