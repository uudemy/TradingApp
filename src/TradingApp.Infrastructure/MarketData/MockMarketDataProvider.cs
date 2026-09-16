using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Common;

namespace TradingApp.Infrastructure.MarketData;

public sealed class MockMarketDataProvider : IMarketDataProvider
{
    private static readonly TimeSpan PriceCacheTtl = TimeSpan.FromSeconds(2);

    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;

    private readonly ICandleStore _candles;

    public MockMarketDataProvider(IAppDbContext db, IMarketDataCache cache, ICandleStore candles)
    {
        _db = db;
        _cache = cache;
        _candles = candles;
    }

    public async Task<MarketPriceDto> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();

        var cached = await _cache.GetPriceAsync(symbol, ct);
        if (cached is not null) return cached;

        var asset = await _db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Symbol == symbol, ct)
            ?? throw new DomainException("asset_not_found", $"Asset '{symbol}' not found.");

        var dto = new MarketPriceDto(
            asset.Symbol,
            asset.CurrentPrice,
            asset.PreviousClose,
            asset.PreviousClose == 0m ? 0m
                : Math.Round((asset.CurrentPrice - asset.PreviousClose) / asset.PreviousClose * 100m, 4),
            asset.DailyVolume,
            DateTimeOffset.UtcNow);

        await _cache.SetPriceAsync(dto, PriceCacheTtl, ct);
        return dto;
    }

    public async Task<IReadOnlyCollection<MarketPriceDto>> GetPricesAsync(CancellationToken ct = default)
    {
        var assets = await _db.Assets.AsNoTracking().Where(a => a.IsActive).ToListAsync(ct);
        return assets.Select(a => new MarketPriceDto(
            a.Symbol,
            a.CurrentPrice,
            a.PreviousClose,
            a.PreviousClose == 0m ? 0m
                : Math.Round((a.CurrentPrice - a.PreviousClose) / a.PreviousClose * 100m, 4),
            a.DailyVolume,
            DateTimeOffset.UtcNow
        )).ToArray();
    }

        public Task<IReadOnlyCollection<CandleDto>> GetCandlesAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
        => _candles.GetCandlesAsync(symbol, interval, limit, ct);
}