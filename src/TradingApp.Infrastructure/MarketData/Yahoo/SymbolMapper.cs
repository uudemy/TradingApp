using Microsoft.Extensions.Options;
using TradingApp.Infrastructure.MarketData.Options;

namespace TradingApp.Infrastructure.MarketData.Yahoo;

/// <summary>
/// Bizim sembolümüzü (THYAO) Yahoo'nun beklediği formata (THYAO.IS) çevirir.
/// </summary>
public sealed class SymbolMapper
{
    private readonly Dictionary<string, string> _map;

    public SymbolMapper(IOptions<MarketDataOptions> options)
        => _map = new Dictionary<string, string>(options.Value.Yahoo.SymbolMappings, StringComparer.OrdinalIgnoreCase);

    public string ToYahoo(string symbol)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (_map.TryGetValue(symbol, out var mapped)) return mapped;

        // Fallback: kripto -USD, BIST .IS tahmini
        if (symbol.Length <= 5 && symbol.All(char.IsLetter)) return symbol + ".IS";
        return symbol;
    }

    public string? FromYahoo(string yahooSymbol)
    {
        yahooSymbol = yahooSymbol.Trim().ToUpperInvariant();
        foreach (var (ours, theirs) in _map)
        {
            if (string.Equals(theirs, yahooSymbol, StringComparison.OrdinalIgnoreCase))
                return ours;
        }
        return null;
    }
}