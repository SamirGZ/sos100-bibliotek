using Microsoft.AspNetCore.Mvc;
using Catalogue_service.Models;
using Catalogue_service.Data;

namespace Catalogue_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateBookController : ControllerBase
{
    private readonly BookCatalogueDbContext _dbContext;

    public UpdateBookController(BookCatalogueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetBookById(int Id)
    {
        var book = await _dbContext.Books.FindAsync(Id);

        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPut("{Id}")]
    public async Task<IActionResult> UpdateBook( int Id, BookCatalogue UpdatedBook)
    {
        if (Id != UpdatedBook.Id)
        {
            return BadRequest();
        }

        var book = await _dbContext.Books.FindAsync(Id);

        if (book == null)
        {
            return NotFound();
        }

        book.Title = UpdatedBook.Title;
        book.Author = UpdatedBook.Author;
        book.Isbn = UpdatedBook.Isbn;
        book.Publisher = UpdatedBook.Publisher;
        book.Year = UpdatedBook.Year;
        book.Pages = UpdatedBook.Pages;
        book.Language = UpdatedBook.Language;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}