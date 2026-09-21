using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;
using TradingApp.Infrastructure.Persistence;

namespace TradingApp.Infrastructure.MarketData.BistDataService;

public sealed class BistAssetSyncService : IBistAssetSyncService
{
    private readonly BistDataServiceClient _client;
    private readonly AppDbContext _db;
    private readonly ILogger<BistAssetSyncService> _logger;

    public BistAssetSyncService(
        BistDataServiceClient client,
        AppDbContext db,
        ILogger<BistAssetSyncService> logger)
    {
        _client = client;
        _db = db;
        _logger = logger;
    }

    public async Task<BistSyncResult> SyncAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("BIST asset sync starting...");

        var quotes = await _client.GetAllQuotesAsync(ct);
        if (quotes.Count == 0)
        {
            _logger.LogWarning("BIST sync: no quotes fetched.");
            return new BistSyncResult(0, 0, 0, DateTimeOffset.UtcNow);
        }

        // Mevcut TRY hisselerini çek
        var existing = await _db.Assets
            .Where(a => a.Currency == "TRY" && a.AssetType == AssetType.Stock)
            .ToDictionaryAsync(a => a.Symbol, ct);

        int added = 0, updated = 0;

        foreach (var q in quotes)
        {
            if (string.IsNullOrWhiteSpace(q.Symbol)) continue;
            if (q.Price is null || q.Price <= 0m) continue;

            var symbol = q.Symbol.Trim().ToUpperInvariant();
            var name = string.IsNullOrWhiteSpace(q.Name) ? symbol : q.Name!.Trim();
            var price = q.Price.Value;
            var prevClose = q.PreviousClose ?? price;
            var volume = q.Volume ?? 0L;

            if (existing.TryGetValue(symbol, out var asset))
            {
                // Mevcut: fiyatları güncelle
                asset.SetPreviousClose(prevClose);
                asset.UpdatePrice(price, 0m);
                updated++;
            }
            else
            {
                // Yeni: ekle
                var created = new Asset(
                    symbol, name,
                    AssetType.Stock, "TRY",
                    initialPrice: price);

                created.SetPreviousClose(prevClose);
                if (volume > 0) created.UpdatePrice(price, volume);

                _db.Assets.Add(created);
                added++;
            }
        }

        await _db.SaveChangesAsync(ct);

        var total = existing.Count + added;
        _logger.LogInformation(
            "BIST sync complete: added={Added}, updated={Updated}, total={Total}",
            added, updated, total);

        return new BistSyncResult(added, updated, total, DateTimeOffset.UtcNow);
    }
}