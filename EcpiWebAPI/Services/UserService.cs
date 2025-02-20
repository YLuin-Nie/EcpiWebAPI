using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using EcpiWebAPI.Models;
using EcpiWebAPI.Hubs;
using System.Threading.Tasks;

namespace EcpiWebAPI.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtKey;
        private readonly IHubContext<ECPIHub> _hubContext; // Inject SignalR Hub

        public UserService(ApplicationDbContext context, IConfiguration configuration, IHubContext<ECPIHub> hubContext)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            _hubContext = hubContext;
        }

        public async Task<string> Login(UserTable user) // 🔹 Ensure method is async Task<string>
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (existingUser == null || !PasswordHasher.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "yourdomain.com",
                Audience = "yourdomain.com",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // 🔹 Notify all clients that a new token has been created
            Console.WriteLine($"📢 TokenCreated event sent for {user.UserName}: {jwtToken}");
            await _hubContext.Clients.All.SendAsync("TokenCreated", user.UserName, jwtToken); // Ensure await is inside an async method

            return jwtToken;
        } 

        public bool CreateUser(UserTable user) // 🔹 Now correctly outside the Login() method
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }
    }
}
