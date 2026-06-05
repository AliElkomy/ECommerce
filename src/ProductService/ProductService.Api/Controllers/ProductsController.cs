using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.Entities;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _db;

    public ProductsController(ProductDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _db.Products.ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Price = dto.Price,
            Quantity = dto.Quantity
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPost("{id}/decrement")]
    public async Task<IActionResult> Decrement(Guid id)
    {
        var updated = await _db.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Quantity = Quantity - 1 WHERE Id = {0} AND Quantity > 0",
            id);

        if (updated == 0)
            return BadRequest("Product is out of stock");

        return Ok();
    }
}

public class CreateProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
