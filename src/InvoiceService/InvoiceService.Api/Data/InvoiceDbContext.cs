using Microsoft.EntityFrameworkCore;
using InvoiceService.Api.Entities;

namespace InvoiceService.Api.Data;

public class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ProcessedEvent>(e =>
        {
            e.HasKey(x => x.EventId);
        });
    }
}
