using MediatR;
using TradingApp.Application.Features.Analysis.Dtos;

namespace TradingApp.Application.Features.Analysis.AnalyzeSymbol;

public sealed record AnalyzeSymbolQuery(string Symbol, string Interval = "1D", int Limit = 200)
    : IRequest<AnalysisDto>;