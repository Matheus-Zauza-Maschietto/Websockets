using Microsoft.AspNetCore.SignalR;

namespace ChatForumSignalR;

public class ChatHub : Hub
{
    public async Task message(string user, string message)
    {
        Clients.All.SendAsync("message", $"User{Context.GetHashCode()}: {message}" );
    }
    
    public async Task writing(string user, string message)
    {
        Clients.Others.SendAsync("writing", $"User{Context.GetHashCode()} is writing" );
    }
    
}
