using Microsoft.AspNetCore.Mvc;
using Catalogue_service.Models;
using Catalogue_service.Data;

namespace Catalogue_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeleteBookController : ControllerBase
{
    private readonly BookCatalogueDbContext _dbContext;

    public DeleteBookController(BookCatalogueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpDelete("{Id}")]
    public async Task<IActionResult> DeleteBook(int Id)
    {
        var book = await _dbContext.Books.FindAsync(Id);

        if (book == null)
        {
            return NotFound();
        }
        
        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}