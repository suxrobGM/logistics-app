using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance;

public class DataRetentionServiceTests
{
    /// <summary>
    /// Read state is per user now, so a notification nobody opened must age out like any other.
    /// </summary>
    [Fact]
    public async Task PurgeAsync_PurgesNotificationsByAgeOnly()
    {
        var masterUow = Substitute.For<IMasterUnitOfWork>();
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        masterUow.Repository<Tenant>().GetListAsync(ct: Arg.Any<CancellationToken>()).Returns([TestTenant.Create()]);

        var old = new Notification
        {
            Title = "Old",
            Message = "Never read",
            CreatedDate = DateTime.UtcNow - PrivacyDefaults.NotificationRetention - TimeSpan.FromDays(1)
        };
        var recent = new Notification { Title = "Recent", Message = "Never read" };
        var notificationRepo = Substitute.For<ITenantRepository<Notification, Guid>>();
        notificationRepo.Query().Returns(_ => new List<Notification> { old, recent }.BuildMock());
        tenantUow.Repository<Notification>().Returns(notificationRepo);

        var sessionRepo = Substitute.For<ITenantRepository<AgentSession, Guid>>();
        sessionRepo.Query().Returns(_ => new List<AgentSession>().BuildMock());
        tenantUow.Repository<AgentSession>().Returns(sessionRepo);

        await new DataRetentionService(masterUow, tenantUow, NullLogger<DataRetentionService>.Instance)
            .PurgeAsync(CancellationToken.None);

        notificationRepo.Received(1).Delete(old);
        notificationRepo.DidNotReceive().Delete(recent);
    }
}
