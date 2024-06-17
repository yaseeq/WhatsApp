using Microsoft.EntityFrameworkCore;
namespace ConsoleApp1.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Booking> bookingtable { get; set; }
}