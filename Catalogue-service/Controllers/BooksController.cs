using Microsoft.AspNetCore.Mvc;
using Catalogue_service.Models;
using Catalogue_service.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalogue_service.Controllers;

[ApiController]
[Route("api/[controller]")] // Detta blir api/Books
public class BooksController : ControllerBase
{
    private readonly BookCatalogueDbContext _dbContext;
    
    public BooksController(BookCatalogueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookCatalogue>>> GetBooks()
    {
        return await _dbContext.Books.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookCatalogue>> GetBook(int id)
    {
        var book = await _dbContext.Books.FindAsync(id);
        if (book == null) return NotFound();
        return book;
    }

    [HttpPost]
    public async Task<IActionResult> PostBooks([FromBody] BookCatalogue book)
    {
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
        return Ok(book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, [FromBody] BookCatalogue book)
    {
        if (id != book.Id) return BadRequest();
        _dbContext.Entry(book).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _dbContext.Books.FindAsync(id);
        if (book == null) return NotFound();
        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}