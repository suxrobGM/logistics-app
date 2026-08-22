using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class NegotiationMapper
{
    /// <summary>
    /// Lane and listing rate come from the caller: reading them off the listing navigation would
    /// lazy-load one listing per row. Required rather than optional so a caller cannot quietly
    /// broadcast a row with the lane blanked out.
    /// </summary>
    public static RateNegotiationDto ToDto(this RateNegotiation entity, LoadBoardListing? listing)
    {
        var dto = Map(entity);
        return dto with
        {
            MaxRounds = RateNegotiation.MaxRounds,
            OriginCity = listing?.OriginAddress.City,
            OriginState = listing?.OriginAddress.State,
            DestinationCity = listing?.DestinationAddress.City,
            DestinationState = listing?.DestinationAddress.State,
            ListingTotalRate = listing?.TotalRate
        };
    }

    [MapperIgnoreSource(nameof(RateNegotiation.DomainEvents))]
    [MapperIgnoreSource(nameof(RateNegotiation.CreatedBy))]
    [MapperIgnoreSource(nameof(RateNegotiation.UpdatedAt))]
    [MapperIgnoreSource(nameof(RateNegotiation.UpdatedBy))]
    [MapperIgnoreSource(nameof(RateNegotiation.ReplyToken))]
    [MapperIgnoreSource(nameof(RateNegotiation.LoadBoardListing))]
    [MapperIgnoreSource(nameof(RateNegotiation.Load))]
    [MapperIgnoreSource(nameof(RateNegotiation.Messages))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.Messages))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.MaxRounds))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.OriginCity))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.OriginState))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.DestinationCity))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.DestinationState))]
    [MapperIgnoreTarget(nameof(RateNegotiationDto.ListingTotalRate))]
    private static partial RateNegotiationDto Map(RateNegotiation entity);

    [MapperIgnoreSource(nameof(NegotiationMessage.DomainEvents))]
    [MapperIgnoreSource(nameof(NegotiationMessage.Negotiation))]
    [MapperIgnoreSource(nameof(NegotiationMessage.NegotiationId))]
    [MapperIgnoreSource(nameof(NegotiationMessage.RawBody))]
    [MapperIgnoreSource(nameof(NegotiationMessage.ProviderMessageId))]
    [MapperIgnoreSource(nameof(NegotiationMessage.InReplyToMessageId))]
    private static partial NegotiationMessageDto Map(NegotiationMessage entity);

    /// <summary>
    /// Projects in SQL rather than materializing entities: RawBody holds up to 64KB per row and no
    /// consumer of the DTO reads it.
    /// </summary>
    public static partial IQueryable<NegotiationMessageDto> ProjectToDto(
        this IQueryable<NegotiationMessage> query);
}
