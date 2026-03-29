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

    // Visar det tomma formuläret för att lägga till en ny bok
    public IActionResult CreateBook() => View();

    [HttpPost]
    public async Task<IActionResult> CreateBook(BookCatalogue book)
    {
        // Kollar automatiskt att alla [Required]-fält och liknande i modellen är korrekt ifyllda 
        // innan vi ens försöker anropa API:et. Sparar onödiga nätverksanrop
        if (ModelState.IsValid)
        {
            var success = await _catalogueService.CreateBook(book);
        
            // Gick det bra i API:et skickas vi tillbaka till boklistan
            if (success) return RedirectToAction("Index");
        }
    
        // Viktig UX-detalj: Om valideringen eller API:et misslyckas returnerar vi vyn MED bok-objektet.
        // Då slipper användaren fylla i alla fält på nytt
        return View(book);
    }

    [HttpGet]
    public async Task<IActionResult> EditBook(int id)
    {
        var book = await _catalogueService.UpdateBookById(id);
        if (book == null)
        {
            TempData["ErrorMessage"] = "Kunde inte hitta boken att redigera.";
            return RedirectToAction("Index");
        }

        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> EditBook(BookCatalogue book)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        var success = await _catalogueService.UpdateBook(book);
        if (!success)
        {
            TempData["ErrorMessage"] = "Kunde inte uppdatera boken.";
            return View(book);
        }

        TempData["SuccessMessage"] = "Boken uppdaterades.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var success = await _catalogueService.DeleteBookById(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Boken togs bort." : "Kunde inte ta bort boken.";

        return RedirectToAction("Index");
    }
}