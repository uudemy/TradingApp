using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> b)
    {
        b.ToTable("assets");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Symbol).IsUnique();

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.AssetType).HasConversion<int>().IsRequired();

        b.Property(x => x.CurrentPrice).HasPrecision(18, 8);
        b.Property(x => x.PreviousClose).HasPrecision(18, 8);
        b.Property(x => x.DailyVolume).HasPrecision(28, 8);

        b.HasIndex(x => x.AssetType);
        b.HasIndex(x => x.IsActive);
    }
}