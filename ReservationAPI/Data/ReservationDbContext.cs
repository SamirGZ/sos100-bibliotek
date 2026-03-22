using Microsoft.EntityFrameworkCore;
using ReservationApi.Models;

namespace ReservationApi.Data;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reservation> Reservations { get; set; }
}