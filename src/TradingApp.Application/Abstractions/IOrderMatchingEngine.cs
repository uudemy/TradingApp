using TradingApp.Domain.Entities;

namespace TradingApp.Application.Abstractions;

/// <summary>
/// Emir eşleştirme motoru. Bir emir veritabanına yazıldıktan sonra çağrılır,
/// karşı taraftaki emirleri bulur ve trade'leri gerçekleştirir.
/// </summary>
public interface IOrderMatchingEngine
{
    Task MatchAsync(Order incomingOrder, CancellationToken ct = default);
}