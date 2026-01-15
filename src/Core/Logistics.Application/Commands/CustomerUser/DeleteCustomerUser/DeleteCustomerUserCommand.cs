using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to delete a customer portal user.
/// </summary>
public class DeleteCustomerUserCommand : IAppRequest<Result>
{
    /// <summary>
    /// The customer user ID to delete.
    /// </summary>
    public required Guid Id { get; set; }
}
