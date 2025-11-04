using MediatR;
using Microsoft.AspNetCore.SignalR;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

namespace StockApp.ApiEndpoints;

public static class DashboardEndpoints
{
    public static void MapDashboard(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");
        
        group.MapGet("/stats", async (IMediator mediator, IHubContext<StockHub> hubContext) =>
        {
            var result = await mediator.Send(new GetDashboardStatsQuery());
            
            // İstatistikleri tüm bağlı client'lara gönder
            await hubContext.Clients.All.SendAsync("DashboardStatsUpdated", result);
            return Results.Ok(result);
        })
        .Produces<DashboardStatsDto>(StatusCodes.Status200OK);
    }
}

