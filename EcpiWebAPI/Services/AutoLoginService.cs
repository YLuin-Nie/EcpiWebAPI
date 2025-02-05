using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;  // 🔹 Replace 'YourNamespace' with your actual project namespace
using BCrypt.Net;

public class AutoLoginService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private const string AutoLoginUsername = "testuser";
    private const string AutoLoginPassword = "Password123"; // 🔹 Change to the real password for auto-login

    public AutoLoginService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 🔹 Check if the user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == AutoLoginUsername);

                if (user != null)
                {
                    // 🔹 Verify the password
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(AutoLoginPassword, user.PasswordHash);

                    if (isPasswordCorrect)
                    {
                        Console.WriteLine($"✅ Auto-login successful for {user.UserName} at {DateTime.Now}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Auto-login failed: Incorrect password for {user.UserName}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Auto-login failed: User '{AutoLoginUsername}' not found.");
                }
            }

            // 🔹 Wait 5 minutes before running again
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
