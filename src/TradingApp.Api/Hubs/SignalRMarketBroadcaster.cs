using Microsoft.AspNetCore.SignalR;
using TradingApp.Application.Abstractions;

namespace TradingApp.Api.Hubs;

/// <summary>
/// IMarketBroadcaster'ın SignalR implementasyonu.
/// API katmanında çünkü MarketHub burada.
/// </summary>
public sealed class SignalRMarketBroadcaster : IMarketBroadcaster
{
    private readonly IHubContext<MarketHub, IMarketHubClient> _hub;
    public SignalRMarketBroadcaster(IHubContext<MarketHub, IMarketHubClient> hub) => _hub = hub;

    public Task BroadcastPriceAsync(
        string symbol, decimal price, decimal changePercent, decimal dailyVolume,
        CancellationToken ct = default)
    {
        var update = new MarketPriceUpdate(symbol, price, changePercent, dailyVolume, DateTimeOffset.UtcNow);
        return _hub.Clients.All.MarketPriceUpdated(update);
    }
}