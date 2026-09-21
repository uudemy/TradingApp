using MediatR;
using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Application.Features.Analysis.Screener;

public sealed record ScreenerQuery(
    string? AssetType = null,
    string Interval = "1D",
    int? MinScore = null,
    int? MaxScore = null,
    decimal? MinRsi = null,
    decimal? MaxRsi = null,
    string? Category = null,
    int Limit = 200
) : IRequest<ScreenerResultDto>;