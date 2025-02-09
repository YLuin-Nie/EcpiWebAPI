using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using YourNamespace.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;

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
        if (existingUser == null || !PasswordHasher.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("Beans_Threads_Beans_Threads_Beans_Threads"); // Match with appsettings.json
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }),
            Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
            Issuer = "yourdomain.com",
            Audience = "yourdomain.com",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new
        {
            token = tokenHandler.WriteToken(token)
        });
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
