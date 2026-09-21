using System.Text.Json.Serialization;

namespace TradingApp.Infrastructure.MarketData.BistDataService;

/// <summary>
/// BIST Data Service /all ve /quotes endpoint yanıtı.
/// Gerçek JSON:
/// {
///   "market": "OPEN",
///   "count": 627,
///   "last_update": "2026-09-18T...",
///   "is_stale": false,
///   "delayed": true,
///   "quotes": [ { "symbol": "THYAO", "price": 342.5, ... } ]
/// }
/// </summary>
public sealed class BistAllResponse
{
    [JsonPropertyName("market")]
    public string? Market { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("last_update")]
    public string? LastUpdate { get; set; }

    [JsonPropertyName("is_stale")]
    public bool? IsStale { get; set; }

    [JsonPropertyName("delayed")]
    public bool? Delayed { get; set; }

    [JsonPropertyName("quotes")]
    public List<BistQuote>? Quotes { get; set; }
}

public sealed class BistQuote
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("previous_close")]
    public decimal? PreviousClose { get; set; }

    [JsonPropertyName("change")]
    public decimal? Change { get; set; }

    [JsonPropertyName("change_percent")]
    public decimal? ChangePercent { get; set; }

    [JsonPropertyName("volume")]
    public long? Volume { get; set; }

    [JsonPropertyName("high")]
    public decimal? High { get; set; }

    [JsonPropertyName("low")]
    public decimal? Low { get; set; }

    [JsonPropertyName("open")]
    public decimal? Open { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("is_stale")]
    public bool? IsStale { get; set; }

    [JsonPropertyName("last_update")]
    public string? LastUpdate { get; set; }
}

public sealed class BistHistoryResponse
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("data")]
    public List<BistBar>? Data { get; set; }
}

public sealed class BistBar
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("open")]
    public decimal? Open { get; set; }

    [JsonPropertyName("high")]
    public decimal? High { get; set; }

    [JsonPropertyName("low")]
    public decimal? Low { get; set; }

    [JsonPropertyName("close")]
    public decimal? Close { get; set; }

    [JsonPropertyName("volume")]
    public long? Volume { get; set; }
}