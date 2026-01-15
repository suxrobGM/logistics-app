using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get a CustomerUser by ID.
/// </summary>
public class GetCustomerUserByIdQuery : IAppRequest<Result<CustomerUserDto>>
{
    /// <summary>
    /// The customer user ID.
    /// </summary>
    public required Guid Id { get; set; }
}
