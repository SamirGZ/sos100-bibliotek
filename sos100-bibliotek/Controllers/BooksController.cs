using Microsoft.AspNetCore.Mvc;
using sos100_bibliotek.Models;
using sos100_bibliotek.Services;

namespace sos100_bibliotek.Controllers;

public class BooksController: Controller
{
    private CatalogueService _catalogueService;

    public BooksController(CatalogueService catalogueService)
    {
        _catalogueService = catalogueService;
    }

    public async Task<IActionResult> Index()
    {
        BookCatalogue[] bookCatalogues = await _catalogueService.GetBookCatalogue();

        return View(bookCatalogues);
    }
}