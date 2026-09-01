using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Infrastructure.Persistence.Services;

/// <summary>
///     A no-op implementation of ICurrentUserService for non-HTTP scenarios (e.g., DbMigrator, background jobs).
/// </summary>
public sealed class NoopCurrentUserService : ICurrentUserService
{
    public string? IpAddress => null;

    public string? UserAgent => null;

    public Guid? GetUserId() => null;

    public string GetUserName() => "System";

    public Guid? GetTenantId() => null;

    public bool IsInRole(params string[] roles) => false;
}
