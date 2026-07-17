using Logistics.Application.Abstractions.Geocoding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logistics.Infrastructure.Routing.Geospatial;

/// <summary>
/// Builds the jurisdiction index at startup. Constructing the resolver parses ~580 KB of embedded
/// GeoJSON and builds an STRtree; without this the cost lands on whichever request or ELD ping
/// happens to touch the singleton first.
/// </summary>
internal sealed class JurisdictionResolverWarmup(IServiceProvider services) : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        // Resolving the singleton is what builds the index.
        services.GetRequiredService<IJurisdictionResolver>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
