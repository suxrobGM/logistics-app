using Logistics.Application.Tests.TestKit;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Platform.Notifications.Commands;
using Logistics.Application.Modules.Platform.Notifications.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class NotificationReadTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly List<Notification> _notifications = [];
    private readonly List<NotificationRead> _reads = [];
    private readonly Guid _userId = Guid.NewGuid();

    public NotificationReadTests()
    {
        _tenantUow.Stub<Notification>(_notifications);

        _tenantUow.Stub<NotificationRead>(_reads);

        _currentUser.GetUserId().Returns(_userId);
    }

    /// <summary>One shared IsRead flag used to clear a notification for the whole tenant.</summary>
    [Fact]
    public async Task GetNotifications_ReadByAnotherUser_StaysUnreadForTheCaller()
    {
        var mine = Add("Load 42 delivered");
        var theirs = Add("Load 43 delivered");
        _reads.Add(new NotificationRead { NotificationId = mine.Id, UserId = _userId });
        _reads.Add(new NotificationRead { NotificationId = theirs.Id, UserId = Guid.NewGuid() });

        var result = await new GetNotificationsHandler(_tenantUow, _currentUser).Handle(
            new GetNotificationsQuery { StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) },
            CancellationToken.None);

        var dtos = result.Value!.ToDictionary(n => n.Id!.Value);
        Assert.True(dtos[mine.Id].IsRead);
        Assert.False(dtos[theirs.Id].IsRead);
    }

    [Fact]
    public async Task MarkRead_InsertsReadRowsForTheCallerOnly()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        FormattableString? sql = null;
        await _tenantUow.ExecuteSqlAsync(Arg.Do<FormattableString>(s => sql = s), Arg.Any<CancellationToken>());

        var result = await new MarkNotificationsReadHandler(_tenantUow, _currentUser).Handle(
            new MarkNotificationsReadCommand { Ids = ids }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(sql);
        Assert.Contains("INSERT INTO notification_reads", sql.Format);
        Assert.Contains("ON CONFLICT (notification_id, user_id) DO NOTHING", sql.Format);
        Assert.Equal(_userId, sql.GetArgument(0));
        Assert.Equal(ids, (Guid[])sql.GetArgument(2)!);
    }

    [Fact]
    public async Task MarkRead_NoUser_Fails()
    {
        _currentUser.GetUserId().Returns((Guid?)null);

        var result = await new MarkNotificationsReadHandler(_tenantUow, _currentUser).Handle(
            new MarkNotificationsReadCommand { Ids = [Guid.NewGuid()] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceiveWithAnyArgs().ExecuteSqlAsync(default!, default);
    }

    private Notification Add(string title)
    {
        var notification = new Notification { Title = title, Message = title };
        _notifications.Add(notification);
        return notification;
    }
}
