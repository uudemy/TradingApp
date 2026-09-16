using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Entities;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData;

/// <summary>
/// Yahoo'dan periyodik olarak (default 60 sn) tüm aktif asset'lerin
/// fiyatlarını çeker; DB, Redis cache ve SignalR'ı günceller.
/// </summary>
public sealed class MarketDataPollingService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<MarketDataPollingService> _logger;
    private readonly TimeSpan _interval;

    public MarketDataPollingService(
        IServiceProvider sp,
        IOptions<MarketDataOptions> options,
        ILogger<MarketDataPollingService> logger)
    {
        _sp = sp;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(Math.Max(15, options.Value.PollingIntervalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MarketDataPollingService started (interval={Sec}s).", _interval.TotalSeconds);

        // İlk tur hemen
        await SafeTickAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SafeTickAsync(stoppingToken);
        }

        _logger.LogInformation("MarketDataPollingService stopped.");
    }

    private async Task SafeTickAsync(CancellationToken ct)
    {
        try
        {
            await TickAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Market data tick failed.");
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IAppDbContextFactory>();
        var provider = scope.ServiceProvider.GetRequiredService<IMarketDataProvider>();
        var cache = scope.ServiceProvider.GetRequiredService<IMarketDataCache>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IMarketBroadcaster>();

        await using var db = (DbContext)dbFactory.CreateDbContext();
        var assets = await db.Set<Asset>().Where(a => a.IsActive).ToListAsync(ct);
        if (assets.Count == 0) return;

        var updated = 0;
        foreach (var asset in assets)
        {
            try
            {
                var price = await provider.GetPriceAsync(asset.Symbol, ct);

                asset.UpdatePrice(price.Price, 0m);
                await cache.SetPriceAsync(price, TimeSpan.FromSeconds(90), ct);
                await broadcaster.BroadcastPriceAsync(
                    asset.Symbol, price.Price, price.ChangePercent, price.DailyVolume, ct);
                updated++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Polling skip {Symbol}", asset.Symbol);
            }
        }

        if (updated > 0)
        {
            await db.SaveChangesAsync(ct);
            await cache.InvalidateAssetListAsync(ct);
        }

        _logger.LogInformation("Market poll complete: {Updated}/{Total} assets updated.", updated, assets.Count);
    }
}