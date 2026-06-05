namespace Shared.Contracts.Events;

public class InvoiceFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}
