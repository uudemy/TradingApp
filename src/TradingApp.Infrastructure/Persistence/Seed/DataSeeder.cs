using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;

namespace TradingApp.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // --- Roles ---
        if (!await db.Roles.AnyAsync(ct))
        {
            db.Roles.AddRange(
                new Role("Admin", "System administrator"),
                new Role("User", "Standard demo trader")
            );
            logger.LogInformation("Seeded roles.");
        }

        // --- Assets ---
        if (!await db.Assets.AnyAsync(ct))
        {
            db.Assets.AddRange(
                new Asset("THYAO", "Türk Hava Yolları",    AssetType.Stock,  "TRY",  320m),
                new Asset("ASELS", "Aselsan",              AssetType.Stock,  "TRY",   58m),
                new Asset("GARAN", "Garanti BBVA",         AssetType.Stock,  "TRY",   95m),
                new Asset("AAPL",  "Apple Inc.",           AssetType.Stock,  "USD",  230m),
                new Asset("TSLA",  "Tesla Inc.",           AssetType.Stock,  "USD",  245m),
                new Asset("BTC",   "Bitcoin",              AssetType.Crypto, "USDT", 100000m),
                new Asset("ETH",   "Ethereum",             AssetType.Crypto, "USDT", 3400m)
            );
            logger.LogInformation("Seeded assets.");
        }

        await db.SaveChangesAsync(ct);
    }
}