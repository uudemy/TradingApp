using System.Text.Json.Serialization;

namespace TradingApp.Infrastructure.MarketData.CoinMarketCap;

public sealed class CmcSimplePriceResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, CmcPriceData>? Data { get; set; }

    [JsonPropertyName("status")]
    public CmcStatus? Status { get; set; }
}

public sealed class CmcPriceData
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, CmcQuote>? Quote { get; set; }
}

public sealed class CmcQuote
{
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal? Volume24h { get; set; }

    [JsonPropertyName("percent_change_1h")]
    public decimal? PercentChange1h { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public decimal? PercentChange24h { get; set; }

    [JsonPropertyName("percent_change_7d")]
    public decimal? PercentChange7d { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal? MarketCap { get; set; }

    [JsonPropertyName("last_updated")]
    public string? LastUpdated { get; set; }
}

public sealed class CmcStatus
{
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}