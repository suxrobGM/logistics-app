using Logistics.Application.Modules.Compliance.Dvir.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Policies;
using Logistics.Application.Abstractions.CurrentUser;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance;

/// <summary>
/// The other half of the Driver role's <c>Dvir.Manage</c> grant: filing your own is fine, signing
/// as a colleague is not. <c>Dvir.Review</c> is what buys acting on someone else's report.
/// </summary>
public class DvirReportOwnershipTests
{
    private static readonly Guid DriverId = Guid.NewGuid();
    private static readonly Guid OtherDriverId = Guid.NewGuid();
    private static readonly Guid SupervisorId = Guid.NewGuid();
    private static readonly Guid TruckId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();

    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<DvirReport, Guid> reportRepo =
        Substitute.For<ITenantRepository<DvirReport, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();

    public DvirReportOwnershipTests()
    {
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        tenantUow.Repository<DvirReport>().Returns(reportRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);

        // Only the supervisor's role carries Dvir.Review; both roles carry Dvir.Manage.
        var driverRole = Role("tenant.driver", Permission.Dvir.Manage);
        var supervisorRole = Role("tenant.manager", Permission.Dvir.Manage, Permission.Dvir.Review);

        employeeRepo.Query().Returns(new List<Employee>
        {
            Employee(DriverId, driverRole),
            Employee(OtherDriverId, driverRole),
            Employee(SupervisorId, supervisorRole)
        }.BuildMock());

        truckRepo.GetByIdAsync(TruckId, Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Truck>());
        employeeRepo.GetByIdAsync(DriverId, Arg.Any<CancellationToken>())
            .Returns(Employee(DriverId, driverRole));
        reportRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(DraftReport());
    }

    private static TenantRole Role(string name, params string[] permissions)
    {
        var role = new TenantRole(name) { Id = Guid.NewGuid() };
        foreach (var permission in permissions)
        {
            role.Claims.Add(new TenantRoleClaim(CustomClaimTypes.Permission, permission)
            {
                RoleId = role.Id
            });
        }

        return role;
    }

    private static Employee Employee(Guid id, TenantRole role) => new()
    {
        Id = id,
        Email = $"{id}@test.com",
        FirstName = "Test",
        LastName = "Employee",
        RoleId = role.Id,
        Role = role
    };

    private static DvirReport DraftReport() => new()
    {
        DriverId = DriverId,
        TruckId = TruckId,
        Status = DvirStatus.Draft,
        DriverSignature = "signed",
        Type = DvirType.PreTrip
    };

    private CreateDvirReportHandler CreateSut() => new(tenantUow, currentUser);

    private SubmitDvirReportHandler SubmitSut() => new(tenantUow, currentUser);

    private static CreateDvirReportCommand CreateCommand() => new()
    {
        TruckId = TruckId,
        DriverId = DriverId,
        Type = DvirType.PreTrip
    };

    [Fact]
    public async Task Create_AnotherDriverWithoutReview_IsRejected()
    {
        currentUser.GetUserId().Returns(OtherDriverId);

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("You can only file a DVIR for yourself.", result.Error);
        await reportRepo.DidNotReceive().AddAsync(Arg.Any<DvirReport>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_OwnDvir_IsAllowed()
    {
        currentUser.GetUserId().Returns(DriverId);

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await reportRepo.Received(1).AddAsync(Arg.Any<DvirReport>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_SupervisorWithReview_IsAllowed()
    {
        currentUser.GetUserId().Returns(SupervisorId);

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await reportRepo.Received(1).AddAsync(Arg.Any<DvirReport>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Unauthenticated_IsRejected()
    {
        currentUser.GetUserId().Returns((Guid?)null);

        var result = await CreateSut().Handle(CreateCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Submit_AnotherDriversReport_IsRejected()
    {
        currentUser.GetUserId().Returns(OtherDriverId);

        var result = await SubmitSut().Handle(
            new SubmitDvirReportCommand { ReportId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("You can only submit your own DVIR.", result.Error);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_OwnReport_IsAllowed()
    {
        currentUser.GetUserId().Returns(DriverId);

        var result = await SubmitSut().Handle(
            new SubmitDvirReportCommand { ReportId = Guid.NewGuid() }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Submit_SupervisorWithReview_IsAllowed()
    {
        currentUser.GetUserId().Returns(SupervisorId);

        var result = await SubmitSut().Handle(
            new SubmitDvirReportCommand { ReportId = Guid.NewGuid() }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
