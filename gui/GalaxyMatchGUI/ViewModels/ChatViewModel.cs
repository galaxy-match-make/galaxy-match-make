// GalaxyMatchGUI/ViewModels/ChatViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class ChatViewModel : ViewModelBase
    {
        private HubConnection _connection;
        // public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        [ObservableProperty]
        private string currentMessage;
        public string Username { get; set; } = "Cindi";
        public string TargetUsername { get; set; } = "Ben";

        public ChatViewModel()
        {
            AddMessage("ViewModel initialized.");
        }



        [RelayCommand]
        public async Task Connect()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:7280/messagehub?username={Uri.EscapeDataString(Username)}")
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Messages.Add(new ChatMessage
                    {
                        Text = $"{user}: {message}",
                        IsIncoming = true
                    });
                });
            });

            _connection.Closed += async (error) =>
            {
                AddMessage("Connection closed. Attempting to reconnect...");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    AddMessage("Reconnected to server");
                }
                catch (Exception ex)
                {
                    AddMessage($"Failed to reconnect: {ex.Message}");
                }
            };

            try
            {
                await _connection.StartAsync();
                Messages.Add(new ChatMessage 
                { 
                    Text = $"Connected as {Username}", 
                    IsIncoming=false
                });

            }
            catch (Exception ex)
            {
                AddMessage($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(CurrentMessage))
            {
                try
                {
                    await _connection.InvokeAsync("SendMessageToUser", Username,
                        TargetUsername,
                        CurrentMessage);

                    AddMessage($"{CurrentMessage}");
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
            Messages.Add(new ChatMessage
            {
                Text = $"{message}",
                IsIncoming = false
            });
        }

    }
}