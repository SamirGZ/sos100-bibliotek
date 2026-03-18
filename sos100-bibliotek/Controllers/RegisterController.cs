using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password, string email)
    {
        bool success = await _userApiService.RegisterAsync(username, password, email);
        if (!success)
        {
            ViewBag.Error = "Användarnamnet är redan taget.";
            return View();
        }

        HttpContext.Session.SetString("Username", username);
        return RedirectToAction("Index", "Home");
    }
}