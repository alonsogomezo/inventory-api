using Microsoft.EntityFrameworkCore;

class AppDbContext : DbContext
{
    public DbSet<Producto> Productos { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }
}