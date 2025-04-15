// Add this new service class
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Json; // Add this namespace
using System.Text.Json; 

public class MessageService
{
    private readonly HttpClient _httpClient;

    public MessageService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5284")
        };
    }

    public async Task<bool> SendMessageAsync(string content, Guid senderId, Guid receiverId)
    {
        try
        {
            var message = new 
            {
                MessageContent = content,
                SenderId = senderId,
                RecipientId = receiverId
            };

            var response = await _httpClient.PostAsJsonAsync("/api/messages", message);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}