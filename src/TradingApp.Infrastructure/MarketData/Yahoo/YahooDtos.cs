using System.Text.Json.Serialization;

namespace TradingApp.Infrastructure.MarketData.Yahoo;

public sealed class YahooChartResponse
{
    [JsonPropertyName("chart")]
    public YahooChartRoot? Chart { get; set; }
}

public sealed class YahooChartRoot
{
    [JsonPropertyName("result")]
    public List<YahooChartResult>? Result { get; set; }

    [JsonPropertyName("error")]
    public YahooError? Error { get; set; }
}

public sealed class YahooError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public sealed class YahooChartResult
{
    [JsonPropertyName("meta")]
    public YahooMeta? Meta { get; set; }

    [JsonPropertyName("timestamp")]
    public List<long>? Timestamps { get; set; }

    [JsonPropertyName("indicators")]
    public YahooIndicators? Indicators { get; set; }
}

public sealed class YahooMeta
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; set; }

    [JsonPropertyName("chartPreviousClose")]
    public decimal? PreviousClose { get; set; }

    [JsonPropertyName("regularMarketVolume")]
    public long? RegularMarketVolume { get; set; }

    [JsonPropertyName("exchangeName")]
    public string? ExchangeName { get; set; }
}

public sealed class YahooIndicators
{
    [JsonPropertyName("quote")]
    public List<YahooQuote>? Quote { get; set; }
}

public sealed class YahooQuote
{
    [JsonPropertyName("open")]
    public List<decimal?>? Open { get; set; }

    [JsonPropertyName("high")]
    public List<decimal?>? High { get; set; }

    [JsonPropertyName("low")]
    public List<decimal?>? Low { get; set; }

    [JsonPropertyName("close")]
    public List<decimal?>? Close { get; set; }

    [JsonPropertyName("volume")]
    public List<long?>? Volume { get; set; }
}