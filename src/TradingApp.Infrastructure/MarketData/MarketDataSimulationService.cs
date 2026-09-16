using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;

namespace TradingApp.Infrastructure.MarketData;

/// <summary>
/// Her 2 saniyede tüm aktif asset'lerin fiyatını rastgele yürüyüşle günceller.
/// Asset tipine göre volatilite farklıdır.
/// Fiyat değişince ilgili cache anahtarlarını temizler.
/// </summary>
public sealed class MarketDataSimulationService : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(2);

    private readonly IServiceProvider _sp;
    private readonly ILogger<MarketDataSimulationService> _logger;
    private readonly Random _rng = new();

    public MarketDataSimulationService(IServiceProvider sp, ILogger<MarketDataSimulationService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MarketDataSimulationService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Market data tick failed.");
            }

            try { await Task.Delay(TickInterval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }

        _logger.LogInformation("MarketDataSimulationService stopped.");
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IAppDbContextFactory>();
        var cache = scope.ServiceProvider.GetRequiredService<IMarketDataCache>();
        var candles = scope.ServiceProvider.GetRequiredService<ICandleStore>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IMarketBroadcaster>();  // YENİ

        await using var db = (DbContext)dbFactory.CreateDbContext();
        var assets = await db.Set<Asset>().Where(a => a.IsActive).ToListAsync(ct);
        if (assets.Count == 0) return;

        foreach (var asset in assets)
        {
            var volatility = VolatilityFor(asset.AssetType);
            var pct = (decimal)((_rng.NextDouble() - 0.5) * 2.0 * volatility);
            if (Math.Abs(pct) < 0.0001m) pct = 0.0001m * (pct < 0 ? -1 : 1);

            var newPrice = asset.CurrentPrice * (1m + pct);
            if (newPrice < 0.01m) newPrice = 0.01m;
            newPrice = Math.Round(newPrice, 4);

            var tickVolume = (decimal)(_rng.NextDouble() * 10.0);
            asset.UpdatePrice(newPrice, tickVolume);

            var changePercent = asset.PreviousClose == 0m
                ? 0m
                : Math.Round((newPrice - asset.PreviousClose) / asset.PreviousClose * 100m, 4);

            await candles.AppendTickAsync(asset.Symbol, newPrice, tickVolume, ct);
            await cache.SetPriceAsync(
                new Application.Features.Market.Dtos.MarketPriceDto(
                    asset.Symbol, newPrice, asset.PreviousClose,
                    changePercent, asset.DailyVolume, DateTimeOffset.UtcNow),
                TimeSpan.FromSeconds(4), ct);

            // YENİ — SignalR yayını
            try
            {
                await broadcaster.BroadcastPriceAsync(
                    asset.Symbol, newPrice, changePercent, asset.DailyVolume, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Broadcast failed for {Symbol}", asset.Symbol);
            }
        }

        await db.SaveChangesAsync(ct);
        await cache.InvalidateAssetListAsync(ct);
    }

    private static double VolatilityFor(AssetType type) => type switch
    {
        AssetType.Stock => 0.003,   // %0.3
        AssetType.Crypto => 0.015,  // %1.5
        AssetType.Etf => 0.002,
        AssetType.Forex => 0.001,
        _ => 0.005
    };
}