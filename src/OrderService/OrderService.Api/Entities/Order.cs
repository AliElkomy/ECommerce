namespace OrderService.Api.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
