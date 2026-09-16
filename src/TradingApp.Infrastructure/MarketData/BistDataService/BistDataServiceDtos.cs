using System.Text.Json.Serialization;

namespace TradingApp.Infrastructure.MarketData.BistDataService;

public sealed class BistAllResponse
{
    [JsonPropertyName("data")]
    public List<BistQuote>? Data { get; set; }

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
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