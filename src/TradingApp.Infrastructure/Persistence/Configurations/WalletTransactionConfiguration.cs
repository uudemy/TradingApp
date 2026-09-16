using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> b)
    {
        b.ToTable("wallet_transactions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Type).HasConversion<int>().IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 8);
        b.Property(x => x.BalanceAfter).HasPrecision(18, 8);
        b.Property(x => x.Reference).HasMaxLength(64);
        b.Property(x => x.Description).HasMaxLength(256);

        b.HasIndex(x => x.WalletId);
        b.HasIndex(x => x.CreatedAt);
    }
}