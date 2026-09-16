using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> b)
    {
        b.ToTable("wallets");
        b.HasKey(x => x.Id);

        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.AvailableBalance).HasPrecision(18, 8);
        b.Property(x => x.LockedBalance).HasPrecision(18, 8);

        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasIndex(x => new { x.UserId, x.Currency }).IsUnique();
    }
}