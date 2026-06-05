using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Data;
using UserService.Api.Entities;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;

    public UsersController(UserDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users.OrderBy(u => u.Name).ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Type = dto.Type,
            Address = dto.Address,
            Email = dto.Email,
            Tel = dto.Tel,
            City = dto.City,
            Currency = dto.Currency ?? "EGP"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Name = dto.Name;
        user.Type = dto.Type;
        user.Address = dto.Address;
        user.Email = dto.Email;
        user.Tel = dto.Tel;
        user.City = dto.City;
        user.Currency = dto.Currency ?? "EGP";

        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public class CreateUserDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Tel { get; set; }
    public string City { get; set; }
    public string Currency { get; set; }
}
