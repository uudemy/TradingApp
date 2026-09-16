using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Infrastructure.MarketData.Yahoo;

public sealed class YahooFinanceMarketDataProvider : IMarketDataProvider
{
    private readonly YahooFinanceClient _client;
    private readonly SymbolMapper _mapper;
    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;
    private readonly ILogger<YahooFinanceMarketDataProvider> _logger;

    private static readonly TimeSpan PriceCacheTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CandleCacheTtl = TimeSpan.FromMinutes(5);

    public YahooFinanceMarketDataProvider(
        YahooFinanceClient client,
        SymbolMapper mapper,
        IAppDbContext db,
        IMarketDataCache cache,
        ILogger<YahooFinanceMarketDataProvider> logger)
    {
        _client = client;
        _mapper = mapper;
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();

        var cached = await _cache.GetPriceAsync(symbol, ct);
        if (cached is not null) return cached;

        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        var yahooSymbol = _mapper.ToYahoo(symbol);
        var result = await _client.GetQuoteAsync(yahooSymbol, ct);
        var meta = result.Meta!;

        var price = meta.RegularMarketPrice!.Value;
        var prevClose = meta.PreviousClose ?? asset.PreviousClose;
        var changePct = prevClose == 0m ? 0m
            : Math.Round((price - prevClose) / prevClose * 100m, 4);

        var dto = new MarketPriceDto(
            symbol, price, prevClose, changePct,
            meta.RegularMarketVolume ?? (long)asset.DailyVolume,
            DateTimeOffset.UtcNow);

        await _cache.SetPriceAsync(dto, PriceCacheTtl, ct);
        return dto;
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        var assets = await _db.Assets.AsNoTracking().Where(a => a.IsActive).ToListAsync(ct);
        var results = new List<MarketPriceDto>(assets.Count);

        foreach (var asset in assets)
        {
            try
            {
                var price = await GetPriceAsync(asset.Symbol, ct);
                results.Add(price);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price for {Symbol}", asset.Symbol);
            }
        }

        return results;
    }

    public async Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        var yahooSymbol = _mapper.ToYahoo(symbol);
        var result = await _client.GetChartAsync(yahooSymbol, interval, ct);

        var timestamps = result.Timestamps!;
        var quote = result.Indicators!.Quote![0];
        var opens = quote.Open ?? new();
        var highs = quote.High ?? new();
        var lows = quote.Low ?? new();
        var closes = quote.Close ?? new();
        var volumes = quote.Volume ?? new();

        var candles = new List<CandleDto>(timestamps.Count);
        var count = Math.Min(timestamps.Count,
            Math.Min(opens.Count, Math.Min(highs.Count, Math.Min(lows.Count, closes.Count))));

        for (int i = 0; i < count; i++)
        {
            if (opens[i] is null || highs[i] is null || lows[i] is null || closes[i] is null) continue;

            candles.Add(new CandleDto(
                YahooFinanceClient.FromUnix(timestamps[i]),
                opens[i]!.Value,
                highs[i]!.Value,
                lows[i]!.Value,
                closes[i]!.Value,
                i < volumes.Count && volumes[i] is not null ? volumes[i]!.Value : 0m));
        }

        // 4h için 1h'leri 4'lü grupla
        if (interval == "4h")
            candles = AggregateCandles(candles, 4);

        return candles.TakeLast(Math.Clamp(limit, 1, 500)).ToArray();
    }

    private static List<CandleDto> AggregateCandles(List<CandleDto> source, int groupSize)
    {
        var result = new List<CandleDto>(source.Count / groupSize + 1);
        for (int i = 0; i < source.Count; i += groupSize)
        {
            var chunk = source.Skip(i).Take(groupSize).ToList();
            if (chunk.Count == 0) continue;
            result.Add(new CandleDto(
                chunk[0].OpenTime,
                chunk[0].Open,
                chunk.Max(c => c.High),
                chunk.Min(c => c.Low),
                chunk[^1].Close,
                chunk.Sum(c => c.Volume)));
        }
        return result;
    }
}