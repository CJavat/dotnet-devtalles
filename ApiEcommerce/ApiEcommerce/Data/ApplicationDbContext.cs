using ApiEcommerce.Models;
using Microsoft.EntityFrameworkCore;

// Extender de DBContext que esa clase está en EntityFramework.
public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

  // Al agregar esto, EntityFramework creará la tabla en la DB.
  public DbSet<Category> Categories { get; set; }
  public DbSet<Product> Products { get; set; }
  public DbSet<User> Users { get; set; }
}