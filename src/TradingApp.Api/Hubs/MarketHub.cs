using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TradingApp.Application.Abstractions;

namespace TradingApp.Api.Hubs;

[AllowAnonymous]
public sealed class MarketHub : Hub<IMarketHubClient>
{
    private readonly ILogger<MarketHub> _logger;
    public MarketHub(ILogger<MarketHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public Task Subscribe(string symbol)
    {
        _logger.LogInformation("Client {Id} subscribed to {Symbol}", Context.ConnectionId, symbol);
        return Task.CompletedTask;
    }
}