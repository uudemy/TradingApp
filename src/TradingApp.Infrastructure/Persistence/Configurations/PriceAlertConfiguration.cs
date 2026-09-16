using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class PriceAlertConfiguration : IEntityTypeConfiguration<PriceAlert>
{
    public void Configure(EntityTypeBuilder<PriceAlert> b)
    {
        b.ToTable("price_alerts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Condition).HasConversion<int>().IsRequired();
        b.Property(x => x.TargetPrice).HasPrecision(18, 8);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => new { x.AssetId, x.IsActive });
    }
}