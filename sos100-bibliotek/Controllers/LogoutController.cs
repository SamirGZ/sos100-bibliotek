using Microsoft.AspNetCore.Mvc;

namespace sos100_bibliotek.Controllers;

public class LogoutController : Controller
{
    public IActionResult Index()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");
    }
}