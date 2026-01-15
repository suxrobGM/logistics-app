using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to create a new customer portal user.
/// </summary>
public class CreateCustomerUserCommand : IAppRequest<Result<CustomerUserDto>>
{
    /// <summary>
    /// The user ID (from master database) to link.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The customer ID this user should be associated with.
    /// </summary>
    public required Guid CustomerId { get; set; }

    /// <summary>
    /// Email address for the portal user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Optional display name.
    /// </summary>
    public string? DisplayName { get; set; }
}
