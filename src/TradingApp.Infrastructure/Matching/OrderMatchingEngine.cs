using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Application.Abstractions;
using TradingApp.Domain.Common;
using TradingApp.Domain.Entities;
using TradingApp.Domain.Enums;
using TradingApp.Infrastructure.Persistence;

namespace TradingApp.Infrastructure.Matching;

/// <summary>
/// Basit fiyat-öncelikli eşleştirme motoru.
/// BUY emri → en düşük fiyatlı SELL emirleriyle eşleşir.
/// SELL emri → en yüksek fiyatlı BUY emirleriyle eşleşir.
/// Eşit fiyat → önce gelen emir önceliklidir (FIFO).
/// Trade fiyatı = pasif (resting) emrin fiyatı.
/// </summary>
public sealed class OrderMatchingEngine : IOrderMatchingEngine
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderMatchingEngine> _logger;

    public OrderMatchingEngine(AppDbContext db, ILogger<OrderMatchingEngine> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task MatchAsync(Order incomingOrder, CancellationToken ct = default)
    {
        // Emir zaten dolmuş/iptal ise çık
        if (incomingOrder.Status is OrderStatus.Filled
            or OrderStatus.Cancelled
            or OrderStatus.Rejected)
            return;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var oppositeSide = incomingOrder.Side == OrderSide.Buy
                ? OrderSide.Sell
                : OrderSide.Buy;

            // Karşı taraf emirleri çek
            var query = _db.Orders
                .Where(o => o.AssetId == incomingOrder.AssetId
                            && o.Side == oppositeSide
                            && o.Id != incomingOrder.Id
                            && (o.Status == OrderStatus.Open || o.Status == OrderStatus.PartiallyFilled));

            // Fiyat-öncelik sıralaması
            query = incomingOrder.Side == OrderSide.Buy
                ? query.OrderBy(o => o.Price).ThenBy(o => o.CreatedAt)      // en ucuz satıcı önce
                : query.OrderByDescending(o => o.Price).ThenBy(o => o.CreatedAt); // en yüksek alıcı önce

            var book = await query.ToListAsync(ct);
            var asset = await _db.Assets.FirstAsync(a => a.Id == incomingOrder.AssetId, ct);

            foreach (var resting in book)
            {
                if (incomingOrder.RemainingQuantity <= 0m) break;

                // Fiyat uygunluk kontrolü (limit tarafı için)
                var crosses = incomingOrder.OrderType == OrderType.Market
                    || (incomingOrder.Side == OrderSide.Buy
                        ? incomingOrder.Price >= resting.Price
                        : incomingOrder.Price <= resting.Price);

                if (!crosses) break; // book sıralı olduğundan gerisi de uymaz

                var fillQty = Math.Min(incomingOrder.RemainingQuantity, resting.RemainingQuantity);
                if (fillQty <= 0m) continue;

                // Trade fiyatı: market emir varsa restingle, yoksa resting fiyatı
                var tradePrice = resting.OrderType == OrderType.Limit
                    ? resting.Price
                    : asset.CurrentPrice;

                await ExecuteFillAsync(incomingOrder, resting, fillQty, tradePrice, ct);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "Matching complete: order {OrderId} status={Status} filled={Filled}/{Qty}",
                incomingOrder.Id, incomingOrder.Status,
                incomingOrder.FilledQuantity, incomingOrder.Quantity);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Matching failed for order {OrderId}", incomingOrder.Id);
            throw;
        }
    }

    private async Task ExecuteFillAsync(
        Order incoming, Order resting, decimal qty, decimal price, CancellationToken ct)
    {
        var buyOrder = incoming.Side == OrderSide.Buy ? incoming : resting;
        var sellOrder = incoming.Side == OrderSide.Sell ? incoming : resting;

        // --- Emirleri güncelle ---
        incoming.Fill(qty, price);
        resting.Fill(qty, price);

        // --- Cüzdanlar ---
        var buyerWallet = await _db.Wallets
            .FirstAsync(w => w.UserId == buyOrder.UserId
                          && w.Currency == _db.Assets.First(a => a.Id == buyOrder.AssetId).Currency, ct);

        var sellerWallet = await _db.Wallets
            .FirstAsync(w => w.UserId == sellOrder.UserId
                          && w.Currency == _db.Assets.First(a => a.Id == sellOrder.AssetId).Currency, ct);

        // Buyer: kilitli tutarı çöz, sonra gerçek maliyeti düş
        var lockedForFill = buyOrder.LockedPrice * qty; // placement'ta kilitlenen birim fiyat        buyerWallet.Unlock(lockedForFill);
        buyerWallet.Withdraw(price * qty);

        // Seller: alım bedelini al
        sellerWallet.Credit(price * qty);

        // --- Portfolio pozisyonları ---
        var buyerPosition = await GetOrCreatePositionAsync(buyOrder.UserId, buyOrder.AssetId, ct);
        var sellerPosition = await GetOrCreatePositionAsync(sellOrder.UserId, sellOrder.AssetId, ct);

        buyerPosition.ApplyBuy(qty, price);
        sellerPosition.ApplySell(qty, price);

        // --- Trade kaydı ---
        var trade = new Trade(
            buyOrder.Id,
            sellOrder.Id,
            buyOrder.AssetId,
            buyOrder.UserId,
            sellOrder.UserId,
            price,
            qty);

        _db.Trades.Add(trade);

        _logger.LogInformation(
            "Trade executed: buyOrder={BuyId} sellOrder={SellId} qty={Qty} price={Price}",
            buyOrder.Id, sellOrder.Id, qty, price);
    }

    private async Task<PortfolioPosition> GetOrCreatePositionAsync(
        Guid userId, Guid assetId, CancellationToken ct)
    {
        var portfolio = await _db.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new DomainException("portfolio_missing", $"Portfolio for user {userId} missing.");

        var position = await _db.PortfolioPositions
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolio.Id && p.AssetId == assetId, ct);

        if (position is null)
        {
            position = new PortfolioPosition(portfolio.Id, assetId, 0m, 0m);
            _db.PortfolioPositions.Add(position);
            await _db.SaveChangesAsync(ct);
        }

        return position;
    }
}