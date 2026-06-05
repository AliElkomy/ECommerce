namespace Shared.Contracts.Commands;

public class CreateInvoiceCommand
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
