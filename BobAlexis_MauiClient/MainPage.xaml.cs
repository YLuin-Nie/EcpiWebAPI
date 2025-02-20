using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Internal;

namespace BobAlexis_MauiClient
{
    public partial class MainPage : ContentPage
    {
        private HubConnection? _hubConnection;
        private bool IsLoggedIn = false; // Track login status
        private readonly HttpClient _httpClient = new HttpClient(); // Reuse HttpClient

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

                // Mark user as logged in
                IsLoggedIn = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatusLabel.Text = $"✅ Logged in as {user}";
                    ConnectionStatusLabel.TextColor = Colors.Green;
                });
            });

            try
            {
                await _hubConnection.StartAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (IsLoggedIn)
                    {
                        ConnectionStatusLabel.Text = "✅ Connected to ecpihub & Logged In";
                        ConnectionStatusLabel.TextColor = Colors.Green;
                    }
                    else
                    {
                        ConnectionStatusLabel.Text = "🔌 Connected to ecpihub (Not Logged In)";
                        ConnectionStatusLabel.TextColor = Colors.Orange;
                    }
                });
            }
            catch (Exception ex)
            {
                IsLoggedIn = false; // Reset login state on failure

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatusLabel.Text = "❌ Failed to Connect";
                    ConnectionStatusLabel.TextColor = Colors.Red;
                    AppendMessage($"❌ Connection Error: {ex.Message}");
                });
            }
        }

        private async void Login_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("📢 Login button was clicked!");

            if (UsernameEntry == null || PasswordEntry == null || LoginStatusLabel == null)
            {
                Console.WriteLine("⚠️ UI elements not initialized properly!");
                return;
            }

            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("⚠️ Missing username or password");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoginStatusLabel.Text = "⚠️ Username and password are required.";
                    LoginStatusLabel.TextColor = Colors.Red;
                });
                return;
            }

            // 🔹 MANUAL JSON STRING FORMATTING
            var jsonContent = new StringContent(
                $"{{\"UserName\": \"{username}\", \"PasswordHash\": \"{password}\"}}",
                Encoding.UTF8, "application/json"
            );

            Console.WriteLine($"📢 Sending login request for {username}");
            try
            {
                var response = await _httpClient.PostAsync("http://localhost:5182/api/User/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var token = responseData.GetProperty("token").GetString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        try
                        {
                            await SecureStorage.SetAsync("jwt_token", token);
                            Console.WriteLine($"✅ Token saved in SecureStorage: {token}");

                            // Retrieve immediately to confirm it's stored
                            var checkToken = await SecureStorage.GetAsync("jwt_token");
                            Console.WriteLine($"🔍 Retrieved Token after saving: {checkToken}");

                            if (string.IsNullOrEmpty(checkToken))
                            {
                                Console.WriteLine("❌ SecureStorage failed to save token. Using Preferences as backup.");
                                Preferences.Set("jwt_token", token);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ SecureStorage error: {ex.Message}");
                            Preferences.Set("jwt_token", token);
                        }

                        IsLoggedIn = true;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoginStatusLabel.TextColor = Colors.Green;
                            LoginStatusLabel.Text = "✅ Login successful!";
                            ConnectionStatusLabel.Text = $"✅ Logged in as {username}";
                            ConnectionStatusLabel.TextColor = Colors.Green;
                        });

                        AppendMessage($"🔑 Logged in as {username}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Login response did not contain a valid token.");
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoginStatusLabel.TextColor = Colors.Red;
                            LoginStatusLabel.Text = "❌ Login failed: No token received.";
                            IsLoggedIn = false;
                        });
                    }
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Login failed: {errorText}");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoginStatusLabel.TextColor = Colors.Red;
                        LoginStatusLabel.Text = $"❌ Login failed: {errorText}";
                        IsLoggedIn = false;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login request failed: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoginStatusLabel.Text = "❌ Error connecting to the server.";
                    LoginStatusLabel.TextColor = Colors.Red;
                    IsLoggedIn = false;
                });
            }
        }

        private async void SendMessage_Clicked(object sender, EventArgs e)
        {
            var user = UserEntry?.Text;
            var message = MessageEntry?.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
            {
                AppendMessage("⚠️ Both fields are required.");
                return;
            }

            var token = await SecureStorage.GetAsync("jwt_token");
            if (string.IsNullOrEmpty(token))
            {
                token = Preferences.Get("jwt_token", null); // Check backup storage
            }

            Console.WriteLine($"🔍 Token retrieved before sending message: {token}");

            if (string.IsNullOrEmpty(token))
            {
                AppendMessage("⚠️ No authentication token found. Please log in first.");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent($"{{\"user\": \"{user}\", \"text\": \"{message}\"}}",
                                            Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:5182/api/messages/send", content);

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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var label = new Label
                {
                    Text = text,
                    FontSize = 14,
                    TextColor = Colors.Black
                };

                MessagesStack.Children.Add(label);
            });
        }
    }
}
