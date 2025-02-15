using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace EcpiMauiClient
{
    public partial class MainPage : ContentPage
    {
        private HubConnection _connection;
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
        private static string _clientId = $"Client_{Guid.NewGuid().ToString().Substring(0, 4)}"; // Unique identifier per instance

        public MainPage()
        {
            InitializeComponent();
            MessagesList.ItemsSource = Messages;
            ConnectToSignalR();
        }

        private async void ConnectToSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5182/ecpihub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"📩 Received message from {user}: {message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Add($"{user}: {message}");
                });
            });

            _connection.On<string, string>("TokenCreated", (user, token) =>
            {
                Console.WriteLine($"🔑 Received TokenCreated event for {user}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Add($"🔑 Token created for {user}: Token is valid for one hour.");
                });
            });

            try
            {
                await _connection.StartAsync();
                Console.WriteLine("✅ Connected to ECPIHub.");
                MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Connected to ECPIHub.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error connecting to SignalR: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = $"Error: {ex.Message}");
            }
        }

        private async void SendMessage(object sender, EventArgs e)
        {
            using var httpClient = new HttpClient();
            var message = new
            {
                User = _clientId, // Unique identifier for each instance
                Text = MessageEntry.Text
            };

            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5182/api/messages/send", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Sent message: {message.Text}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Add($"You ({_clientId}): {message.Text}");
                    MessageEntry.Text = ""; // Clear input field
                });
            }
            else
            {
                Console.WriteLine("❌ Error sending message.");
                StatusLabel.Text = "Error sending message.";
            }
        }
    }
}
