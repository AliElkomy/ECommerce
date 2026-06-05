namespace UserService.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Tel { get; set; }
    public string City { get; set; }
    public string Currency { get; set; } = "EGP";
}
