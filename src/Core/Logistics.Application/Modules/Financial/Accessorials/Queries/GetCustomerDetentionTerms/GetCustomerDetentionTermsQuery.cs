using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

/// <summary>A customer's own detention terms, as stored (null = uses the tenant's).</summary>
public class GetCustomerDetentionTermsQuery : IQuery<Result<DetentionTermsDto>>
{
    public Guid CustomerId { get; set; }
}
