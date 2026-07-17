using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Queries;

[RequiresFeature(TenantFeature.LoadBoard)]
public class GetBrokerCreditQuery : IQuery<Result<BrokerCreditDto>>
{
    public required string McNumber { get; set; }

    /// <summary>
    /// Optional listing to stamp with the fetched credit data.
    /// </summary>
    public Guid? ListingId { get; set; }
}
