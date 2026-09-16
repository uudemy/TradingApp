namespace TradingApp.Application.Abstractions;

public interface IAppDbContextFactory
{
    IAppDbContext CreateDbContext();
}