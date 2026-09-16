using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Infrastructure.MarketData.BistDataService;

public sealed class BistDataServiceProvider : IMarketDataProvider
{
    private readonly BistDataServiceClient _client;
    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;
    private readonly ILogger<BistDataServiceProvider> _logger;

    private static readonly TimeSpan PriceCacheTtl = TimeSpan.FromSeconds(30);

    public BistDataServiceProvider(
        BistDataServiceClient client,
        IAppDbContext db,
        IMarketDataCache cache,
        ILogger<BistDataServiceProvider> logger)
    {
        _client = client;
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

        var quote = await _client.GetQuoteAsync(symbol, ct)
            ?? throw new DomainException("bist_quote_missing", $"BIST quote not found for {symbol}.");

        var price = quote.Price!.Value;
        var prevClose = quote.PreviousClose ?? asset.PreviousClose;
        var changePct = quote.ChangePercent
            ?? (prevClose == 0m ? 0m : Math.Round((price - prevClose) / prevClose * 100m, 4));

        var dto = new MarketPriceDto(
            symbol, price, prevClose, Math.Round(changePct, 4),
            quote.Volume ?? 0L, DateTimeOffset.UtcNow);

        await _cache.SetPriceAsync(dto, PriceCacheTtl, ct);
        return dto;
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => a.IsActive && a.Currency == "TRY")
            .ToListAsync(ct);

        if (assets.Count == 0) return Array.Empty<MarketPriceDto>();

        var allQuotes = await _client.GetAllQuotesAsync(ct);
        var quoteMap = allQuotes
            .Where(q => !string.IsNullOrEmpty(q.Symbol))
            .ToDictionary(q => q.Symbol!.ToUpperInvariant(), q => q);

        var results = new List<MarketPriceDto>(assets.Count);
        foreach (var asset in assets)
        {
            if (!quoteMap.TryGetValue(asset.Symbol.ToUpperInvariant(), out var quote)) continue;
            if (quote.Price is null) continue;

            var price = quote.Price.Value;
            var prevClose = quote.PreviousClose ?? asset.PreviousClose;
            var changePct = quote.ChangePercent
                ?? (prevClose == 0m ? 0m : Math.Round((price - prevClose) / prevClose * 100m, 4));

            results.Add(new MarketPriceDto(
                asset.Symbol, price, prevClose, Math.Round(changePct, 4),
                quote.Volume ?? 0L, DateTimeOffset.UtcNow));
        }

        return results;
    }

    public async Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();

        var bars = await _client.GetHistoryAsync(symbol, ct);
        if (bars.Count == 0)
            throw new DomainException("bist_history_empty", $"No history for {symbol}.");

        var candles = new List<CandleDto>(bars.Count);
        foreach (var bar in bars)
        {
            if (string.IsNullOrWhiteSpace(bar.Date)) continue;
            if (!DateTimeOffset.TryParse(bar.Date, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var dt)) continue;
            if (bar.Open is null || bar.High is null || bar.Low is null || bar.Close is null) continue;

            candles.Add(new CandleDto(
                dt, bar.Open.Value, bar.High.Value, bar.Low.Value, bar.Close.Value,
                bar.Volume ?? 0L));
        }

        return candles.TakeLast(Math.Clamp(limit, 1, 500)).ToArray();
    }
}