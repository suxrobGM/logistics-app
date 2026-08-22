using Logistics.Application.Modules.Integrations.Negotiation;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds the AI rate negotiation demo: lane rate floors, the load board listings the threads hang
/// off, and one thread per status so every state of the Negotiations page has a row.
/// </summary>
internal class NegotiationSeeder(ILogger<NegotiationSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(NegotiationSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 165;
    public override IReadOnlyList<string> DependsOn => [nameof(LoadSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<RateNegotiation>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");
        var tenant = context.CurrentTenant ?? throw new InvalidOperationException("Current tenant not set");
        var currency = region.Currency.ToString();

        // The same value the live resolver falls back to, so a TenantDefault thread shows its floor.
        var baseFloor = tenant.Settings.DefaultRateFloorPerMile
            ?? throw new InvalidOperationException("Tenant has no DefaultRateFloorPerMile");

        var lanes = await SeedLaneFloorsAsync(context, region, currency, baseFloor, cancellationToken);

        var bookedLoadId = await context.TenantUnitOfWork.Repository<Load>().Query()
            .OrderByDescending(l => l.Number)
            .Select(l => (Guid?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var scenarios = Enum.GetValues<NegotiationScenario>();
        foreach (var scenario in scenarios)
        {
            var lane = PickLane(region, lanes, baseFloor, scenario);
            var listing = await CreateListingAsync(context, region, lane, currency, scenario, cancellationToken);
            await BuildThreadAsync(context, tenant.Id, listing, lane, currency, scenario, bookedLoadId, cancellationToken);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(scenarios.Length);
    }

    /// <summary>
    /// Two exact lanes and one destination-wide row, so the rate floor page shows the resolver
    /// falling back from the most specific match down to the tenant default.
    /// </summary>
    private async Task<List<SeededLane>> SeedLaneFloorsAsync(
        SeederContext context,
        IRegionProfile region,
        string currency,
        decimal baseFloor,
        CancellationToken cancellationToken)
    {
        var repo = context.TenantUnitOfWork.Repository<LaneRateFloor>();
        var points = region.RoutePoints;
        var created = new List<SeededLane>();

        var lanes = new (RoutePoint Origin, RoutePoint? Destination, decimal Premium, string Notes)[]
        {
            (points[0], points[7], 0.35m, "Premium lane - the agent holds this floor even on a rebook."),
            (points[6], points[9], 0.15m, "Steady volume, thinner margin."),
            (points[2], null, 0.10m, "Anything leaving this origin.")
        };

        foreach (var (origin, destination, premium, notes) in lanes)
        {
            var originAddress = LaneAddress(origin);
            var destinationAddress = destination is null ? null : LaneAddress(destination);

            var floor = new LaneRateFloor
            {
                OriginCountry = LaneKey.Country(originAddress.Country),
                OriginState = LaneKey.State(originAddress.State),
                DestinationCountry = LaneKey.Country(destinationAddress?.Country ?? originAddress.Country),
                DestinationState = LaneKey.State(destinationAddress?.State),
                MinRatePerMile = decimal.Round(baseFloor + premium, 2),
                MinTotalRate = destinationAddress is null ? null : new Money { Amount = 950m, Currency = currency },
                Notes = notes
            };

            await repo.AddAsync(floor, cancellationToken);
            created.Add(new SeededLane(
                origin, originAddress, destination, destinationAddress, floor.MinRatePerMile));
        }

        return created;
    }

    /// <summary>
    /// The first scenarios reuse a seeded lane, so its own row is the floor <c>LaneRateFloorResolver</c>
    /// would resolve. The rest draw an origin no lane row covers, which lands them on the tenant default.
    /// </summary>
    private Lane PickLane(
        IRegionProfile region,
        List<SeededLane> seededLanes,
        decimal baseFloor,
        NegotiationScenario scenario)
    {
        var seeded = (int)scenario < seededLanes.Count ? seededLanes[(int)scenario] : null;

        RoutePoint origin, destination;
        Address originAddress, destinationAddress;
        decimal floorPerMile;
        RateFloorSource source;

        if (seeded is not null)
        {
            origin = seeded.Origin;
            originAddress = seeded.OriginAddress;
            floorPerMile = seeded.MinRatePerMile;

            // A destination-any row still pins the destination country, so the fill-in has to stay in it.
            destination = seeded.Destination ?? random.Pick(region.RoutePoints
                .Where(p => p != origin && p.Address.Country == originAddress.Country)
                .ToList());
            destinationAddress = seeded.DestinationAddress ?? LaneAddress(destination);
            source = seeded.Destination is null ? RateFloorSource.LaneOriginAny : RateFloorSource.LaneExact;
        }
        else
        {
            // Floor rows key off the state, and two route points can share one - so exclude the
            // state, not the point, or a "tenant default" thread lands on a seeded lane instead.
            var covered = seededLanes.Select(l => l.OriginAddress.State).ToHashSet();
            origin = random.Pick(region.RoutePoints.Where(p => !covered.Contains(p.StateCode)).ToList());
            destination = random.Pick(region.RoutePoints.Where(p => p != origin).ToList());
            originAddress = LaneAddress(origin);
            destinationAddress = LaneAddress(destination);
            floorPerMile = baseFloor;
            source = RateFloorSource.TenantDefault;
        }

        return new Lane(
            originAddress,
            new GeoPoint(origin.Longitude, origin.Latitude),
            destinationAddress,
            new GeoPoint(destination.Longitude, destination.Latitude),
            floorPerMile,
            source,
            random.Next(320, 1150));
    }

    /// <summary>Lane floor columns hold three characters, so they key off the subdivision code.</summary>
    private static Address LaneAddress(RoutePoint point) =>
        point.Address with { State = point.StateCode };

    private async Task<LoadBoardListing> CreateListingAsync(
        SeederContext context,
        IRegionProfile region,
        Lane lane,
        string currency,
        NegotiationScenario scenario,
        CancellationToken cancellationToken)
    {
        // The posted rate sits below the floor - that gap is what the agent negotiates away.
        var postedPerMile = decimal.Round(lane.FloorPerMile * 0.82m, 2);
        var brokers = region.BrokerNames;
        var brokerName = brokers[(int)scenario % brokers.Count];

        var listing = new LoadBoardListing
        {
            ExternalListingId = $"DEMO-{scenario}-{random.Next(1000, 9999)}",
            ProviderType = LoadBoardProviderType.Demo,
            OriginAddress = lane.Origin,
            OriginLocation = lane.OriginLocation,
            DestinationAddress = lane.Destination,
            DestinationLocation = lane.DestinationLocation,
            RatePerMile = postedPerMile,
            TotalRate = new Money { Amount = decimal.Round(postedPerMile * lane.Miles, 0), Currency = currency },
            Distance = lane.Miles,
            Weight = random.Next(18000, 44000),
            Length = 53,
            PickupDateStart = DateTime.UtcNow.AddDays(random.Next(1, 4)),
            PickupDateEnd = DateTime.UtcNow.AddDays(random.Next(4, 7)),
            DeliveryDateStart = DateTime.UtcNow.AddDays(random.Next(7, 9)),
            DeliveryDateEnd = DateTime.UtcNow.AddDays(random.Next(9, 12)),
            EquipmentType = random.Pick(["Dry Van", "Reefer", "Flatbed"]),
            Commodity = random.Pick(["Palletized consumer goods", "Packaged food", "Building materials", "Auto parts"]),
            BrokerName = brokerName,
            BrokerEmail = BrokerEmail(brokerName),
            BrokerPhone = region.GenerateBrokerPhone(),
            BrokerMcNumber = $"MC-{random.Next(100000, 999999)}",
            BrokerCreditScore = scenario == NegotiationScenario.Accepted ? random.Next(82, 96) : random.Next(58, 92),
            BrokerDaysToPay = random.Next(21, 45),
            BrokerCreditCheckedAt = DateTime.UtcNow.AddDays(-random.Next(1, 6)),
            Status = scenario == NegotiationScenario.Accepted ? LoadBoardListingStatus.Booked : LoadBoardListingStatus.Available,
            ExpiresAt = DateTime.UtcNow.AddDays(random.Next(2, 6))
        };

        await context.TenantUnitOfWork.Repository<LoadBoardListing>().AddAsync(listing, cancellationToken);
        return listing;
    }

    private async Task BuildThreadAsync(
        SeederContext context,
        Guid tenantId,
        LoadBoardListing listing,
        Lane lane,
        string currency,
        NegotiationScenario scenario,
        Guid? bookedLoadId,
        CancellationToken cancellationToken)
    {
        var negotiations = context.TenantUnitOfWork.Repository<RateNegotiation>();
        var messages = context.TenantUnitOfWork.Repository<NegotiationMessage>();

        var floorTotal = new Money { Amount = decimal.Round(lane.FloorPerMile * lane.Miles, 0), Currency = currency };
        var snapshot = new RateFloorSnapshot(lane.FloorPerMile, floorTotal, lane.FloorSource);

        var negotiation = RateNegotiation.Create(
            listing.Id,
            listing.BrokerEmail!,
            snapshot,
            listing.BrokerName,
            listing.BrokerMcNumber);

        await negotiations.AddAsync(negotiation, cancellationToken);

        var openedAt = random.UtcDate(DateTime.UtcNow.AddHours(-96), DateTime.UtcNow.AddHours(-6));
        var askPerMile = decimal.Round(lane.FloorPerMile + 0.18m, 2);
        var ask = new Money { Amount = decimal.Round(askPerMile * lane.Miles, 0), Currency = currency };

        Money Total(decimal perMile) =>
            new() { Amount = decimal.Round(perMile * lane.Miles, 0), Currency = currency };

        async Task AddAsync(NegotiationMessage message, double hoursIn)
        {
            message.OccurredAt = openedAt.AddHours(hoursIn);
            await messages.AddAsync(message, cancellationToken);
        }

        async Task AddBrokerReplyAsync(double hoursIn, decimal bumpPerMile)
        {
            var counterPerMile = decimal.Round(listing.RatePerMile!.Value + bumpPerMile, 2);
            var counter = Total(counterPerMile);

            await AddAsync(
                negotiation.AddInboundMessage(
                    $"Best I can do right now is {Format(counter)}. Shipper is firm on the pickup window, " +
                    "let me know quickly if that works for you.",
                    $"Re: {negotiation.Reference}",
                    proposedTotalRate: counter,
                    proposedRatePerMile: counterPerMile),
                hoursIn);
        }

        await AddAsync(
            negotiation.AddOutboundMessage(
                OutboundBody(listing, lane, ask, askPerMile),
                $"{negotiation.Reference} - {listing.OriginAddress.City} to {listing.DestinationAddress.City}",
                ask,
                askPerMile),
            0);

        switch (scenario)
        {
            case NegotiationScenario.AwaitingBroker:
                break;

            case NegotiationScenario.BrokerReplied:
                await AddBrokerReplyAsync(5, 0.06m);
                break;

            case NegotiationScenario.Accepted:
                await AddBrokerReplyAsync(3, 0.08m);

                var settledPerMile = decimal.Round(askPerMile - 0.05m, 2);
                var settled = Total(settledPerMile);

                await AddAsync(
                    negotiation.AddOutboundMessage(
                        $"Deal at {Format(settled)} all-in. Rate confirmation to follow, dispatch will call with the driver details.",
                        $"Re: {negotiation.Reference}",
                        settled,
                        settledPerMile),
                    4);

                await AddAsync(
                    negotiation.AddInboundMessage(
                        $"Accepted at {Format(settled)}. Rate con is attached, please sign and return today.",
                        $"Re: {negotiation.Reference}",
                        proposedTotalRate: settled),
                    6);

                if (bookedLoadId is { } loadId)
                {
                    negotiation.MarkAccepted(loadId);
                    listing.LoadId = loadId;
                    listing.BookedAt = openedAt.AddHours(6);
                }
                else
                {
                    negotiation.Close(RateNegotiationStatus.Accepted, "Broker accepted the counter offer");
                }
                break;

            case NegotiationScenario.Declined:
                await AddAsync(
                    negotiation.AddInboundMessage(
                        $"We cannot go above {Format(listing.TotalRate!)} on this one. Covering it with another carrier.",
                        $"Re: {negotiation.Reference}",
                        proposedTotalRate: listing.TotalRate),
                    7);
                negotiation.Close(RateNegotiationStatus.Declined, "Broker declined - offer stayed below the lane floor");
                break;

            case NegotiationScenario.Expired:
                await AddAsync(
                    negotiation.AddInboundMessage(
                        "Unlimited freight leads, click here to claim your free trial.",
                        "You have new load matches",
                        quarantined: true),
                    11);
                negotiation.ExpiresAt = openedAt.AddHours(48);
                negotiation.Close(RateNegotiationStatus.Expired, "No broker reply within 48 hours");
                break;
        }

        await context.MasterUnitOfWork.Repository<InboundEmailRoute>().AddAsync(
            new InboundEmailRoute
            {
                ThreadToken = negotiation.ReplyToken,
                TenantId = tenantId,
                CreatedAt = openedAt,
                ExpiresAt = negotiation.ExpiresAt,
                RevokedAt = negotiation.IsOpen ? null : negotiation.ClosedAt
            },
            cancellationToken);
    }

    private static string OutboundBody(LoadBoardListing listing, Lane lane, Money ask, decimal askPerMile)
    {
        return
            $"Hello {listing.BrokerName}, we can cover {listing.OriginAddress.City}, {listing.OriginAddress.State} " +
            $"to {listing.DestinationAddress.City}, {listing.DestinationAddress.State} " +
            $"({lane.Miles:N0} miles, {listing.EquipmentType}) at {Format(ask)} all-in, " +
            $"which is {askPerMile:0.00} {ask.Currency} per mile. " +
            "Equipment is available for the posted pickup window. Reply to this address to confirm.";
    }

    private static string Format(Money money) => $"{money.Amount:N0} {money.Currency}";

    private static string BrokerEmail(string brokerName) =>
        $"rates@{new string([.. brokerName.ToLowerInvariant().Where(char.IsLetterOrDigit)])}.example.com";

    private record SeededLane(
        RoutePoint Origin,
        Address OriginAddress,
        RoutePoint? Destination,
        Address? DestinationAddress,
        decimal MinRatePerMile);

    private record Lane(
        Address Origin,
        GeoPoint OriginLocation,
        Address Destination,
        GeoPoint DestinationLocation,
        decimal FloorPerMile,
        RateFloorSource FloorSource,
        int Miles);

    private enum NegotiationScenario
    {
        AwaitingBroker,
        BrokerReplied,
        Accepted,
        Declined,
        Expired
    }
}
