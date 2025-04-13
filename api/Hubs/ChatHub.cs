using Microsoft.AspNetCore.SignalR;


namespace galaxy_match_make.Hubs
{
    public class Chathub: Hub
    {
        // Join a specific match room (created per match)
        public async Task JoinMatchRoom(string matchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
            await Clients.Group(matchId).SendAsync("ReceiveMessage", "System", $"User joined {matchId}");
        }

        public async Task LeaveMatchRoom(string matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
            await Clients.Group(matchId).SendAsync("ReceiveMessage", "System", $"User left {matchId}");
        }

        public async Task SendMessageToMatch(string matchId, string user, string message)
        {
            await Clients.Group(matchId).SendAsync("ReceiveMessage", user, message);
        }

    }
}
