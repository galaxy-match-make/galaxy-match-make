// ChatHub.cs
using Microsoft.AspNetCore.SignalR;

namespace galaxy_match_make.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderId, string recipientId, string messageContent)
        {
            
            await Clients.Users(new[] { senderId, recipientId })
            .SendAsync("ReceiveMessage", new {
                Id = 0, // Replace with actual DB ID if available
                SenderId = senderId,
                RecipientId = recipientId,
                MessageContent = messageContent,
                SentDate = DateTime.Now
            });
        }
        [HubMethodName("JoinChat")]
        public async Task JoinChat(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}