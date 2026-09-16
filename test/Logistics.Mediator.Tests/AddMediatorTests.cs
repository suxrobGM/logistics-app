using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class AddMediatorTests
{
    private static ServiceCollection NewServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Trace());
        return services;
    }

    [Fact]
    public void AddMediator_WithoutAssembly_RegistersMediatorOnly()
    {
        var services = NewServices();

        services.AddMediator();

        Assert.Contains(services, d => d.ServiceType == typeof(IMediator));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRequestHandler<Ping, string>));
    }

    /// <summary>
    ///     A host that composes one slice of an assembly relies on this. Without the filter it
    ///     registers notification handlers whose dependencies it never wires, and the failure only
    ///     surfaces on the first save that raises an event.
    /// </summary>
    [Fact]
    public void AddMediator_TypeFilter_ExcludesNonMatchingHandlers()
    {
        var services = NewServices();

        services.AddMediator(typeof(Ping).Assembly, type => type == typeof(PingHandler));

        Assert.Contains(services, d => d.ImplementationType == typeof(PingHandler));
        Assert.DoesNotContain(services, d => d.ImplementationType == typeof(FirstListener));
    }

    [Fact]
    public void AddMediator_CalledTwice_DoesNotDuplicateRegistrations()
    {
        var services = NewServices();
        Func<Type, bool> filter = type => type != typeof(SecondDuplicateHandler);

        services.AddMediator(typeof(Ping).Assembly, filter);
        var afterFirst = services.Count;
        services.AddMediator(typeof(Ping).Assembly, filter);

        Assert.Equal(afterFirst, services.Count);
        Assert.Single(services, d => d.ServiceType == typeof(IMediator));
    }

    [Fact]
    public void AddMediator_TwoHandlersForOneRequest_Throws()
    {
        var services = NewServices();

        var ex = Assert.Throws<InvalidOperationException>(
            () => services.AddMediator(typeof(Ping).Assembly));

        Assert.Contains(nameof(Duplicated), ex.Message, StringComparison.Ordinal);
    }

}
