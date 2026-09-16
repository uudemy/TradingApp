namespace TradingApp.Infrastructure.MarketData.Options;

public sealed class MarketDataOptions
{
    public const string SectionName = "MarketData";

    public string Provider { get; set; } = "Yahoo";
    public int PollingIntervalSeconds { get; set; } = 60;
    public bool FallbackToMock { get; set; } = true;
    public YahooOptions Yahoo { get; set; } = new();
}

public sealed class YahooOptions
{
    public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com";
    public string UserAgent { get; set; } = "Mozilla/5.0";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public Dictionary<string, string> SymbolMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}