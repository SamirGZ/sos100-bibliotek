using Microsoft.EntityFrameworkCore;
using NotificationsService.Models;

namespace NotificationsService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications { get; set; }
}