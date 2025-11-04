namespace StockApp.Hub;

public class StockHub : Microsoft.AspNetCore.SignalR.Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
    }

    public async Task JoinDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "DashboardGroup");
    }
    
    public async Task LeaveDashboardGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DashboardGroup");
    }
}