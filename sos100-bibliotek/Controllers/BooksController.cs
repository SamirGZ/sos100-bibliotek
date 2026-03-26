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
}