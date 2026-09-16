using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.CoinMarketCap;

public sealed class CoinMarketCapProvider : IMarketDataProvider
{
    private readonly CoinMarketCapClient _client;
    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;
    private readonly CoinMarketCapOptions _opt;
    private readonly ILogger<CoinMarketCapProvider> _logger;

    private static readonly TimeSpan PriceCacheTtl = TimeSpan.FromSeconds(60);

    public CoinMarketCapProvider(
        CoinMarketCapClient client,
        IAppDbContext db,
        IMarketDataCache cache,
        IOptions<MarketDataOptions> options,
        ILogger<CoinMarketCapProvider> logger)
    {
        _client = client;
        _db = db;
        _cache = cache;
        _opt = options.Value.CoinMarketCap;
        _logger = logger;
    }

    public async Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();

        var cached = await _cache.GetPriceAsync(symbol, ct);
        if (cached is not null) return cached;

        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        if (!_opt.SymbolMappings.TryGetValue(symbol, out var cmcId))
            throw new DomainException("cmc_symbol_missing", $"No CMC ID for {symbol}.");

        var resp = await _client.GetSimplePriceAsync(new[] { cmcId }, ct);
        if (resp.Data is null || !resp.Data.TryGetValue(cmcId, out var data))
            throw new DomainException("cmc_quote_missing", $"CMC quote not found for {symbol}.");

        if (data.Quote is null || !data.Quote.TryGetValue("USD", out var usd))
            throw new DomainException("cmc_usd_missing", $"CMC USD quote missing for {symbol}.");

        var price = usd.Price!.Value;
        var changePct = usd.PercentChange24h ?? 0m;
        var prevClose = changePct == 0m ? price : price / (1m + changePct / 100m);
        var volume = (long)(usd.Volume24h ?? 0m);

        var dto = new MarketPriceDto(
            symbol, price, Math.Round(prevClose, 8), Math.Round(changePct, 4),
            volume, DateTimeOffset.UtcNow);

        await _cache.SetPriceAsync(dto, PriceCacheTtl, ct);
        return dto;
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => a.IsActive && a.Currency == "USDT")
            .ToListAsync(ct);

        if (assets.Count == 0) return Array.Empty<MarketPriceDto>();

        var mappings = assets
            .Select(a => (Asset: a, CmcId: _opt.SymbolMappings.GetValueOrDefault(a.Symbol)))
            .Where(x => x.CmcId is not null)
            .ToList();

        var cmcIds = mappings.Select(x => x.CmcId!).Distinct();
        var resp = await _client.GetSimplePriceAsync(cmcIds, ct);

        var results = new List<MarketPriceDto>(mappings.Count);
        foreach (var (asset, cmcId) in mappings)
        {
            if (resp.Data is null || !resp.Data.TryGetValue(cmcId!, out var data)) continue;
            if (data.Quote is null || !data.Quote.TryGetValue("USD", out var usd)) continue;

            var price = usd.Price!.Value;
            var changePct = usd.PercentChange24h ?? 0m;
            var prevClose = changePct == 0m ? price : price / (1m + changePct / 100m);

            results.Add(new MarketPriceDto(
                asset.Symbol, price, Math.Round(prevClose, 8), Math.Round(changePct, 4),
                (long)(usd.Volume24h ?? 0m), DateTimeOffset.UtcNow));
        }

        return results;
    }

    public Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
        => throw new DomainException("cmc_candles_unsupported",
            "CoinMarketCap provider does not support candles. Use Yahoo or Stooq.");
}