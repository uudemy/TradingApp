using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Common;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.Stooq;

public sealed class StooqProvider : IMarketDataProvider
{
    private readonly StooqClient _client;
    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;
    private readonly StooqOptions _opt;
    private readonly ILogger<StooqProvider> _logger;

    private static readonly TimeSpan PriceCacheTtl = TimeSpan.FromSeconds(30);

    public StooqProvider(
        StooqClient client,
        IAppDbContext db,
        IMarketDataCache cache,
        IOptions<MarketDataOptions> options,
        ILogger<StooqProvider> logger)
    {
        _client = client;
        _db = db;
        _cache = cache;
        _opt = options.Value.Stooq;
        _logger = logger;
    }

    public async Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();

        var cached = await _cache.GetPriceAsync(symbol, ct);
        if (cached is not null) return cached;

        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        if (!_opt.SymbolMappings.TryGetValue(symbol, out var stooqSymbol))
            throw new DomainException("stooq_symbol_missing", $"No Stooq symbol for {symbol}.");

        var quote = await _client.GetQuoteAsync(stooqSymbol, ct)
            ?? throw new DomainException("stooq_quote_missing", $"Stooq quote not found for {symbol}.");

        var price = quote.Close;
        var prevClose = asset.PreviousClose;
        var changePct = prevClose == 0m ? 0m : Math.Round((price - prevClose) / prevClose * 100m, 4);

        var dto = new MarketPriceDto(
            symbol, price, prevClose, changePct,
            quote.Volume, DateTimeOffset.UtcNow);

        await _cache.SetPriceAsync(dto, PriceCacheTtl, ct);
        return dto;
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => a.IsActive && (a.Currency == "USD" || a.Currency == "TRY"))
            .ToListAsync(ct);

        var results = new List<MarketPriceDto>(assets.Count);
        foreach (var asset in assets)
        {
            if (!_opt.SymbolMappings.ContainsKey(asset.Symbol)) continue;
            try { results.Add(await GetPriceAsync(asset.Symbol, ct)); }
            catch (Exception ex) { _logger.LogWarning(ex, "Stooq price failed for {Symbol}", asset.Symbol); }
        }

        return results;
    }

    public async Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (!_opt.SymbolMappings.TryGetValue(symbol, out var stooqSymbol))
            throw new DomainException("stooq_symbol_missing", $"No Stooq symbol for {symbol}.");

        var bars = await _client.GetDailyHistoryAsync(stooqSymbol, ct);
        if (bars.Count == 0)
            throw new DomainException("stooq_history_empty", $"No history for {symbol}.");

        var candles = bars.Select(b => new CandleDto(
            b.Date, b.Open, b.High, b.Low, b.Close, b.Volume)).ToArray();

        return candles.TakeLast(Math.Clamp(limit, 1, 500)).ToArray();
    }
}