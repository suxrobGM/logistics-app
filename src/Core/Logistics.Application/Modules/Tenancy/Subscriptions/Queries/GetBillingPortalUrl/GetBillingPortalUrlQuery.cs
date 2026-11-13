using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

public class GetBillingPortalUrlQuery : IQuery<Result<BillingPortalUrlDto>>
{
    public required string ReturnUrl { get; set; }
}
