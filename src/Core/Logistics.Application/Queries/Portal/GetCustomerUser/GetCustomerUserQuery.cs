using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get the CustomerUser info for an authenticated user.
/// </summary>
public class GetCustomerUserQuery : IAppRequest<Result<CustomerUserDto>>
{
    /// <summary>
    /// The user ID (from authentication token).
    /// </summary>
    public required Guid UserId { get; set; }
}
