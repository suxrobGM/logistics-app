using System.Net.Http.Json;
using Hangfire;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Domain.Options;
using Microsoft.Extensions.Options;

namespace Logistics.API.Jobs;

/// <summary>
/// Posts the daily license heartbeat for this deployment to the author's API. Global job, no
/// tenant fan-out. Never throws: a receiver outage must not surface as a failed job.
/// </summary>
public class ProductLicenseHeartbeatJob(
    ILogger<ProductLicenseHeartbeatJob> logger,
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<ProductLicenseOptions> options)
{
    public const string JobId = "product-license-heartbeat";
    public const string HttpClientName = "product-license-heartbeat";

    // Restarts re-trigger the job; anything sent within this window is not sent again.
    private static readonly TimeSpan MinInterval = TimeSpan.FromHours(20);

    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<ProductLicenseHeartbeatJob>(
            JobId,
            job => job.SendAsync(CancellationToken.None),
            Cron.Daily(3));
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task SendAsync(CancellationToken ct)
    {
        var config = options.Value;
        if (!config.HeartbeatEnabled)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var heartbeats = scope.ServiceProvider.GetRequiredService<IProductLicenseHeartbeatService>();

        var lastSent = await heartbeats.GetLastSentAtAsync(ct);
        if (lastSent is { } at && DateTime.UtcNow - at < MinInterval)
        {
            return;
        }

        var payload = await heartbeats.BuildHeartbeatAsync(ct);

        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsJsonAsync(config.HeartbeatUrl, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("License heartbeat to {Url} returned {StatusCode}", config.HeartbeatUrl, (int)response.StatusCode);
                return;
            }

            await heartbeats.MarkSentAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "License heartbeat to {Url} failed", config.HeartbeatUrl);
        }
    }
}
