using System.Collections.Concurrent;

namespace NotificationService.Worker.Data;

public class Notification
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationStore
{
    private readonly ConcurrentBag<Notification> _notifications = new();

    public void Add(Notification notification)
    {
        _notifications.Add(notification);
    }

    public List<Notification> GetAll()
    {
        return _notifications.OrderByDescending(n => n.CreatedAt).ToList();
    }
}
