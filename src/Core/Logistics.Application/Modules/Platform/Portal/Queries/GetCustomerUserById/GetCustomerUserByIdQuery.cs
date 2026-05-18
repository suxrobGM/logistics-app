using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get a CustomerUser by ID.
/// </summary>
public class GetCustomerUserByIdQuery : IQuery<Result<CustomerUserDto>>
{
    /// <summary>
    /// The customer user ID.
    /// </summary>
    public required Guid Id { get; set; }
}
