using Microsoft.EntityFrameworkCore;
using ProductService.Api.Entities;

namespace ProductService.Api.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}
