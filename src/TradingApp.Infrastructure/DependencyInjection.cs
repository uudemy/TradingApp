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
using TradingApp.Infrastructure.MarketData.BistDataService;
using TradingApp.Infrastructure.MarketData.CoinMarketCap;
using TradingApp.Infrastructure.MarketData.Stooq;
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

                // BIST Data Service
        services.AddHttpClient<BistDataServiceClient>();
        services.AddScoped<BistDataServiceProvider>();

        // CoinMarketCap
        services.AddHttpClient<CoinMarketCapClient>();
        services.AddScoped<CoinMarketCapProvider>();

        // Stooq
        services.AddHttpClient<StooqClient>();
        services.AddScoped<StooqProvider>();

        // Provider seçimi + fallback
        var provider = configuration[$"{MarketDataOptions.SectionName}:Provider"] ?? "Mock";
        var fallback = configuration.GetValue<bool>($"{MarketDataOptions.SectionName}:FallbackToMock");

              services.AddScoped<IMarketDataProvider>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var log = loggerFactory.CreateLogger<FallbackMarketDataProvider>();

            // Sıralı fallback zinciri
            var chain = new List<IMarketDataProvider>();

            // 1. Birincil (config'ten)
            if (string.Equals(provider, "Yahoo", StringComparison.OrdinalIgnoreCase))
                chain.Add(sp.GetRequiredService<YahooFinanceMarketDataProvider>());

            // 2. BIST Data Service (sadece TRY assetler için ideal)
            chain.Add(sp.GetRequiredService<BistDataServiceProvider>());

            // 3. CoinMarketCap (kripto)
            chain.Add(sp.GetRequiredService<CoinMarketCapProvider>());

            // 4. Stooq (ABD hisseleri)
            chain.Add(sp.GetRequiredService<StooqProvider>());

            // 5. Mock (son çare)
            if (fallback)
                chain.Add(sp.GetRequiredService<MockMarketDataProvider>());

            // Zinciri birleştir
            IMarketDataProvider result = chain[0];
            for (int i = 1; i < chain.Count; i++)
                result = new FallbackMarketDataProvider(result, chain[i], log);

            return result;
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