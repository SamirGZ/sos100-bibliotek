using Microsoft.AspNetCore.Mvc;
using Catalogue_service.Models;
using Catalogue_service.Data;

namespace Catalogue_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreateBookController : ControllerBase
{
    private readonly BookCatalogueDbContext _dbContext;

    public CreateBookController(BookCatalogueDbContext dbContext)
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

    [HttpPost]
    public async Task<IActionResult> CreateBook(BookCatalogue book)
    {
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookById), new { Id = book.Id }, book);
    }
}
    