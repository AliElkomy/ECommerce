using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Entities;
using OrderService.Api.Sagas;
using Shared.Contracts.Events;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IHttpClientFactory _http;

    public OrdersController(OrderDbContext db, IHttpClientFactory http)
    {
        _db = db;
        _http = http;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _db.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync();
        var sagaStates = await _db.OrderStates.ToListAsync();
        var result = orders.Select(o =>
        {
            var saga = sagaStates.FirstOrDefault(s => s.CorrelationId == o.Id);
            return new
            {
                o.Id,
                o.ProductId,
                o.UserId,
                o.Quantity,
                o.UnitPrice,
                Status = saga?.CurrentState ?? o.Status,
                o.CreatedAt
            };
        });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var client = _http.CreateClient();
        var productRes = await client.GetAsync($"http://localhost:5001/api/products/{dto.ProductId}");
        if (!productRes.IsSuccessStatusCode)
            return BadRequest("Product not found");
        var product = JsonSerializer.Deserialize<ProductDto>(await productRes.Content.ReadAsStringAsync(), opts);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            UserId = dto.UserId,
            Quantity = dto.Quantity,
            UnitPrice = product.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var orderEvent = new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = order.ProductId,
            UserId = order.UserId,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            PlacedAt = order.CreatedAt
        };

        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Orders.Add(order);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(OrderPlacedEvent),
            Payload = JsonSerializer.Serialize(orderEvent),
            CreatedAt = DateTime.UtcNow,
            Processed = false
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(order.Id);
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> RetryOrder(Guid id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound("Order not found");

        var saga = await _db.OrderStates.FindAsync(id);
        if (saga != null)
        {
            _db.OrderStates.Remove(saga);
        }

        var orderEvent = new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = order.ProductId,
            UserId = order.UserId,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            PlacedAt = order.CreatedAt
        };

        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(OrderPlacedEvent),
            Payload = JsonSerializer.Serialize(orderEvent),
            CreatedAt = DateTime.UtcNow,
            Processed = false
        });

        await _db.SaveChangesAsync();

        return Ok("Order retry queued");
    }
}

public class CreateOrderDto
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
