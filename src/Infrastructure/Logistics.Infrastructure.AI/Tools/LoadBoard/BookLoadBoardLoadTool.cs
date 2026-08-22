using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class BookLoadBoardLoadTool(IMediator mediator, IAgentRunContext runContext)
    : AgentTool<BookLoadBoardLoadTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The listing_id (GUID) returned by search_loadboard. Not the load board's own external id, which is not stable between searches.")]
        public required Guid ListingId { get; init; }

        [Description("The truck ID (GUID) to assign the booked load to")]
        [AgentEntityId(AgentEntityKind.Truck)]
        public required Guid TruckId { get; init; }

        [Description("Optional customer name, when booking creates a new customer from the broker")]
        public string? CustomerName { get; init; }

        [Description("The rate the broker agreed to in a negotiation thread. Set it whenever a negotiation on this listing reached agreement - leaving it out books at the listing's own rate, which is still refused when it falls below the floor that negotiation opened against.")]
        public decimal? NegotiatedTotalRate { get; init; }

        [Description("Optional notes recorded against the booking")]
        public string? Notes { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "book_loadboard_load",
        "Book a load from a load board. This claims the load and creates it in the system.")
    {
        RequiredFeature = TenantFeature.LoadBoard,
        RequiredPermission = Permission.Dispatch.Manage,
        DecisionType = AgentDecisionType.BookLoadBoardLoad,
        // No MCP: the booking is attributed to a real dispatcher, and an API key names a tenant. The
        // model is never asked for the user id - it has no reliable source for one.
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Dispatch
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (runContext.TriggeredByUserId is not { } dispatcherId)
            return ToolResult.Error(
                "Booking requires a dispatcher. This run has no user attached, so the booking must be made by a person.");

        var command = new BookLoadBoardLoadCommand
        {
            ListingId = input.ListingId,
            TruckId = input.TruckId,
            DispatcherId = dispatcherId,
            CustomerName = input.CustomerName,
            NegotiatedTotalRate = input.NegotiatedTotalRate,
            Notes = input.Notes,
            // Deliberately not exposed to the agent: overriding a failed broker credit check is a
            // dispatcher's judgement call, and the prompt tells the agent never to book below the
            // tenant minimum.
            OverrideCreditCheck = false
        };

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.WriteFailed(result);

        var booking = result.Value;
        return booking.Success
            ? ToolResult.Ok(new
            {
                success = true,
                listing_id = input.ListingId,
                truck_id = input.TruckId,
                load_id = booking.CreatedLoadId,
                load_number = booking.CreatedLoadNumber,
                confirmation_id = booking.ExternalConfirmationId
            })
            : ToolResult.Ok(new { success = false, error = booking.ErrorMessage });
    }
}
