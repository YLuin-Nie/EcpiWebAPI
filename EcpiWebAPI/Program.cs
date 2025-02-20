using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using EcpiWebAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EcpiWebAPI.Services;
using EcpiWebAPI.Attributes;
using EcpiWebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Add database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 Register Background Service
builder.Services.AddHostedService<AutoLoginService>();

// Add services to the container.
builder.Services.AddControllers();

// Register UserService for dependency injection
builder.Services.AddScoped<UserService>();
builder.Services.AddSignalR(); // Add SignalR

// Register custom attribute for dependency injection
//builder.Services.AddSingleton<RestrictToUsersAttribute>();

// 🔹 Add CORS policy to allow requests from your MAUI app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://localhost:5182")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Console.WriteLine($"🔑 JWT Key: {builder.Configuration["Jwt:Key"]}");
Console.WriteLine($"🌍 Issuer: {builder.Configuration["Jwt:Issuer"]}");
Console.WriteLine($"🎯 Audience: {builder.Configuration["Jwt:Audience"]}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Apply the CORS policy
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<ECPIHub>("/ecpihub");

app.Run();