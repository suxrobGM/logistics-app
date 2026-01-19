using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("loadboard")]
[Produces("application/json")]
public class LoadBoardController(IMediator mediator) : ControllerBase
{
    #region Provider Configuration

    [HttpGet("providers", Name = "GetLoadBoardProviders")]
    [ProducesResponseType(typeof(List<LoadBoardConfigurationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.LoadBoard.View)]
    public async Task<IActionResult> GetProviders()
    {
        var result = await mediator.Send(new GetLoadBoardConfigurationsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("providers", Name = "CreateLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateLoadBoardConfigurationCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("providers/{providerId:guid}", Name = "DeleteLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> DeleteProvider(Guid providerId)
    {
        var result = await mediator.Send(new DeleteLoadBoardConfigurationCommand { Id = providerId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Load Search & Booking

    [HttpPost("search", Name = "SearchLoadBoard")]
    [ProducesResponseType(typeof(LoadBoardSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Search)]
    public async Task<IActionResult> SearchLoads([FromBody] SearchLoadBoardCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("listings/{listingId:guid}/book", Name = "BookLoadBoardListing")]
    [ProducesResponseType(typeof(LoadBoardBookingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Book)]
    public async Task<IActionResult> BookListing(Guid listingId, [FromBody] LoadBoardBookingRequest request)
    {
        var result = await mediator.Send(new BookLoadBoardLoadCommand
        {
            ListingId = listingId,
            TruckId = request.TruckId,
            DispatcherId = request.DispatcherId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            Notes = request.Notes
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Posted Trucks

    [HttpGet("trucks", Name = "GetPostedTrucks")]
    [ProducesResponseType(typeof(List<PostedTruckDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.LoadBoard.View)]
    public async Task<IActionResult> GetPostedTrucks([FromQuery] GetPostedTrucksQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("trucks", Name = "PostTruckToLoadBoard")]
    [ProducesResponseType(typeof(PostTruckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Post)]
    public async Task<IActionResult> PostTruck([FromBody] PostTruckToLoadBoardCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("trucks/{postedTruckId:guid}", Name = "RemovePostedTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Post)]
    public async Task<IActionResult> RemovePostedTruck(Guid postedTruckId)
    {
        var result = await mediator.Send(new RemovePostedTruckCommand { PostedTruckId = postedTruckId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
