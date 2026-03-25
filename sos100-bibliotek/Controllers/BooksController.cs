using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class BooksController : Controller
{
    private readonly CatalogueService _catalogueService;

    public BooksController(CatalogueService catalogueService)
    {
        _catalogueService = catalogueService;
    }

    // Visar boklistan
    public async Task<IActionResult> Index()
    {
        var books = await _catalogueService.GetBookCatalogue();
        return View(books);
    }

    public IActionResult CreateBook() => View();

    [HttpPost]
    public async Task<IActionResult> CreateBook(BookCatalogue book)
    {
        if (ModelState.IsValid)
        {
            var success = await _catalogueService.CreateBook(book);
            if (success) return RedirectToAction("Index");
        }
        return View(book);
    }
}