namespace Shared.Contracts.Events;

public class InvoiceCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
