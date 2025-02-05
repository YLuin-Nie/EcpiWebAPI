using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

[Route("api/[controller]")]
[ApiController]
public class AutoLoginController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AutoLoginController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 Manually trigger auto-login
    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerAutoLogin()
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");

        if (user != null)
        {
            return Ok(new { message = $"Auto-login successful for {user.UserName} at {DateTime.Now}" });
        }

        return NotFound(new { message = "User not found" });
    }
}
