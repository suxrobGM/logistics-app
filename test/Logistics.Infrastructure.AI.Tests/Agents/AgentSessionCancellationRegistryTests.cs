using Logistics.Infrastructure.AI.Agents;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Agents;

public class AgentSessionCancellationRegistryTests
{
    private readonly AgentSessionCancellationRegistry _sut = new();

    [Fact]
    public void Register_ReturnsLinkedToken()
    {
        var sessionId = Guid.NewGuid();
        var token = _sut.Register(sessionId, CancellationToken.None);

        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public async Task Register_DeadlineElapses_CancelsToken()
    {
        var token = _sut.Register(Guid.NewGuid(), CancellationToken.None, TimeSpan.FromMilliseconds(50));

        Assert.False(token.IsCancellationRequested);

        // Wait on the token rather than a fixed delay: a loaded CI runner can fire the timer late.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Task.Delay(TimeSpan.FromSeconds(10), token));
    }

    [Fact]
    public void Register_NoDeadline_LeavesTokenOpen()
    {
        var token = _sut.Register(Guid.NewGuid(), CancellationToken.None, deadline: null);

        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public void TryCancel_RegisteredSession_CancelsToken()
    {
        var sessionId = Guid.NewGuid();
        var token = _sut.Register(sessionId, CancellationToken.None);

        var result = _sut.TryCancel(sessionId);

        Assert.True(result);
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void TryCancel_UnknownSession_ReturnsFalse()
    {
        var result = _sut.TryCancel(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void Unregister_RemovesSession()
    {
        var sessionId = Guid.NewGuid();
        _sut.Register(sessionId, CancellationToken.None);

        _sut.Unregister(sessionId);

        Assert.False(_sut.TryCancel(sessionId));
    }

    [Fact]
    public void Register_LinkedToExternalToken_CancelsWhenExternalCancels()
    {
        var cts = new CancellationTokenSource();
        var sessionId = Guid.NewGuid();
        var token = _sut.Register(sessionId, cts.Token);

        cts.Cancel();

        Assert.True(token.IsCancellationRequested);
    }
}
