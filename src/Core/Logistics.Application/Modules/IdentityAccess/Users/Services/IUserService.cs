using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.IdentityAccess.Users.Services;

public interface IUserService : IApplicationService
{
    /// <summary>
    ///     Applies the non-empty fields to the master user and, when one changed, saves and syncs the tenant
    ///     profiles. Returns false when the user doesn't exist.
    /// </summary>
    Task<bool> UpdateUserAsync(UpdateUserParams userParams, CancellationToken ct = default);

    /// <summary>
    ///     Copies the master user's email, name and phone onto their <see cref="Employee" /> and email onto their
    ///     <see cref="CustomerUser" /> rows in every tenant they belong to. Call it after saving a change to any of
    ///     those fields - the master user is the source of truth. Runs in its own scope, so the caller's tenant
    ///     context and tracked changes are left alone.
    /// </summary>
    Task SyncTenantProfilesAsync(User user, CancellationToken ct = default);
}

public record UpdateUserParams(Guid Id)
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
