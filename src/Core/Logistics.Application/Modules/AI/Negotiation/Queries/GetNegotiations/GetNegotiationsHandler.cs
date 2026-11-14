using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.AI.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

internal sealed class GetNegotiationsHandler(ITenantUnitOfWork tenantUow, INegotiationDtoMapper dtoMapper)
    : IRequestHandler<GetNegotiationsQuery, PagedResult<RateNegotiationDto>>
{
    private static readonly SortFields<RateNegotiation> Sorts = new SortFields<RateNegotiation>("-createdAt")
        .Add("createdAt", x => x.CreatedAt);

    public async Task<PagedResult<RateNegotiationDto>> Handle(GetNegotiationsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<RateNegotiation>().Query();

        if (req.Status.HasValue)
        {
            query = query.Where(n => n.Status == req.Status.Value);
        }
        else if (req.ActiveOnly)
        {
            query = query.Where(RateNegotiation.Open());
        }

        return await query.ToPagedResultAsync(req, Sorts, MapPageAsync, ct);
    }

    private async Task<IEnumerable<RateNegotiationDto>> MapPageAsync(
        IReadOnlyList<RateNegotiation> negotiations,
        CancellationToken ct) =>
        await dtoMapper.MapAsync(negotiations, ct);
}
