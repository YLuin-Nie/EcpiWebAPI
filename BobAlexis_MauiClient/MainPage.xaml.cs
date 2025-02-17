using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Net.Http;
using System.Text;
using System.Reflection.Emit;

namespace BobAlexis_MauiClient
{
    public partial class MainPage : ContentPage
    {
        private HubConnection? _hubConnection;

        public MainPage()
        {
            InitializeComponent();
            ConnectToHub();
        }

        private async void ConnectToHub()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5182/ecpihub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AppendMessage($"{user}: {message}");
                });
            });

            _hubConnection.On<string, string>("TokenCreated", (user, token) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AppendMessage($"🔑 Token Created for {user}");
                });
            });

            try
            {
                await _hubConnection.StartAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatusLabel.Text = "Connected to ecpihub";
                    ConnectionStatusLabel.TextColor = Colors.Green;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatusLabel.Text = "Failed to Connect";
                    ConnectionStatusLabel.TextColor = Colors.Red;
                    AppendMessage($"❌ Connection Error: {ex.Message}");
                });
            }
        }

        private async void SendMessage_Clicked(object sender, EventArgs e)
        {
            var user = UserEntry.Text;
            var message = MessageEntry.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
            {
                AppendMessage("⚠️ Both fields are required.");
                return;
            }

            var httpClient = new HttpClient();
            var content = new StringContent($"{{\"user\": \"{user}\", \"text\": \"{message}\"}}",
                                            Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5182/api/messages/send", content);
            if (response.IsSuccessStatusCode)
            {
                AppendMessage($"📤 {user} sent: {message}");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                AppendMessage($"❌ Failed to send message: {errorMessage}");
            }
        }

        private void AppendMessage(string text)
        {
            var label = new Microsoft.Maui.Controls.Label // ✅ Use fully qualified namespace
            {
                Text = text,
                FontSize = 14,
                TextColor = Colors.Black
            };

            MessagesStack.Children.Add(label);
        }

    }
}
