using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> b)
    {
        b.ToTable("trades");
        b.HasKey(x => x.Id);

        b.Property(x => x.Price).HasPrecision(18, 8);
        b.Property(x => x.Quantity).HasPrecision(18, 8);

        b.HasIndex(x => x.AssetId);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.BuyOrderId);
        b.HasIndex(x => x.SellOrderId);
        b.HasIndex(x => x.BuyerUserId);
        b.HasIndex(x => x.SellerUserId);
    }
}