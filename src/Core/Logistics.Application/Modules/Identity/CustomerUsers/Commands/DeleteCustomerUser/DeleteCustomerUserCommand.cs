using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.CustomerUsers.Commands;

/// <summary>
/// Command to delete a customer portal user.
/// </summary>
public class DeleteCustomerUserCommand : ICommand<Result>, IHaveId
{
    /// <summary>
    /// The customer user ID to delete.
    /// </summary>
    public required Guid Id { get; set; }
}
