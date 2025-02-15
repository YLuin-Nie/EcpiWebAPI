using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using EcpiWebAPI.Hubs;
using System.Threading.Tasks;

[Route("api/messages")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IHubContext<ECPIHub> _hubContext;

    public MessagesController(IHubContext<ECPIHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
    {
        if (string.IsNullOrWhiteSpace(message.User) || string.IsNullOrWhiteSpace(message.Text))
        {
            return BadRequest("Invalid message.");
        }

        Console.WriteLine($"📩 Message received from {message.User}: {message.Text}"); // Debugging log

        // Send message to all connected clients via the hub
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Text);
        return Ok(new { Status = "Message sent successfully." });
    }
}

public class MessageDto
{
    public string? User { get; set; }
    public string? Text { get; set; }
}
