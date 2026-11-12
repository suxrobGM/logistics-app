using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Application.Modules.Integrations.LoadBoard.Queries;

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
        return result.ToActionResult();
    }

    [HttpPost("providers", Name = "CreateLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateLoadBoardConfigurationCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("providers/{providerId:guid}", Name = "UpdateLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> UpdateProvider(Guid providerId, [FromBody] UpdateLoadBoardConfigurationCommand request)
    {
        request.Id = providerId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("providers/{providerId:guid}/test", Name = "TestLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> TestProvider(Guid providerId)
    {
        var result = await mediator.Send(new TestLoadBoardConnectionCommand { Id = providerId });
        return result.ToActionResult();
    }

    [HttpDelete("providers/{providerId:guid}", Name = "DeleteLoadBoardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Manage)]
    public async Task<IActionResult> DeleteProvider(Guid providerId)
    {
        var result = await mediator.Send(new DeleteLoadBoardConfigurationCommand { Id = providerId });
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    [HttpGet("brokers/{mcNumber}/credit", Name = "GetBrokerCredit")]
    [ProducesResponseType(typeof(BrokerCreditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.View)]
    public async Task<IActionResult> GetBrokerCredit(string mcNumber, [FromQuery] Guid? listingId)
    {
        var result = await mediator.Send(new GetBrokerCreditQuery { McNumber = mcNumber, ListingId = listingId });
        return result.ToActionResult();
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
            Notes = request.Notes,
            OverrideCreditCheck = request.OverrideCreditCheck
        });
        return result.ToActionResult();
    }

    #endregion

    #region Posted Trucks

    [HttpGet("trucks", Name = "GetPostedTrucks")]
    [ProducesResponseType(typeof(List<PostedTruckDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.LoadBoard.View)]
    public async Task<IActionResult> GetPostedTrucks([FromQuery] GetPostedTrucksQuery query)
    {
        var result = await mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpPost("trucks", Name = "PostTruckToLoadBoard")]
    [ProducesResponseType(typeof(PostTruckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Post)]
    public async Task<IActionResult> PostTruck([FromBody] PostTruckToLoadBoardCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("trucks/{postedTruckId:guid}", Name = "RemovePostedTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.LoadBoard.Post)]
    public async Task<IActionResult> RemovePostedTruck(Guid postedTruckId)
    {
        var result = await mediator.Send(new RemovePostedTruckCommand { PostedTruckId = postedTruckId });
        return result.ToActionResult();
    }

    #endregion
}
