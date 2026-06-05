using MassTransit;
using NotificationService.Worker.Data;
using Shared.Contracts.Events;

namespace NotificationService.Worker.Consumers;

public class NotificationConsumer : IConsumer<InvoiceCreatedEvent>
{
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly NotificationStore _store;

    public NotificationConsumer(ILogger<NotificationConsumer> logger, NotificationStore store)
    {
        _logger = logger;
        _store = store;
    }

    public Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        var msg = context.Message;
        var message = $"Invoice {msg.InvoiceId} created for order {msg.OrderId}, amount {msg.Amount:C}.";

        _store.Add(new Notification
        {
            Id = Guid.NewGuid(),
            InvoiceId = msg.InvoiceId,
            OrderId = msg.OrderId,
            Amount = msg.Amount,
            Message = message,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("{Message}", message);
        return Task.CompletedTask;
    }
}
