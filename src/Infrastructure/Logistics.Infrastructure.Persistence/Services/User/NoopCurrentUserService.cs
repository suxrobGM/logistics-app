using Logistics.Application.Services;

namespace Logistics.Infrastructure.Persistence.Services;

/// <summary>
///     A no-op implementation of ICurrentUserService for non-HTTP scenarios (e.g., DbMigrator, background jobs).
/// </summary>
public sealed class NoopCurrentUserService : ICurrentUserService
{
    public Guid? GetUserId()
    {
        return null;
    }
}
