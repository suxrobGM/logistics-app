using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class MediatorPipelineTests
{
    private readonly Trace _trace = new();

    private IMediator Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddMediator(typeof(Ping).Assembly, type => type != typeof(SecondDuplicateHandler));
        configure(services);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_MultipleBehaviours_RunsFirstRegisteredOutermost()
    {
        var sut = Build(services =>
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OuterBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InnerBehaviour<,>));
        });

        await sut.Send(new Ping(), CancellationToken.None);

        Assert.Equal(["A>", "B>", "handler", "B<", "A<"], _trace.Entries);
    }

    /// <summary>How FeatureCheckBehaviour denies a request.</summary>
    [Fact]
    public async Task Send_BehaviourDoesNotCallNext_HandlerNeverRuns()
    {
        var sut = Build(services =>
            services.AddTransient<IPipelineBehavior<Ping, string>, ShortCircuitBehaviour<Ping>>());

        var response = await sut.Send(new Ping(), CancellationToken.None);

        Assert.Equal("short-circuited", response);
        Assert.Equal(["denied"], _trace.Entries);
    }
}
