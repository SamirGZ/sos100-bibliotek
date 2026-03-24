using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Models;

namespace Bibliotek.LoanAPI.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanEvent> LoanEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vi säger till EF att UserId bara är ett vanligt heltal.
        // Inga kopplingar till andra tabeller = Inga Foreign Key-fel!
        modelBuilder.Entity<Loan>()
            .Property(l => l.UserId)
            .IsRequired();
            
        // Fixar kopplingen till historiken (denna vill vi ha kvar)
        modelBuilder.Entity<LoanEvent>()
            .HasOne(e => e.Loan)
            .WithMany(l => l.History)
            .HasForeignKey(e => e.LoanId);
    }
}