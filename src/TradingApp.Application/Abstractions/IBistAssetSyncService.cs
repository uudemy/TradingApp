namespace TradingApp.Application.Abstractions;

public sealed record BistSyncResult(
    int Added,
    int Updated,
    int Total,
    DateTimeOffset SyncedAt);

public interface IBistAssetSyncService
{
    /// <summary>
    /// BIST Data Service'ten tüm aktif hisseleri çeker ve ana DB'ye aktarır.
    /// Yeni semboller eklenir, mevcutların fiyatları güncellenir.
    /// </summary>
    Task<BistSyncResult> SyncAsync(CancellationToken ct = default);
}