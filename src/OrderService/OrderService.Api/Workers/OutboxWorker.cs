using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Entities;
using Shared.Contracts.Events;

namespace OrderService.Api.Workers;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var pending = await db.OutboxMessages
                    .Where(m => !m.Processed)
                    .ToListAsync(ct);

                foreach (var msg in pending)
                {
                    var @event = JsonSerializer.Deserialize<OrderPlacedEvent>(msg.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (@event != null)
                    {
                        await publishEndpoint.Publish(@event, ct);
                    }
                    msg.Processed = true;
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OutboxWorker error: {ex.Message}");
            }

            await Task.Delay(5000, ct);
        }
    }
}
