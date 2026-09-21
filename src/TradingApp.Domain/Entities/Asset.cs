using TradingApp.Domain.Common;
using TradingApp.Domain.Enums;
using TradingApp.Domain.ValueObjects;

namespace TradingApp.Domain.Entities;

public sealed class Asset : BaseEntity, IAggregateRoot
{
    public string Symbol { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public AssetType AssetType { get; private set; }
    public string Currency { get; private set; } = default!;
    public decimal CurrentPrice { get; private set; }
    public decimal PreviousClose { get; private set; }
    public decimal DailyVolume { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Asset() { } // EF

    public Asset(string symbol, string name, AssetType assetType, string currency, decimal initialPrice)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("invalid_symbol", "Symbol required.");
        if (initialPrice < 0)
            throw new DomainException("invalid_price", "Initial price cannot be negative.");

        Symbol = symbol.Trim().ToUpperInvariant();
        Name = name.Trim();
        AssetType = assetType;
        Currency = currency.ToUpperInvariant();
        CurrentPrice = initialPrice;
        PreviousClose = initialPrice;
    }

    /// <summary>Market data engine fiyat güncellediğinde çağrılır.</summary>
    public void UpdatePrice(decimal newPrice, decimal addedVolume = 0m)
    {
        if (newPrice <= 0)
            throw new DomainException("invalid_price", "Price must be positive.");

        CurrentPrice = newPrice;
        DailyVolume += addedVolume;
        Touch();
    }

    /// <summary>Polling sağlayıcısından gelen önceki kapanış fiyatını set eder.</summary>
    public void SetPreviousClose(decimal previousClose)
    {
        if (previousClose <= 0) return;
        PreviousClose = previousClose;
        Touch();
    }
    /// <summary>Gün sonu — PreviousClose güncellenir, DailyVolume sıfırlanır.</summary>
    public void CloseDay(decimal closePrice)
    {
        PreviousClose = closePrice;
        DailyVolume = 0m;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
}