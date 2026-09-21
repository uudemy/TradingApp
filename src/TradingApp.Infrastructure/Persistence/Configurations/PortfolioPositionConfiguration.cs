using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class PortfolioPositionConfiguration : IEntityTypeConfiguration<PortfolioPosition>
{
    public void Configure(EntityTypeBuilder<PortfolioPosition> b)
    {
        b.ToTable("portfolio_positions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Quantity).HasPrecision(18, 8);
        b.Property(x => x.LockedQuantity).HasPrecision(18, 8);
        b.Property(x => x.AverageCost).HasPrecision(18, 8);
        b.Property(x => x.RealizedPnl).HasPrecision(18, 8);
        b.Property(x => x.Notes).HasMaxLength(500);

        b.Ignore(x => x.AvailableQuantity);

        b.HasIndex(x => new { x.PortfolioId, x.AssetId }).IsUnique();
    }
}