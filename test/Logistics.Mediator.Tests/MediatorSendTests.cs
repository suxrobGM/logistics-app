using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class MediatorSendTests
{
    private readonly Trace _trace = new();

    private IMediator Build()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddMediator(typeof(Ping).Assembly, type => type != typeof(SecondDuplicateHandler));
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_RegisteredHandler_ReturnsResponse()
    {
        var response = await Build().Send(new Ping("hello"), CancellationToken.None);

        Assert.Equal("hello", response);
    }

    /// <summary>Handlers are internal sealed, so the scan must use GetTypes, not GetExportedTypes.</summary>
    [Fact]
    public async Task Send_InternalSealedHandler_IsDiscovered()
    {
        var response = await Build().Send(new OtherPing(), CancellationToken.None);

        Assert.Equal("other", response);
    }

    [Fact]
    public async Task Send_NoHandlerRegistered_ThrowsNamingTheRequest()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Build().Send(new Unhandled(), CancellationToken.None));

        Assert.Contains(nameof(Unhandled), ex.Message, StringComparison.Ordinal);
    }

    /// <summary>The path CommandEnqueuerJob takes: the response type is not known statically.</summary>
    [Fact]
    public async Task SendObject_RequestTypedAsIBaseRequest_DispatchesByRuntimeType()
    {
        IBaseRequest request = new Ping("boxed");

        var response = await Build().Send(request, CancellationToken.None);

        Assert.Equal("boxed", response);
    }
}
