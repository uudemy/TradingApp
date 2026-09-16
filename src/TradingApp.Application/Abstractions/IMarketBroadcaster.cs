namespace TradingApp.Application.Abstractions;

/// <summary>
/// Fiyat güncellemelerini bağlı client'lara yayınlar.
/// Infrastructure'da SignalR ile implement edilir.
/// </summary>
public interface IMarketBroadcaster
{
    Task BroadcastPriceAsync(
        string symbol,
        decimal price,
        decimal changePercent,
        decimal dailyVolume,
        CancellationToken ct = default);
}