using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class BookLoadBoardLoadTool(IMediator mediator, IAgentRunContext runContext) : IAgentTool
{
    public string Name => "book_loadboard_load";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("listing_id") is not { } listingId)
            return ToolResult.Error("Invalid or missing listing_id - use the listing_id returned by search_loadboard");

        if (input.GetGuid("truck_id") is not { } truckId)
            return ToolResult.Error("Invalid or missing truck_id");

        // The booking is attributed to a real dispatcher, so a run with no human origin cannot
        // make one. The model is never asked for this - it has no reliable source for a user id.
        if (runContext.TriggeredByUserId is not { } dispatcherId)
            return ToolResult.Error(
                "Booking requires a dispatcher. This run has no user attached, so the booking must be made by a person.");

        var command = new BookLoadBoardLoadCommand
        {
            ListingId = listingId,
            TruckId = truckId,
            DispatcherId = dispatcherId,
            CustomerName = input.GetString("customer_name"),
            Notes = input.GetString("notes"),
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
                listing_id = listingId,
                truck_id = truckId,
                load_id = booking.CreatedLoadId,
                load_number = booking.CreatedLoadNumber,
                confirmation_id = booking.ExternalConfirmationId
            })
            : ToolResult.Ok(new { success = false, error = booking.ErrorMessage });
    }
}
