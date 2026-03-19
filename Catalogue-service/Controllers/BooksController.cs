using Microsoft.AspNetCore.Mvc;
using Catalogue_service.Models;
using Catalogue_service.Data;

namespace Catalogue_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookCatalogueDbContext _dbContext;
    
    public BooksController(BookCatalogueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public BookCatalogue[] GetBooks()
    {
        BookCatalogue[] books = _dbContext.Books.ToArray();
        return books;
    }

    [HttpPost]
    public async Task <IActionResult> PostBooks(BookCatalogue book)
    {
        _dbContext.Books.Add(book);
       await _dbContext.SaveChangesAsync();
        
        return Ok(book);
    }
}