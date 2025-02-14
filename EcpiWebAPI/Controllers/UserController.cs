using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcpiWebAPI.Models;
using EcpiWebAPI.Services;
using EcpiWebAPI.Attributes;

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
    public IActionResult Login([FromBody] UserTable user)
    {
        var token = _userService.Login(user);
        if (token == null)
            return Unauthorized("Invalid username or password.");

        return Ok(new { token });
    }

    [Authorize]
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] UserTable user)
    {
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

