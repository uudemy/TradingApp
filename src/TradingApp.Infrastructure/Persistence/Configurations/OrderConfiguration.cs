using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.Side).HasConversion<int>().IsRequired();
        b.Property(x => x.OrderType).HasConversion<int>().IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();

        b.Property(x => x.Price).HasPrecision(18, 8);
        b.Property(x => x.Quantity).HasPrecision(18, 8);
        b.Property(x => x.FilledQuantity).HasPrecision(18, 8);
        b.Property(x => x.AverageFillPrice).HasPrecision(18, 8);
        b.Property(x => x.LockedPrice).HasPrecision(18, 8);
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.AssetId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => new { x.AssetId, x.Status, x.Side });  // matching engine için
    }
}