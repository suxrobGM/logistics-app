using Logistics.Infrastructure.AI.Tools.LoadBoard;
using Logistics.Infrastructure.AI.Agents;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.LoadBoard;

public class LoadBoardToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly AgentRunContext runContext = new();

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    private static Address SomeAddress => new()
    {
        Line1 = "1 Depot", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US"
    };

    private static LoadBoardListingDto Listing(Guid id) => new()
    {
        Id = id,
        ExternalListingId = "DAT-99",
        ProviderType = LoadBoardProviderType.Dat,
        OriginAddress = SomeAddress,
        OriginLocation = new GeoPoint(-96.8, 32.78),
        DestinationAddress = SomeAddress,
        DestinationLocation = new GeoPoint(-95.36, 29.76),
        RatePerMile = 2.75m,
        TotalRate = 1800m,
        Currency = "USD",
        BrokerName = "Acme Brokerage",
        BrokerMcNumber = "MC123456",
        BrokerCreditScore = 92,
        ExpiresAt = DateTime.UtcNow.AddHours(6)
    };

    #region search_loadboard

    [Fact]
    public async Task Search_MissingOrigin_ReturnsErrorWithoutQuerying()
    {
        var result = await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject { ["origin_city"] = "Dallas" }, CancellationToken.None);

        Assert.Equal(
            "Missing required input: origin_state",
            Parse(result).GetProperty("error").GetString());
        await mediator.DidNotReceive().Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_BuildsCommandFromCityStateAndRadius()
    {
        mediator.Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardSearchResultDto>.Ok(new LoadBoardSearchResultDto()));

        await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["origin_city"] = "Dallas",
                ["origin_state"] = "TX",
                ["radius_miles"] = 250,
                ["destination_state"] = "AZ"
            },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<SearchLoadBoardCommand>(c =>
                c.OriginAddress!.City == "Dallas" &&
                c.OriginAddress.State == "TX" &&
                c.OriginRadius == 250 &&
                c.DestinationAddress!.State == "AZ"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_NoRadiusGiven_DefaultsTo100Miles()
    {
        mediator.Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardSearchResultDto>.Ok(new LoadBoardSearchResultDto()));

        await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject { ["origin_city"] = "Dallas", ["origin_state"] = "TX" },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<SearchLoadBoardCommand>(c => c.OriginRadius == 100 && c.DestinationAddress == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_ReturnsPersistedListingIdSoBookingCanReferenceIt()
    {
        var listingId = Guid.NewGuid();
        mediator.Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardSearchResultDto>.Ok(new LoadBoardSearchResultDto
            {
                Listings = [Listing(listingId)],
                TotalCount = 1
            }));

        var root = Parse(await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject { ["origin_city"] = "Dallas", ["origin_state"] = "TX" },
            CancellationToken.None));

        var listing = root.GetProperty("listings")[0];
        Assert.Equal(listingId, listing.GetProperty("listing_id").GetGuid());
        Assert.Equal("Dat", listing.GetProperty("provider").GetString());
        Assert.Equal(2.75m, listing.GetProperty("rate_per_mile").GetDecimal());
        Assert.Equal("MC123456", listing.GetProperty("broker_mc_number").GetString());
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.False(root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task Search_ProviderFailed_SurfacesItRatherThanReportingNoFreight()
    {
        mediator.Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardSearchResultDto>.Ok(new LoadBoardSearchResultDto
            {
                Listings = [],
                Errors = new Dictionary<LoadBoardProviderType, string?>
                {
                    [LoadBoardProviderType.Dat] = "credentials rejected"
                }
            }));

        var root = Parse(await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject { ["origin_city"] = "Dallas", ["origin_state"] = "TX" },
            CancellationToken.None));

        var error = root.GetProperty("provider_errors")[0];
        Assert.Equal("Dat", error.GetProperty("provider").GetString());
        Assert.Equal("credentials rejected", error.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Search_FeatureGateRejects_SurfacesTheError()
    {
        mediator.Send(Arg.Any<SearchLoadBoardCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardSearchResultDto>.Fail("Load board is not enabled for this plan"));

        var root = Parse(await new SearchLoadBoardTool(mediator).ExecuteAsync(
            new JsonObject { ["origin_city"] = "Dallas", ["origin_state"] = "TX" },
            CancellationToken.None));

        Assert.Equal("Load board is not enabled for this plan", root.GetProperty("error").GetString());
    }

    #endregion

    #region book_loadboard_load

    private BookLoadBoardLoadTool BookingTool() => new(mediator, runContext);

    [Fact]
    public async Task Book_NoDispatcherOnTheRun_RefusesRatherThanGuessing()
    {
        // A scheduled dispatch pass has no human origin. Booking real freight must be attributable.
        var result = await BookingTool().ExecuteAsync(
            new JsonObject
            {
                ["listing_id"] = Guid.NewGuid().ToString(),
                ["truck_id"] = Guid.NewGuid().ToString()
            },
            CancellationToken.None);

        Assert.Contains("dispatcher", Parse(result).GetProperty("error").GetString()!);
        await mediator.DidNotReceive().Send(Arg.Any<BookLoadBoardLoadCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Book_MissingListingId_NamesTheMissingProperty()
    {
        runContext.SetTriggeredBy(Guid.NewGuid());

        var result = await BookingTool().ExecuteAsync(
            new JsonObject { ["truck_id"] = Guid.NewGuid().ToString() }, CancellationToken.None);

        Assert.Contains("listing_id", Parse(result).GetProperty("error").GetString()!);
    }

    [Fact]
    public async Task Book_Success_EmitsCreatedLoadAndConfirmation()
    {
        var dispatcherId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();
        runContext.SetTriggeredBy(dispatcherId);

        mediator.Send(Arg.Any<BookLoadBoardLoadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardBookingResultDto>.Ok(new LoadBoardBookingResultDto
            {
                Success = true,
                CreatedLoadId = loadId,
                CreatedLoadNumber = 1042,
                ExternalConfirmationId = "CONF-7"
            }));

        var root = Parse(await BookingTool().ExecuteAsync(
            new JsonObject
            {
                ["listing_id"] = listingId.ToString(),
                ["truck_id"] = truckId.ToString(),
                ["customer_name"] = "Acme Brokerage"
            },
            CancellationToken.None));

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(loadId, root.GetProperty("load_id").GetGuid());
        Assert.Equal(1042, root.GetProperty("load_number").GetInt64());
        Assert.Equal("CONF-7", root.GetProperty("confirmation_id").GetString());

        await mediator.Received(1).Send(
            Arg.Is<BookLoadBoardLoadCommand>(c =>
                c.ListingId == listingId &&
                c.TruckId == truckId &&
                c.DispatcherId == dispatcherId &&
                c.CustomerName == "Acme Brokerage" &&
                !c.OverrideCreditCheck),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Book_NeverOverridesTheBrokerCreditCheck()
    {
        runContext.SetTriggeredBy(Guid.NewGuid());
        mediator.Send(Arg.Any<BookLoadBoardLoadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardBookingResultDto>.Ok(new LoadBoardBookingResultDto { Success = true }));

        // Even if the model invents the argument, overriding is a dispatcher's judgement call.
        await BookingTool().ExecuteAsync(
            new JsonObject
            {
                ["listing_id"] = Guid.NewGuid().ToString(),
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["override_credit_check"] = true
            },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<BookLoadBoardLoadCommand>(c => !c.OverrideCreditCheck), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Book_CommandFails_EmitsSuccessFalseAndError()
    {
        runContext.SetTriggeredBy(Guid.NewGuid());
        mediator.Send(Arg.Any<BookLoadBoardLoadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardBookingResultDto>.Fail("Broker credit below the tenant minimum"));

        var root = Parse(await BookingTool().ExecuteAsync(
            new JsonObject
            {
                ["listing_id"] = Guid.NewGuid().ToString(),
                ["truck_id"] = Guid.NewGuid().ToString()
            },
            CancellationToken.None));

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("Broker credit below the tenant minimum", root.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Book_ProviderRejectedTheBooking_ReportsFailureNotSuccess()
    {
        runContext.SetTriggeredBy(Guid.NewGuid());
        mediator.Send(Arg.Any<BookLoadBoardLoadCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadBoardBookingResultDto>.Ok(new LoadBoardBookingResultDto
            {
                Success = false,
                ErrorMessage = "Listing already claimed"
            }));

        var root = Parse(await BookingTool().ExecuteAsync(
            new JsonObject
            {
                ["listing_id"] = Guid.NewGuid().ToString(),
                ["truck_id"] = Guid.NewGuid().ToString()
            },
            CancellationToken.None));

        // A command that succeeded carrying a failed booking must not read as a booked load.
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("Listing already claimed", root.GetProperty("error").GetString());
    }

    #endregion

}
