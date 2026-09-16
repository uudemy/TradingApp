using MediatR;

namespace TradingApp.Application.System;

public sealed record PingQuery : IRequest<PingResponse>;

public sealed record PingResponse(string Status, DateTimeOffset TimestampUtc, string Version);

public sealed class PingQueryHandler : IRequestHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken)
        => Task.FromResult(new PingResponse("ok", DateTimeOffset.UtcNow, "1.0.0-phase1"));
}