using MassTransit;
using Microsoft.EntityFrameworkCore;
using InvoiceService.Api.Data;
using InvoiceService.Api.Entities;
using Shared.Contracts.Commands;
using Shared.Contracts.Events;

namespace InvoiceService.Api.Consumers;

public class InvoiceConsumer : IConsumer<CreateInvoiceCommand>
{
    private readonly InvoiceDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public InvoiceConsumer(InvoiceDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CreateInvoiceCommand> context)
    {
        var msg = context.Message;

        var duplicate = await _db.ProcessedEvents.AnyAsync(e => e.EventId == msg.OrderId);
        if (duplicate) return;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = msg.OrderId,
            UserId = msg.UserId,
            Amount = msg.Quantity * msg.UnitPrice,
            CreatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(invoice);
        _db.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = msg.OrderId,
            ProcessedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        await _publishEndpoint.Publish(new InvoiceCreatedEvent
        {
            OrderId = msg.OrderId,
            InvoiceId = invoice.Id,
            Amount = invoice.Amount,
            CreatedAt = invoice.CreatedAt
        });
    }
}
