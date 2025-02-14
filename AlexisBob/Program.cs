using System;
using System.Threading.Tasks;
using Internal;
using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    static async Task Main()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5182/ecpihub") // Change URL if needed
            .WithAutomaticReconnect()
            .Build();

        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
        });

        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connected to ECPIHub.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        while (true)
        {
            Console.Write("Enter message: ");
            var message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) break;

            await connection.InvokeAsync("SendMessage", "Bob", message);
        }
    }
}
