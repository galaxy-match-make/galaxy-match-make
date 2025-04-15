using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;


namespace galaxy_match_make.Hubs
{
    public class MessageHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _users = new();

        public override Task OnConnectedAsync()
        {
            //TODO: check if user exists already
            var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
           

            // Only add the user if the username is valid
            if (!string.IsNullOrEmpty(username))
            {
                // Map the connection ID to the username
                _users[Context.ConnectionId] = username;
                Console.WriteLine("\n");
                //foreach (var kvp in _users)
                //{
                //    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                //}
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
        public async Task SendMessageToUser(string sender, string receiver, string message)
    {
            // Find the connection ID for the target user
            var receiverConnectionId = _users.FirstOrDefault(u => u.Value == receiver).Key;
            var senderConnectionId = _users.FirstOrDefault(u => u.Value == sender).Key;

            if (!string.IsNullOrEmpty(receiverConnectionId) && !string.IsNullOrEmpty(senderConnectionId))
        {
                Console.WriteLine("Receiver: " + _users[receiverConnectionId]);
                Console.WriteLine("Sender: " + _users[senderConnectionId]);
                Console.WriteLine("Message" + message);
                Console.WriteLine("\n");
            
                // Send message to the receiver with the sender's name
                try
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", sender, message);
                }
                catch (Exception ex)
                {
                    // Log error - likely means client disconnected
                    Console.WriteLine($"Failed to send: {ex.Message}");
                }
        }
            else
        {
                // If receiver not connected, notify sender
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"User {receiver} is not connected.");
            }
        }


    }
}