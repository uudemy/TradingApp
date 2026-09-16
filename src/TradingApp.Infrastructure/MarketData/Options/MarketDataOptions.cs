namespace TradingApp.Infrastructure.MarketData.Options;

public sealed class MarketDataOptions
{
    public const string SectionName = "MarketData";

    public string Provider { get; set; } = "Yahoo";
    public int PollingIntervalSeconds { get; set; } = 60;
    public bool FallbackToMock { get; set; } = true;
    public bool FallbackToYahoo { get; set; } = true;

    public YahooOptions Yahoo { get; set; } = new();
    public BistDataServiceOptions BistDataService { get; set; } = new();
    public CoinMarketCapOptions CoinMarketCap { get; set; } = new();
    public StooqOptions Stooq { get; set; } = new();
}

public sealed class YahooOptions
{
    public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com";
    public string UserAgent { get; set; } = "Mozilla/5.0";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public Dictionary<string, string> SymbolMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class BistDataServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public bool AuthRequired { get; set; } = false;
    public string ApiKey { get; set; } = "";
}

public sealed class CoinMarketCapOptions
{
    public string BaseUrl { get; set; } = "https://pro-api.coinmarketcap.com/public-api";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public Dictionary<string, string> SymbolMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class StooqOptions
{
    public string BaseUrl { get; set; } = "https://stooq.com";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public Dictionary<string, string> SymbolMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}