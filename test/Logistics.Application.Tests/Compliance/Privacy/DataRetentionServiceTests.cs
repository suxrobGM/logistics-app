using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Logistics.Application.Modules.Compliance.Privacy.Constants;

namespace Logistics.Application.Tests.Compliance.Privacy;

public class DataRetentionServiceTests
{
    /// <summary>
    /// Read state is per user now, so a notification nobody opened must age out like any other.
    /// </summary>
    [Fact]
    public async Task PurgeAsync_PurgesNotificationsByAgeOnly()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();

        var old = new Notification
        {
            Title = "Old",
            Message = "Never read",
            CreatedDate = DateTime.UtcNow - PrivacyDefaults.NotificationRetention - TimeSpan.FromDays(1)
        };
        var recent = new Notification { Title = "Recent", Message = "Never read" };
        var notificationRepo = tenantUow.Stub<Notification>(old, recent);

        tenantUow.Stub<AgentSession>();

        await new DataRetentionService(tenantUow, NullLogger<DataRetentionService>.Instance)
            .PurgeAsync(TestTenant.Create(), CancellationToken.None);

        notificationRepo.Received(1).Delete(old);
        notificationRepo.DidNotReceive().Delete(recent);
    }
}
