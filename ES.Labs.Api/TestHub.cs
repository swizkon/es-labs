

using Microsoft.AspNetCore.SignalR;

namespace ES.Labs.Api;

public class TestHub : Hub<ITestHubClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }

    public async Task Broadcast(string user, string message)
    {
        await Clients.All.Broadcast(user, message);
    }

    public async Task PlayerPosition(string player, int x, int y)
    {
        await Clients.All.PlayerPosition(player, x, y);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "All Connected Users");
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }
}
