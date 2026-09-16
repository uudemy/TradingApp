using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;

namespace TradingApp.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IAppDbContextFactory
{
    private readonly IDbContextFactory<AppDbContext> _inner;
    public AppDbContextFactory(IDbContextFactory<AppDbContext> inner) => _inner = inner;

    public IAppDbContext CreateDbContext() => _inner.CreateDbContext();
}