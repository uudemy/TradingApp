namespace TradingApp.Application.Abstractions;

public interface IMarketHubClient
{
    Task MarketPriceUpdated(MarketPriceUpdate update);
}

public sealed record MarketPriceUpdate(
    string Symbol,
    decimal Price,
    decimal ChangePercent,
    decimal DailyVolume,
    DateTimeOffset TimestampUtc);