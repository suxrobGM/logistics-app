using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

public class GetBillingPortalUrlQuery : IQuery<Result<BillingPortalUrlDto>>
{
    public required string ReturnUrl { get; set; }
}
