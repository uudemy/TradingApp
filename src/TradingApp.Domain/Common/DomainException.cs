namespace TradingApp.Domain.Common;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message)
        => Code = code;

    public DomainException(string message) : this("domain_error", message) { }
}