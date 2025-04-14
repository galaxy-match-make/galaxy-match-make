using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;


namespace galaxy_match_make.Hubs
{
    public class MessageHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _users = new();

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
            Console.WriteLine(username);
            Console.WriteLine($"User connected: {username} ({Context.ConnectionId})");
            // Only add the user if the username is valid
            if (!string.IsNullOrEmpty(username))
            {
                // Map the connection ID to the username
                _users[Context.ConnectionId] = username;
            }

            return base.OnConnectedAsync();
        }

        //public override Task OnDisconnectedAsync(Exception exception)
        //{
        //    var user = _users.FirstOrDefault(x => x.Value == Context.ConnectionId);
            
        //    _users.TryRemove(user.Key, out _);
        //    return base.OnDisconnectedAsync(exception);
        //}

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // Method for sending a message from one user to another
        public async Task SendMessageToUser(string receiver, string message)
        {
            // Find the connection ID for the target user
            var receiverConnectionId = _users.FirstOrDefault(u => u.Value == receiver).Key;

            if (!string.IsNullOrEmpty(receiverConnectionId))
            {
                // Send the message to the target user by connection ID
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
            }
            else
            {
                // Optionally handle case when receiver is not connected
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"User {receiver} is not connected.");
            }
        }

    }
}
