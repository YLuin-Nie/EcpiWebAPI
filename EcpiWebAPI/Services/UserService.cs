using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration; // Add this
using YourNamespace.Models;

namespace YourNamespace.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtKey;

        // Inject IConfiguration to access appsettings.json
        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        }

        public string Login(UserTable user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (existingUser == null || !PasswordHasher.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                return string.Empty; // Return empty string for failed login

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

            return tokenHandler.WriteToken(token);
        }

        public bool CreateUser(UserTable user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.PasswordHash))
                return false; // Indicate failure

            user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }
    }
}
