namespace Shared.Contracts.Commands;

public class NotifyUserCommand
{
    public Guid UserId { get; set; }
    public string Reason { get; set; }
}
