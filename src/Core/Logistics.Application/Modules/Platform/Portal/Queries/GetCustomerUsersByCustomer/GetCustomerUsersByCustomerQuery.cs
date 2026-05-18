using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get all CustomerUsers for a specific customer.
/// </summary>
public class GetCustomerUsersByCustomerQuery : IQuery<Result<IEnumerable<CustomerUserDto>>>
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    public required Guid CustomerId { get; set; }
}
