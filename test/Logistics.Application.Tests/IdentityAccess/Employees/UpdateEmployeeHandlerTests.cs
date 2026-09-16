using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Employees.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Employees;

public class UpdateEmployeeHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<TenantRole, Guid> _roleRepo =
        Substitute.For<ITenantRepository<TenantRole, Guid>>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    public UpdateEmployeeHandlerTests()
    {
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.Repository<TenantRole>().Returns(_roleRepo);
    }

    [Fact]
    public async Task Handle_CustomRoleWithinCallerPermissions_AssignsRole()
    {
        var permission = new TenantRoleClaim("permission", "employee.manage");
        var callerRole = Role("manager", permission);
        var newRole = Role("custom", new TenantRoleClaim(permission.ClaimType, permission.ClaimValue));
        var caller = Employee(callerRole);
        var target = Employee(Role("driver"));
        Arrange(caller, target, newRole);

        var result = await Handler().Handle(
            new UpdateEmployeeCommand { UserId = target.Id, Role = newRole.Name },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(newRole, target.Role);
    }

    [Fact]
    public async Task Handle_RoleBeyondCallerPermissions_IsRejected()
    {
        var caller = Employee(Role("manager", new TenantRoleClaim("permission", "employee.manage")));
        var targetRole = Role("driver");
        var target = Employee(targetRole);
        var newRole = Role("custom", new TenantRoleClaim("permission", "tenant.manage"));
        Arrange(caller, target, newRole);

        var result = await Handler().Handle(
            new UpdateEmployeeCommand { UserId = target.Id, Role = newRole.Name },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Same(targetRole, target.Role);
    }

    [Fact]
    public async Task Handle_PlatformAdminWithoutTenantEmployee_AssignsRole()
    {
        var target = Employee(Role("driver"));
        var newRole = Role("owner", new TenantRoleClaim("permission", "tenant.manage"));
        _employeeRepo.GetByIdAsync(target.Id, Arg.Any<CancellationToken>()).Returns(target);
        _roleRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<TenantRole, bool>>>(),
            Arg.Any<CancellationToken>()).Returns(newRole);
        _currentUser.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);

        var result = await Handler().Handle(
            new UpdateEmployeeCommand { UserId = target.Id, Role = newRole.Name },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(newRole, target.Role);
    }

    private void Arrange(Employee caller, Employee target, TenantRole newRole)
    {
        _currentUser.GetUserId().Returns(caller.Id);
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<Guid>(0) == target.Id ? target : caller);
        _roleRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<TenantRole, bool>>>(),
            Arg.Any<CancellationToken>()).Returns(newRole);
    }

    private UpdateEmployeeHandler Handler() => new(_tenantUow, _currentUser);

    private static Employee Employee(TenantRole role) => new()
    {
        Id = Guid.NewGuid(),
        Email = "employee@test.com",
        FirstName = "Test",
        LastName = "Employee",
        Role = role
    };

    private static TenantRole Role(string name, params TenantRoleClaim[] claims)
    {
        var role = new TenantRole(name);
        foreach (var claim in claims)
        {
            role.Claims.Add(claim);
        }

        return role;
    }
}
