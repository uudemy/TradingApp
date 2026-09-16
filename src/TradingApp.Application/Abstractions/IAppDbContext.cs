using Microsoft.EntityFrameworkCore;
using TradingApp.Domain.Entities;

namespace TradingApp.Application.Abstractions;

/// <summary>
/// Application katmanının veritabanı erişimi için kullandığı soyutlama.
/// Infrastructure bunu AppDbContext ile implement eder.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Asset> Assets { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<WalletTransaction> WalletTransactions { get; }
    DbSet<Portfolio> Portfolios { get; }
    DbSet<PortfolioPosition> PortfolioPositions { get; }
    DbSet<Order> Orders { get; }
    DbSet<Trade> Trades { get; }
    DbSet<WatchlistItem> WatchlistItems { get; }
    DbSet<PriceAlert> PriceAlerts { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}