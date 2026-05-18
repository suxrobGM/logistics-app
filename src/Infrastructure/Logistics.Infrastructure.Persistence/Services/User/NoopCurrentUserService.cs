using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Infrastructure.Persistence.Services;

/// <summary>
///     A no-op implementation of ICurrentUserService for non-HTTP scenarios (e.g., DbMigrator, background jobs).
/// </summary>
public sealed class NoopCurrentUserService : ICurrentUserService
{
    public string? IpAddress => null;

    public string? UserAgent => null;

    public Guid? GetUserId()
    {
        return null;
    }

    public string GetUserName()
    {
        return "System";
    }
}
