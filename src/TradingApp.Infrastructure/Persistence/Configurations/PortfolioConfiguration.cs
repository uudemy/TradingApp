using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingApp.Domain.Entities;

namespace TradingApp.Infrastructure.Persistence.Configurations;

public sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> b)
    {
        b.ToTable("portfolios");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId).IsUnique();

        b.HasMany(x => x.Positions).WithOne().HasForeignKey(p => p.PortfolioId).OnDelete(DeleteBehavior.Cascade);
    }
}