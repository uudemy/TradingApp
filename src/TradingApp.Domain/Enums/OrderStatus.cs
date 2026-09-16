namespace TradingApp.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Open = 2,
    PartiallyFilled = 3,
    Filled = 4,
    Cancelled = 5,
    Rejected = 6
}