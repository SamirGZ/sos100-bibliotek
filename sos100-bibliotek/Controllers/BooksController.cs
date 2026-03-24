using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class BooksController: Controller
{
    private readonly CatalogueService _catalogueService;

    public BooksController(CatalogueService catalogueService)
    {
        _catalogueService = catalogueService;
    }

    public async Task<IActionResult> Index()
    {
        var bookCatalogue= await _catalogueService.GetBookCatalogue();

        return View(bookCatalogue);
    }

    [HttpGet]
    public async Task<IActionResult> EditBook(int Id)
    {
        var editBook = await _catalogueService.UpdateBookById(Id);

        if (editBook == null)
        {
            return NotFound();
        }
        
        return View(editBook);
    }

    [HttpPost]
    public async Task<IActionResult> EditBook(BookCatalogue bookCatalogue)
    {
        if (!ModelState.IsValid)
        {
            return View(bookCatalogue);
        }

        var Success = await _catalogueService.UpdateBook(bookCatalogue);

        if (!Success)
        {
            ModelState.AddModelError("", "Kunde inte uppdatera boken");
            
            return View(bookCatalogue);
        }
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBook(int Id)
    {
        var Success = await _catalogueService.DeleteBookById(Id);

        if (!Success)
        {
            TempData["ErrorMessage"] = "Kunde inte ta bort boken";
        }
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult CreateBook()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook(BookCatalogue bookCatalogue)
    {
        if (!ModelState.IsValid)
        {
            return View(bookCatalogue);
        }

        var Success = await _catalogueService.CreateBook(bookCatalogue);

        if (!Success)
        {
            ModelState.AddModelError("", "Kunde inte skapa boken");
            
            return View(bookCatalogue);
        }
        
        return RedirectToAction("Index");
    }
}