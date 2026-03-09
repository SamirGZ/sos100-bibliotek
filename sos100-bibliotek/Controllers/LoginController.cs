using Microsoft.AspNetCore.Mvc;

namespace sos100_bibliotek.Controllers;

public class LoginController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}