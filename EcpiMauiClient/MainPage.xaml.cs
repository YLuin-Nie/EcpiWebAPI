using Microsoft.AspNetCore.SignalR.Client; // Import SignalR client for real-time communication
using System;
using System.Collections.ObjectModel; // Collection that updates UI automatically when changed
using System.Net.Http; // Used for sending HTTP requests
using System.Text; // Provides encoding utilities for text
using System.Text.Json; // JSON serialization utilities
using System.Threading.Tasks; // Supports asynchronous programming
using Microsoft.Maui.Controls; // MAUI UI framework
using Internal;

namespace EcpiMauiClient // Namespace for organizing the application
{
    public partial class MainPage : ContentPage // MainPage inherits from ContentPage (a UI page in MAUI)
    {
        private HubConnection _connection; // SignalR connection instance
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>(); // Collection bound to UI to display messages dynamically
        private static string _clientId = $"Client_{Guid.NewGuid().ToString().Substring(0, 4)}"; // Generates a unique client ID with 4 random characters

        public MainPage() // Constructor for the page
        {
            InitializeComponent(); // Initializes UI components from XAML
            MessagesList.ItemsSource = Messages; // Binds Messages collection to UI list
            ConnectToSignalR(); // Initiates SignalR connection
        }

        private async void ConnectToSignalR() // Connects to SignalR hub for real-time communication
        {
            _connection = new HubConnectionBuilder() // Builds a SignalR connection
                .WithUrl("http://localhost:5182/ecpihub") // URL of the SignalR hub
                .WithAutomaticReconnect() // Enables automatic reconnection on failure
                .Build(); // Finalizes the connection setup

            // Event handler for receiving messages from SignalR
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"📩 Received message from {user}: {message}"); // Log received message
                MainThread.BeginInvokeOnMainThread(() => // Ensure UI updates on the main thread
                {
                    Messages.Add($"{user}: {message}"); // Add message to UI-bound collection
                });
            });

            // Event handler for receiving a token event
            _connection.On<string, string>("TokenCreated", (user, token) =>
            {
                Console.WriteLine($"🔑 Received TokenCreated event for {user}"); // Log token event
                MainThread.BeginInvokeOnMainThread(() => // Ensure UI updates on the main thread
                {
                    Messages.Add($"🔑 Token created for {user}: Token is valid for one hour."); // Display token info in UI
                });
            });

            try
            {
                await _connection.StartAsync(); // Starts the SignalR connection
                Console.WriteLine("✅ Connected to ECPIHub."); // Log successful connection
                MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Connected to ECPIHub."); // Update UI label
            }
            catch (Exception ex) // Handle connection errors
            {
                Console.WriteLine($"❌ Error connecting to SignalR: {ex.Message}"); // Log error
                MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = $"Error: {ex.Message}"); // Update UI label with error
            }
        }

        private async void SendMessage(object sender, EventArgs e) // Sends a message via HTTP request
        {
            using var httpClient = new HttpClient(); // Creates an HTTP client instance
            var message = new
            {
                User = _clientId, // Includes client ID for tracking sender
                Text = MessageEntry.Text // Retrieves text from UI input field
            };

            var json = JsonSerializer.Serialize(message); // Converts message object to JSON string
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // Prepares HTTP content

            var response = await httpClient.PostAsync("http://localhost:5182/api/messages/send", content); // Sends HTTP POST request

            if (response.IsSuccessStatusCode) // If message was successfully sent
            {
                Console.WriteLine($"✅ Sent message: {message.Text}"); // Log success
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Add($"You ({_clientId}): {message.Text}"); // Add sent message to UI-bound collection
                    MessageEntry.Text = ""; // Clear the input field
                });
            }
            else // If sending failed
            {
                Console.WriteLine("❌ Error sending message."); // Log failure
                StatusLabel.Text = "Error sending message."; // Update UI with error message
            }
        }
    }
}
