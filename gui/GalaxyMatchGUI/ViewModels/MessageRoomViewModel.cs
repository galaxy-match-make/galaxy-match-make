using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic; // Add this
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using GalaxyMatchGUI.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Avalonia.Threading;

namespace GalaxyMatchGUI.ViewModels;

public partial class MessageRoomViewModel : ViewModelBase
{
    private HubConnection _connection;
   
    private readonly HttpClient _httpClient;
    
    // IDs for the current user and recipient
    private string _recipientId; // Replace with actual user ID
    private string _recipientName;
    private readonly string _currentUserId = JwtStorage.Instance.authDetails.UserId.ToString();
    private readonly string _currentUserName = JwtStorage.Instance.authDetails.Name;

    [ObservableProperty]
    private string currentMessage = string.Empty;
    
    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string selectedOption;

    public ObservableCollection<string> Options { get; } = new()
    {
        "Cheesy",
        "Clever",
        "Complimentary",
        "Flirty",
        "Funny",
        "Romantic"
    };

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public MessageRoomViewModel(Contact recipient = null)
    {
        _httpClient = new HttpClient();
        SelectedOption = Options.First();
        
        if (recipient != null)
        {
            _recipientId = recipient.UserId.ToString();
            _recipientName = recipient.DisplayName;
        }
        
        LoadInitialMessagesCommand.Execute(null);
    }

    private void AddMessage(string message)
    {
        Messages.Add(new ChatMessage
        {
            Text = $"{message}",
            IsIncoming = false,
            Timestamp= DateTime.UtcNow
        });
    }
    
    //Signal R
    [RelayCommand]
    public async Task Connect()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"https://localhost:7280/messagehub?username={Uri.EscapeDataString(_currentUserName)}")
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                Messages.Add(new ChatMessage
                {
                    Text = $"{user}: {message}",
                    IsIncoming = true,
                    Timestamp= DateTime.UtcNow
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

            AddMessage($"Connected as {_currentUserName}");
           

        }
        catch (Exception ex)
        {
            AddMessage($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RefreshMessagesAsync()
    {
        try
        {
            IsLoading = true;
            await LoadInitialMessagesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;
            
        IsLoading = true;
        
        try
        {
            var messageToSend = new
            {
                messageContent = CurrentMessage,
                sentDate = DateTime.UtcNow,
                senderId = _currentUserId,
                recipientId = _recipientId
            };

            //Signal R
            try
            {
                await _connection.InvokeAsync("SendMessageToUser", _currentUserName,
                    _recipientName,
                    CurrentMessage);
                AddMessage($"{CurrentMessage}");
               
            }
            catch (Exception ex)
            {
                AddMessage($"Error sending message: {ex.Message}");
            }

            // Send to API
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5284/api/messages", 
                messageToSend);

            CurrentMessage = $"{response.StatusCode}";
                
            if (response.IsSuccessStatusCode)
            {
                var savedMessage = await response.Content.ReadFromJsonAsync<ApiMessage>();
                
                if (savedMessage != null)
                {
                    // Convert UTC time to local time when displaying
                    Messages.Add(new ChatMessage
                    {
                        
                        Text = savedMessage.MessageContent,
                        IsIncoming = savedMessage.SenderId == _currentUserId,
                        Timestamp = savedMessage.SentDate.ToLocalTime()  // Convert to local time
                    });
                    
                    CurrentMessage = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task LoadInitialMessagesAsync()
    {
        try
        {
            IsLoading = true;
            
            // Fetch previous messages
            var response = await _httpClient.GetAsync(
                $"http://localhost:5284/api/messages/between?senderId={_currentUserId}&recipientId={_recipientId}");
                
            if (response.IsSuccessStatusCode)
            {
                var messages = await response.Content.ReadFromJsonAsync<ApiMessage[]>();
                
                if (messages != null)
                {
                    // Clear existing messages first to avoid duplicates
                    Messages.Clear();
                    
                    // Add messages in chronological order
                    foreach (var msg in messages.OrderBy(m => m.SentDate))
                    {
                        Messages.Add(new ChatMessage
                        {
                            
                            Text = msg.MessageContent,
                            IsIncoming = msg.SenderId == _currentUserId,
                            Timestamp = msg.SentDate.ToLocalTime() 
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading messages: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void TestBinding()
    {
        CurrentMessage = "Test message";
        Console.WriteLine($"Test message set: {CurrentMessage}");
    }

    [RelayCommand]
    private async Task GetRizzLineAsync()
    {
        try
        {
            IsLoading = true;
            
            // Get the selected category from dropdown (lowercase)
            var category = selectedOption;
            Console.WriteLine($"Fetching rizz line for category: {category}");
            
            // Get a page of lines in the selected category
            var response = await _httpClient.GetAsync(
                $"https://rizzapi.vercel.app/category/{category}?page=1&perPage=20");
                
            Console.WriteLine($"API Response Status: {response.StatusCode}");
                
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response Content: {responseContent}");
                
                // Deserialize to List<RizzLine>
                var lines = await response.Content.ReadFromJsonAsync<List<RizzLine>>();
                
                
                if (lines != null && lines.Count > 0)
                {
                    Console.WriteLine($"Parsed {lines.Count} lines");
                    
                    // Pick a random line from the results
                    var random = new Random();
                    var randomIndex = random.Next(0, lines.Count);
                    
                    // Get the text from the random line
                    var rizzLine = lines[randomIndex].text;
                    
                    Console.WriteLine($"Selected line: {rizzLine}");
                    
                    // Set it as the current message
                    CurrentMessage = rizzLine;
                    Console.WriteLine($"CurrentMessage set to: {CurrentMessage}");
                }
                else
                {
                    Console.WriteLine("No lines returned from API");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting rizz line: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    // Update the RizzLine class to match the actual API response
    private class RizzLine
    {
        public string text { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string _id { get; set; } = string.Empty;
        // Add other properties if needed
    }
}

// Class representing API message structure
public class ApiMessage
{
    public int Id { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
}

// Keep your ChatMessage class as is
//public class ChatMessage
//{
//    public int Id { get; set; } // Add this property
//    public string Content { get; set; } = string.Empty;
//    public bool IsSentByMe { get; set; }
//    public DateTime Timestamp { get; set; }
//}


public class ChatMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsIncoming { get; set; } // true = received, false = sent
    public DateTime Timestamp { get; set; }
}