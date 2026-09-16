using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TradingApp.Application.Abstractions;
using TradingApp.Infrastructure.Auth;
using TradingApp.Infrastructure.Identity;
using TradingApp.Infrastructure.MarketData;
using TradingApp.Infrastructure.Persistence;
using TradingApp.Infrastructure.Time;
using TradingApp.Infrastructure.Matching;
namespace TradingApp.Infrastructure;
using TradingApp.Infrastructure.MarketData.Options;
using TradingApp.Infrastructure.MarketData.Yahoo;
using Microsoft.Extensions.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------- PostgreSQL ----------
        var postgres = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres tanımlı değil.");

        // Sadece Factory — Singleton. AddDbContext KULLANMIYORUZ (çakışır).
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(postgres, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // AppDbContext'i Scoped olarak fabrikadan üret (handler'lar için)
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

        // ---------- Redis ----------
        var redis = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis tanımlı değil.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { redis },
                AbortOnConnectFail = false
            }));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redis;
            options.InstanceName = "TradingApp:";
        });

        // ---------- Auth & Time ----------
        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
                // ---------- Matching Engine ----------
        services.AddScoped<IOrderMatchingEngine, OrderMatchingEngine>();

        // ---------- Market Data ----------
        services.Configure<MarketDataOptions>(configuration.GetSection(MarketDataOptions.SectionName));

        services.AddScoped<IMarketDataCache, RedisMarketDataCache>();
        services.AddSingleton<ICandleStore, CandleStore>();

        // Mock her zaman kayıtlı (fallback için)
        services.AddScoped<MockMarketDataProvider>();

        // Yahoo + HTTP client
        services.AddHttpClient<YahooFinanceClient>();
        services.AddSingleton<SymbolMapper>();
        services.AddScoped<YahooFinanceMarketDataProvider>();

        // Provider seçimi + fallback
        var provider = configuration[$"{MarketDataOptions.SectionName}:Provider"] ?? "Mock";
        var fallback = configuration.GetValue<bool>($"{MarketDataOptions.SectionName}:FallbackToMock");

        services.AddScoped<IMarketDataProvider>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var mock = sp.GetRequiredService<MockMarketDataProvider>();

            if (string.Equals(provider, "Yahoo", StringComparison.OrdinalIgnoreCase))
            {
                var yahoo = sp.GetRequiredService<YahooFinanceMarketDataProvider>();
                if (fallback)
                {
                    return new FallbackMarketDataProvider(
                        yahoo, mock,
                        loggerFactory.CreateLogger<FallbackMarketDataProvider>());
                }
                return yahoo;
            }

            return mock;
        });

        // Background polling (Yahoo) veya simülasyon (Mock)
        if (string.Equals(provider, "Yahoo", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHostedService<MarketDataPollingService>();
        }
        else
        {
            services.AddHostedService<MarketDataSimulationService>();
        }
        return services;
    }
}