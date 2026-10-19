using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Tenancy;

public interface IUserTenantReader
{
    /// <summary>
    ///     Every tenant holding rows for the user: their own tenant (<see cref="User.TenantId" />) plus each
    ///     tenant they reach through <see cref="UserTenantAccess" /> (customer portal access).
    /// </summary>
    Task<List<Tenant>> ListAsync(User user, CancellationToken ct);
}
