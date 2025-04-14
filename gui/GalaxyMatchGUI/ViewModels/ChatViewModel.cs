// GalaxyMatchGUI/ViewModels/ChatViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class ChatViewModel : ViewModelBase
    {
        private HubConnection _connection;
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public string CurrentMessage { get; set; } = string.Empty;
        public string Username { get; set; } = "Cindi";
        public string TargetUsername { get; set; } = "Ben";

        public ChatViewModel()
        {
           
            _connection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:7280/messagehub?username={Uri.EscapeDataString(Username)}")
                .Build();

            _connection.Closed += async (error) =>
            {
                Messages.Add("Connection closed. Attempting to reconnect...");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    Messages.Add("Reconnected to server");
                }
                catch (Exception ex)
                {
                    Messages.Add($"Failed to reconnect: {ex.Message}");
                }
            };

            Messages.Add("ViewModel initialized.");
        }



        [RelayCommand]
        public async Task Connect()
        {
            try
            {
                await _connection.StartAsync();
                Messages.Add($"Connected as {Username}");


                

                _connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Messages.Add($"{user}: {message}");
                    });
                });
            }
            catch (Exception ex)
            {
                Messages.Add($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(CurrentMessage))
            {
                try
                {
                    await _connection.InvokeAsync("SendMessage",
                        TargetUsername,
                        CurrentMessage);

                    CurrentMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    AddMessage($"Error sending message: {ex.Message}");
                }
            }
        }


        private void AddMessage(string message)
        {
            Messages.Add(message);
        }

    }
}