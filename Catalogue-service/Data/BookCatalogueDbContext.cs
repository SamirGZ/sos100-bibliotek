using Catalogue_service.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalogue_service.Data;

public class BookCatalogueDbContext: DbContext
{
    public BookCatalogueDbContext(DbContextOptions<BookCatalogueDbContext> options)
        : base(options) { }
    
    public DbSet<BookCatalogue> Books { get; set; }
}