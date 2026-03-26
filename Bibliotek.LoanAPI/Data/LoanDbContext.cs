using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Models;

namespace Bibliotek.LoanAPI.Data;

public class LoanDbContext : DbContext
{
    // Tar in db-inställningar via DI (t.ex. att vi kör SQLite)
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanEvent> LoanEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserId är bara en vanlig int. 
        // Vi skapar INGEN Foreign Key eftersom användarna ligger i en helt annan databas (UserAPI).
        modelBuilder.Entity<Loan>()
            .Property(l => l.UserId)
            .IsRequired();
            
        // Kopplar ihop historikloggen (LoanEvent) med det specifika lånet (1 till många)
        modelBuilder.Entity<LoanEvent>()
            .HasOne(e => e.Loan)
            .WithMany(l => l.History)
            .HasForeignKey(e => e.LoanId);
    }
}