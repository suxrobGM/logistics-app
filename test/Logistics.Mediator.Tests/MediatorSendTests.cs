using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class MediatorSendTests
{
    private readonly Trace _trace = new();

    private IMediator Build(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddMediator(typeof(Ping).Assembly, type => type != typeof(SecondDuplicateHandler));
        configure?.Invoke(services);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_RegisteredHandler_ReturnsResponse()
    {
        var sut = Build();

        var response = await sut.Send(new Ping("hello"), CancellationToken.None);

        Assert.Equal("hello", response);
        Assert.Equal(["handler"], _trace.Entries);
    }

    [Fact]
    public async Task Send_InternalSealedHandler_IsDiscovered()
    {
        var sut = Build();

        var response = await sut.Send(new OtherPing(), CancellationToken.None);

        Assert.Equal("other", response);
    }

    [Fact]
    public async Task Send_NoHandlerRegistered_ThrowsNamingTheRequest()
    {
        var sut = Build();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Send(new Unhandled(), CancellationToken.None));

        Assert.Contains(nameof(Unhandled), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendObject_RequestTypedAsIBaseRequest_DispatchesByRuntimeType()
    {
        var sut = Build();
        IBaseRequest request = new Ping("boxed");

        var response = await sut.Send(request, CancellationToken.None);

        Assert.Equal("boxed", response);
    }

    [Fact]
    public async Task Send_NullRequest_Throws()
    {
        var sut = Build();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.Send<string>(null!, CancellationToken.None));
    }
}
