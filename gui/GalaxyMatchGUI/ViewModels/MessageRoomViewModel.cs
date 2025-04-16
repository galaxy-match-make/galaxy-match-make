using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.lib;
using System.Threading;

namespace GalaxyMatchGUI.ViewModels;

public partial class MessageRoomViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient;
    private System.Threading.Timer _pollingTimer;
    private CancellationTokenSource _pollingCts;
    private bool _isPolling = false;
    private const int POLLING_INTERVAL = 3000; 
    private DateTime _lastMessageTime = DateTime.MinValue;
    
    private string _recipientId;
    private readonly string _currentUserId = JwtStorage.Instance.authDetails.UserId.ToString();
    
    [ObservableProperty]
    private string currentMessage = string.Empty;
    
    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string selectedOption;
    [ObservableProperty]
    private string _recipientName;

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
            RecipientName = recipient.DisplayName;
            _ = StartPolling();
        }
        
        LoadInitialMessagesCommand.Execute(null);
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

    private async Task StartPolling()
    {
        if (_isPolling || string.IsNullOrEmpty(_recipientId)) return;
        
        _isPolling = true;
        _pollingCts = new CancellationTokenSource();
        
        try
        {
            await Task.Delay(1000, _pollingCts.Token);
            
            while (!_pollingCts.Token.IsCancellationRequested)
            {
                await PollForNewMessages();
                await Task.Delay(POLLING_INTERVAL, _pollingCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Polling error: {ex.Message}");
        }
        finally
        {
            _isPolling = false;
        }
    }

    private async Task PollForNewMessages()
    {
        try
        {
            if (IsLoading) return;

            var response = await _httpClient.GetAsync(
                $"{AppSettings.BackendUrl}/api/messages/between?senderId={_currentUserId}&receiverId={_recipientId}");

            if (response.IsSuccessStatusCode)
            {
                var allMessages = await response.Content.ReadFromJsonAsync<ApiMessage[]>();
                
                if (allMessages != null)
                {
                    // Filter for messages newer than our last known message
                    var newMessages = allMessages
                        .Where(m => m.SentDate > _lastMessageTime)
                        .OrderBy(m => m.SentDate)
                        .ToList();

                    if (newMessages.Any())
                    {
                        // Update last message time
                        _lastMessageTime = newMessages.Max(m => m.SentDate);
                        
                        // Add new messages to collection
                        foreach (var msg in newMessages)
                        {
                            Messages.Add(new ChatMessage
                            {
                                Id = msg.Id,
                                Content = msg.MessageContent,
                                IsSentByMe = msg.SenderId == _currentUserId,
                                Timestamp = msg.SentDate.ToLocalTime()
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error polling for messages: {ex.Message}");
        }
    }

    private void StopPolling()
    {
        _isPolling = false;
        _pollingTimer?.Dispose();
        _pollingTimer = null;
    }

    public void Cleanup()
    {
        StopPolling();
    }

    [RelayCommand]
    private void GoBack()
    {
        Cleanup();
        NavigationService?.NavigateTo<InteractionsViewModel>();
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
            
            var response = await _httpClient.PostAsJsonAsync(
                AppSettings.BackendUrl+"/api/messages", 
                messageToSend);

            if (response.IsSuccessStatusCode)
            {
                CurrentMessage = string.Empty;
                
                await PollForNewMessages(); 
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
            Messages.Clear();
            
            var response = await _httpClient.GetAsync(
                $"{AppSettings.BackendUrl}/api/messages/between?senderId={_currentUserId}&receiverId={_recipientId}");

            if (response.IsSuccessStatusCode)
            {
                var messages = await response.Content.ReadFromJsonAsync<ApiMessage[]>();
                
                if (messages != null && messages.Length > 0)
                {
                    foreach (var msg in messages.OrderBy(m => m.SentDate))
                    {
                        Messages.Add(new ChatMessage
                        {
                            Id = msg.Id,
                            Content = msg.MessageContent,
                            IsSentByMe = msg.SenderId == _currentUserId,
                            Timestamp = msg.SentDate.ToLocalTime()
                        });
                    }
                    
                    _lastMessageTime = messages.Max(m => m.SentDate);
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
            
            var category = selectedOption;
            Console.WriteLine($"Fetching rizz line for category: {category}");
            
            var response = await _httpClient.GetAsync(
                $"https://rizzapi.vercel.app/category/{category}?page=1&perPage=20");
                
            Console.WriteLine($"API Response Status: {response.StatusCode}");
                
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response Content: {responseContent}");
                
                var lines = await response.Content.ReadFromJsonAsync<List<RizzLine>>();
                
                
                if (lines != null && lines.Count > 0)
                {
                    Console.WriteLine($"Parsed {lines.Count} lines");
                    
                    var random = new Random();
                    var randomIndex = random.Next(0, lines.Count);
                    
                    var rizzLine = lines[randomIndex].text;
                    
                    Console.WriteLine($"Selected line: {rizzLine}");
                    
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
    private class RizzLine
    {
        public string text { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string _id { get; set; } = string.Empty;
    }
}

public class ApiMessage
{
    public int Id { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
}

public class ChatMessage
{
    public int Id { get; set; }    public string Content { get; set; } = string.Empty;
    public bool IsSentByMe { get; set; }
    public DateTime Timestamp { get; set; }
}