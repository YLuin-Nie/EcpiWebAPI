using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using YourNamespace.Models;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 Public access for login (No authentication required)
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] UserTable user)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
        if (existingUser == null)
            return Unauthorized("Invalid username or password.");

        if (!PasswordHasher.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
            return Unauthorized("Invalid username or password.");

        return Ok(new { message = "Login successful!" });
    }

    // 🔒 Secure the user creation endpoint (Authentication required)
    [Authorize]
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] UserTable user)
    {
        if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.PasswordHash))
            return BadRequest("Username and password are required.");

        user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);

        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok(new { message = "User created successfully" });
    }
}
