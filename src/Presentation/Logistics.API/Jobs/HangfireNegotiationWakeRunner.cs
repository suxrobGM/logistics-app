using Hangfire;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Negotiation;

namespace Logistics.API.Jobs;

public class HangfireNegotiationWakeRunner(IBackgroundJobClient jobClient)
    : IDelayedBackgroundJobRunner<NegotiationWakeRequest>
{
    public void Schedule(NegotiationWakeRequest request, TimeSpan delay)
    {
        jobClient.Schedule<NegotiationWakeJob>(
            job => job.RunAsync(request.TenantId, request.NegotiationId, CancellationToken.None),
            delay);
    }
}
