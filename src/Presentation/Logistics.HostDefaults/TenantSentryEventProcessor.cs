using Logistics.Application.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using Sentry.Extensibility;

namespace Logistics.HostDefaults;

/// <summary>
/// Tags Sentry events raised during a request with the tenant id, so errors can be filtered per
/// carrier without sending user emails. Outside a request (Hangfire jobs) there is no tenant to
/// resolve; the job's own log properties carry the tenant name instead.
/// </summary>
public sealed class TenantSentryEventProcessor(IHttpContextAccessor httpContextAccessor) : ISentryEventProcessor
{
    public const string TenantIdTag = "tenant_id";

    public SentryEvent? Process(SentryEvent sentryEvent)
    {
        var accessor = httpContextAccessor.HttpContext?.RequestServices.GetService<ICurrentTenantAccessor>();
        if (accessor is null)
        {
            return sentryEvent;
        }

        try
        {
            // The accessor caches the tenant per request, so this is normally a field read.
            sentryEvent.SetTag(TenantIdTag, accessor.GetCurrentTenant().Id.ToString());
        }
        catch (Exception)
        {
            // No tenant on this request (anonymous endpoint, bad header, expired subscription):
            // the event is still worth sending untagged.
        }

        return sentryEvent;
    }
}
