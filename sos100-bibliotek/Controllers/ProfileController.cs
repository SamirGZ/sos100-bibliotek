using Microsoft.AspNetCore.Mvc;

namespace sos100_bibliotek.Controllers;

public class ProfileController : Controller
{
    private readonly UserApiService _userApiService;

    public ProfileController(UserApiService userApiService)
    {
        _userApiService = userApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
            return RedirectToAction("Index", "Login");

        var user = await _userApiService.GetUserAsync(username);
        if (user == null)
            return RedirectToAction("Index", "Login");

        return View(user);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
            return RedirectToAction("Index", "Login");

        await _userApiService.DeleteUserAsync(username);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Login");
    }

    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword(string newPassword)
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
            return RedirectToAction("Index", "Login");

        bool success = await _userApiService.UpdatePasswordAsync(username, newPassword);
        if (!success)
        {
            ViewBag.Error = "Något gick fel, försök igen.";
            var user = await _userApiService.GetUserAsync(username);
            return View("Index", user);
        }

        ViewBag.Success = "Lösenordet har uppdaterats!";
        var updatedUser = await _userApiService.GetUserAsync(username);
        return View("Index", updatedUser);
    }
}