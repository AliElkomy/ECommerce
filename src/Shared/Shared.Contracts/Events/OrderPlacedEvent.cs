namespace Shared.Contracts.Events;

public class OrderPlacedEvent
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime PlacedAt { get; set; }
}
