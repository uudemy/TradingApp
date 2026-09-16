using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingApp.Application.Abstractions;
using TradingApp.Application.Features.Market.Dtos;
using TradingApp.Domain.Enums;

namespace TradingApp.Application.Features.Market.GetAssets;

public sealed class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, IReadOnlyCollection<AssetDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);

    private readonly IAppDbContext _db;
    private readonly IMarketDataCache _cache;

    public GetAssetsQueryHandler(IAppDbContext db, IMarketDataCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<AssetDto>> Handle(GetAssetsQuery request, CancellationToken ct)
    {
        var cached = await _cache.GetAssetListAsync(ct);
        if (cached is not null) return cached;

        var assets = await _db.Assets
            .Where(a => a.IsActive)
            .OrderBy(a => a.Symbol)
            .Select(a => new
            {
                a.Id, a.Symbol, a.Name, a.AssetType, a.Currency,
                a.CurrentPrice, a.PreviousClose, a.DailyVolume, a.IsActive
            })
            .ToListAsync(ct);

        var result = assets.Select(a => new AssetDto(
            a.Id,
            a.Symbol,
            a.Name,
            a.AssetType.ToString(),
            a.Currency,
            a.CurrentPrice,
            a.PreviousClose,
            a.DailyVolume,
            a.IsActive,
            a.PreviousClose == 0m ? 0m
                : Math.Round((a.CurrentPrice - a.PreviousClose) / a.PreviousClose * 100m, 4)
        )).ToArray();

        await _cache.SetAssetListAsync(result, CacheTtl, ct);
        return result;
    }
}