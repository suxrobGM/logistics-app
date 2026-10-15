using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Operations.Containers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Containers;

public class ContainerDeadlineAlertServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Container, Guid> _containerRepo =
        Substitute.For<ITenantRepository<Container, Guid>>();
    private readonly INotificationService _notifications = Substitute.For<INotificationService>();
    private readonly List<Container> _containers = [];
    private readonly ContainerDeadlineAlertService _sut;

    public ContainerDeadlineAlertServiceTests()
    {
        _tenantUow.Repository<Container>().Returns(_containerRepo);
        _containerRepo.Query().Returns(_ => _containers.BuildMock());
        _sut = new ContainerDeadlineAlertService(_tenantUow, _notifications);
    }

    private Container Add(string number, Action<Container> setup)
    {
        var container = new Container { Number = number, IsoType = ContainerIsoType.Hc40 };
        setup(container);
        _containers.Add(container);
        return container;
    }

    #region Selection

    [Fact]
    public async Task SendAlerts_SelectsOnlyRunningClocksWithinADayOrPast()
    {
        Add("AAAU0000001", c => c.LastFreeDay = Today);                        // due today
        Add("AAAU0000002", c => c.LastFreeDay = Today.AddDays(1));             // due tomorrow
        Add("AAAU0000003", c => c.LastFreeDay = Today.AddDays(-3));            // overdue
        Add("AAAU0000004", c => c.LastFreeDay = Today.AddDays(2));             // slack
        Add("AAAU0000005", c =>                                                // demurrage stopped
        {
            c.LastFreeDay = Today.AddDays(-3);
            c.PickedUpAt = Now.AddDays(-4);
        });
        Add("AAAU0000006", c => c.EmptyReturnDeadline = Today.AddDays(-1));    // per diem overdue
        Add("AAAU0000007", c =>                                                // per diem stopped
        {
            c.EmptyReturnDeadline = Today.AddDays(-1);
            c.MarkStreetTurned("EXP-1", Now.AddDays(-2));
        });
        Add("AAAU0000008", _ => { });                                          // no deadlines

        var sent = await _sut.SendAlertsAsync(Now);

        Assert.Equal(4, sent);
        await _notifications.Received(1).SendNotificationAsync(Arg.Is<string>(t => t.Contains("AAAU0000001")), Arg.Any<string>());
        await _notifications.Received(1).SendNotificationAsync(Arg.Is<string>(t => t.Contains("AAAU0000002")), Arg.Any<string>());
        await _notifications.Received(1).SendNotificationAsync(Arg.Is<string>(t => t.Contains("AAAU0000003")), Arg.Any<string>());
        await _notifications.Received(1).SendNotificationAsync(Arg.Is<string>(t => t.Contains("AAAU0000006")), Arg.Any<string>());
    }

    [Fact]
    public async Task SendAlerts_ReturnedBox_IsNotAlertedForPerDiem()
    {
        Add("AAAU0000009", c =>
        {
            c.EmptyReturnDeadline = Today.AddDays(-1);
            c.MarkReturned(new Terminal
            {
                Name = "Depot",
                Code = "USLAX",
                CountryCode = "US",
                Type = TerminalType.InlandDepot,
                Address = new Logistics.Domain.Primitives.ValueObjects.Address
                {
                    Line1 = "1 Yard", City = "Carson", State = "CA", ZipCode = "90745", Country = "US"
                }
            });
        });

        var sent = await _sut.SendAlertsAsync(Now);

        Assert.Equal(0, sent);
    }

    #endregion

    #region Message

    [Fact]
    public void BuildAlert_Overdue_StatesDaysAndAccruedCharge()
    {
        var container = Add("MSCU1234567", c =>
        {
            c.LastFreeDay = Today.AddDays(-2);
            c.DemurrageRatePerDay = 150m;
        });

        var alert = ContainerDeadlineAlertService.BuildAlert(container, container.GetFreeTime(Now));

        Assert.NotNull(alert);
        Assert.Contains("past its free time", alert.Value.Title);
        Assert.Contains("2 day(s) past the last free day", alert.Value.Message);
        Assert.Contains("300.00 demurrage accrued", alert.Value.Message);
    }

    [Fact]
    public void BuildAlert_DueTomorrow_SaysTomorrow()
    {
        var container = Add("MSCU1234567", c => c.EmptyReturnDeadline = Today.AddDays(1));

        var alert = ContainerDeadlineAlertService.BuildAlert(container, container.GetFreeTime(Now));

        Assert.NotNull(alert);
        Assert.Contains("ends soon", alert.Value.Title);
        Assert.Contains("empty return deadline is tomorrow", alert.Value.Message);
    }

    #endregion
}
