namespace TradingApp.Api.Hubs;

/// <summary>
/// SignalR client metodları. Frontend bu metodları dinler.
/// </summary>
public interface IMarketHubClient
{
    /// <summary>Fiyat güncellendiğinde çağrılır.</summary>
    Task MarketPriceUpdated(MarketPriceUpdate update);
}

public sealed record MarketPriceUpdate(
    string Symbol,
    decimal Price,
    decimal ChangePercent,
    decimal DailyVolume,
    DateTimeOffset TimestampUtc);