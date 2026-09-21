using MediatR;
using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Application.Features.Analysis.Scan;

public sealed record ScanAnalysisQuery(string? AssetType = null, int TopCount = 10)
    : IRequest<AnalysisScanDto>;