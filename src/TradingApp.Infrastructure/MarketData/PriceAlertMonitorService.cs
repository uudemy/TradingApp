using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;

namespace TradingApp.Infrastructure.MarketData;

/// <summary>
/// Her X saniyede bir aktif price alert'leri kontrol eder.
/// Koşul sağlanıyorsa:
///   1) Alert.MarkTriggered()
///   2) Notification kaydı
///   3) SignalR ile canlı bildirim
/// </summary>
public sealed class PriceAlertMonitorService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _sp;
    private readonly ILogger<PriceAlertMonitorService> _logger;

    public PriceAlertMonitorService(IServiceProvider sp, ILogger<PriceAlertMonitorService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceAlertMonitorService started (interval={Sec}s).", Interval.TotalSeconds);

        using var timer = new PeriodicTimer(Interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Alert monitor tick failed."); }
        }

        _logger.LogInformation("PriceAlertMonitorService stopped.");
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IAppDbContextFactory>();
        var cache = scope.ServiceProvider.GetRequiredService<IMarketDataCache>();
        var publisher = scope.ServiceProvider.GetRequiredService<INotificationPublisher>();

        await using var db = (DbContext)dbFactory.CreateDbContext();

        var activeAlerts = await db.Set<PriceAlert>()
            .Where(p => p.IsActive)
            .ToListAsync(ct);

        if (activeAlerts.Count == 0) return;

        // Asset sembollerini toplu çek
        var assetIds = activeAlerts.Select(a => a.AssetId).Distinct().ToList();
        var assets = await db.Set<Asset>()
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var triggered = 0;

        foreach (var alert in activeAlerts)
        {
            if (!assets.TryGetValue(alert.AssetId, out var asset)) continue;

            var price = asset.CurrentPrice;
            if (!alert.IsTriggered(price)) continue;

            // 1) Alert'i tetikle
            alert.MarkTriggered();

            // 2) Notification oluştur
            var direction = alert.Condition == PriceAlertCondition.GreaterThan ? "üzerine çıktı" : "altına düştü";
            var title = $"{asset.Symbol} fiyat alarmı";
            var message = $"{asset.Symbol} fiyatı {alert.TargetPrice:N2} {direction}. Şu anki fiyat: {price:N2}.";

            var notification = new Notification(
                alert.UserId, title, message, NotificationType.PriceAlertTriggered);
            db.Set<Notification>().Add(notification);

            // 3) SignalR yayını
            await publisher.PublishAsync(
                alert.UserId,
                new NotificationPayload(
                    notification.Id, title, message,
                    notification.Type.ToString(), notification.CreatedAt),
                ct);

            triggered++;
        }

        if (triggered > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Price alerts triggered: {Count}", triggered);
        }
    }
}