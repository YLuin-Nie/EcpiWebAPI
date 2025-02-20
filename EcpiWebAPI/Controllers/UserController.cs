using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcpiWebAPI.Models;
using EcpiWebAPI.Services;
using EcpiWebAPI.Attributes;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserTable user) // ✅ Correct return type
    {
    //    Console.WriteLine($"📢 Received login attempt: {user.UserName}");
    //    Console.WriteLine($"📢 Received password: {user.PasswordHash}");

        var token = await _userService.Login(user);

        if (string.IsNullOrEmpty(token)) // ✅ Use string.IsNullOrEmpty to check if token is valid
            return Unauthorized("Invalid username or password.");

        return Ok(new { token });
    }

    [Authorize]
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] UserTable user)
    {
        Console.WriteLine($"📢 Received login attempt: {user.UserName}");
        Console.WriteLine($"📢 Received password: {user.PasswordHash}"); // TEMPORARY for debugging

        if (!_userService.CreateUser(user))
            return BadRequest("Username and password are required.");

        return Ok(new { message = "User created successfully" });
    }

    [RestrictToUsers("Bob", "Alexis")]
    [HttpGet("restricted")]
    public IActionResult RestrictedFunction()
    {
        return Ok(new { message = "Welcome, Bob or Alexis!" });
    }
}

