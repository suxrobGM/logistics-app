using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get the CustomerUser info for an authenticated user.
/// </summary>
public class GetCustomerUserQuery : IQuery<Result<CustomerUserDto>>
{
    /// <summary>
    /// The user ID (from authentication token).
    /// </summary>
    public required Guid UserId { get; set; }
}
