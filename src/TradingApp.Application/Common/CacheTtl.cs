namespace TradingApp.Application.Common;

/// <summary>
/// Interval'e göre cache TTL.
/// Kısa periyotlar kısa ömür (veri hızlı değişir),
/// uzun periyotlar uzun ömür (veri yavaş değişir).
/// </summary>
public static class CacheTtl
{
    public static TimeSpan ForInterval(string interval) => interval.ToUpperInvariant() switch
    {
        "1S"  => TimeSpan.FromSeconds(15),
        "1M"  => TimeSpan.FromMinutes(1),       // 1 dakika
        "5M"  => TimeSpan.FromMinutes(2),
        "15M" => TimeSpan.FromMinutes(5),
        "1H"  => TimeSpan.FromMinutes(15),
        "4H"  => TimeSpan.FromMinutes(30),
        "1D"  => TimeSpan.FromHours(1),
        "1W"  => TimeSpan.FromHours(4),
        "1MO" => TimeSpan.FromHours(12),        // 1 ay (Month)
        _     => TimeSpan.FromMinutes(15)
    };

    public static TimeSpan ScreenerResult => TimeSpan.FromMinutes(5);
}