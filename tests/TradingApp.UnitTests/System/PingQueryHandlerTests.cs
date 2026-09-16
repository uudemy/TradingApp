using TradingApp.Application.System;

namespace TradingApp.UnitTests.System;

public class PingQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_Status()
    {
        var handler = new PingQueryHandler();

        var response = await handler.Handle(new PingQuery(), CancellationToken.None);

        Assert.Equal("ok", response.Status);
        Assert.Equal("1.0.0-phase1", response.Version);
        Assert.True(response.TimestampUtc <= DateTimeOffset.UtcNow);
    }
}