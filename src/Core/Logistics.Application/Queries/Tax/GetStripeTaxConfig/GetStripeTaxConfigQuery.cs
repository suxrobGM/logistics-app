using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetStripeTaxConfigQuery : IAppRequest<Result<StripeTaxConfigDto>>
{
    /// <summary>When true, also include the (large) list of all Stripe tax codes.</summary>
    public bool IncludeTaxCodes { get; init; }
}
