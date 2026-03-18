using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Models;

namespace Bibliotek.LoanAPI.Data;

public class LoanDbContext : DbContext
{
    // Konstruktor
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    // Tabellen för lånen i databasen
    public DbSet<Loan> Loans { get; set; }
    public DbSet<User> Users { get; set; }
    
    public DbSet<LoanEvent> LoanEvents { get; set; }
}