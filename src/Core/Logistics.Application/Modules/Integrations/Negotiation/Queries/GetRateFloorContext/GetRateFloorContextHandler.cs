using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

internal sealed class GetRateFloorContextHandler(
    ITenantUnitOfWork tenantUow,
    ILaneRateFloorResolver resolver)
    : IAppRequestHandler<GetRateFloorContextQuery, Result<RateFloorContextDto>>
{
    public async Task<Result<RateFloorContextDto>> Handle(GetRateFloorContextQuery req, CancellationToken ct)
    {
        var listing = await tenantUow.Repository<LoadBoardListing>().GetByIdAsync(req.ListingId, ct);
        if (listing is null)
        {
            return Result<RateFloorContextDto>.Fail($"Could not find a load board listing with ID '{req.ListingId}'");
        }

        var floor = await resolver.ResolveAsync(listing, ct);
        var negotiation = await tenantUow.Repository<RateNegotiation>()
            .GetAsync(RateNegotiation.OpenForListing(listing.Id), ct);

        return Result<RateFloorContextDto>.Ok(new RateFloorContextDto
        {
            ListingId = listing.Id,
            Floor = floor,
            BrokerEmailAvailable = !string.IsNullOrWhiteSpace(listing.BrokerEmail),
            ActiveNegotiationId = negotiation?.Id,
            RoundCount = negotiation?.RoundCount ?? 0,
            MaxRounds = RateNegotiation.MaxRounds,
            ListingTotalRate = listing.TotalRate?.Amount,
            ListingRatePerMile = listing.RatePerMile,
            DistanceMiles = listing.Distance,
            Currency = listing.TotalRate?.Currency ?? ComposeNegotiationEmailRequest.DefaultCurrency
        });
    }
}
