using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceService.Api.Data;

namespace InvoiceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceDbContext _db;

    public InvoicesController(InvoiceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? orderId)
    {
        var query = _db.Invoices.AsQueryable();
        if (orderId.HasValue)
            query = query.Where(i => i.OrderId == orderId.Value);
        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }
}
