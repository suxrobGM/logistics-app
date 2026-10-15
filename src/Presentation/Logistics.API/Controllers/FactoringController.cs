using Logistics.Application.Modules.Financial.Factoring.Commands;
using Logistics.Application.Modules.Financial.Factoring.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     Factoring companies, and the schedules of accounts invoices are submitted to them on.
/// </summary>
[ApiController]
[Route("factoring")]
[Produces("application/json")]
public class FactoringController(IMediator mediator) : ControllerBase
{
    #region Factoring companies

    [HttpGet("companies", Name = "GetFactoringCompanies")]
    [ProducesResponseType(typeof(List<FactoringCompanyDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetCompanies()
    {
        var result = await mediator.Send(new GetFactoringCompaniesQuery());
        return Ok(result.Value);
    }

    [HttpPost("companies", Name = "CreateFactoringCompany")]
    [ProducesResponseType(typeof(FactoringCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> CreateCompany([FromBody] CreateFactoringCompanyCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("companies/{id:guid}", Name = "UpdateFactoringCompany")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateFactoringCompanyCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("companies/{id:guid}", Name = "DeleteFactoringCompany")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var result = await mediator.Send(new DeleteFactoringCompanyCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Schedules of accounts

    [HttpGet("batches", Name = "GetFactoringBatches")]
    [ProducesResponseType(typeof(PagedResponse<FactoringBatchDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetBatches([FromQuery] GetFactoringBatchesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<FactoringBatchDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("batches/{id:guid}", Name = "GetFactoringBatchById")]
    [ProducesResponseType(typeof(FactoringBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetBatchById(Guid id)
    {
        var result = await mediator.Send(new GetFactoringBatchByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Download a schedule of accounts as PDF.
    /// </summary>
    [HttpGet("batches/{id:guid}/pdf", Name = "DownloadFactoringSchedulePdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> DownloadSchedulePdf(Guid id)
    {
        var result = await mediator.Send(new GetFactoringSchedulePdfQuery { BatchId = id });
        return result.IsSuccess
            ? File(result.Value!.PdfBytes, "application/pdf", result.Value.FileName)
            : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Put invoices on a schedule of accounts and email it, with the invoice PDFs and load paperwork,
    ///     to the factoring company.
    /// </summary>
    [HttpPost("batches", Name = "SubmitInvoicesToFactor")]
    [ProducesResponseType(typeof(FactoringBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Submit([FromBody] SubmitInvoicesToFactorCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Record which invoices on a schedule the factor funded (with advance and fee) or rejected.
    /// </summary>
    [HttpPost("batches/{id:guid}/funding", Name = "RecordFactoringFunding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> RecordFunding(Guid id, [FromBody] RecordFactoringFundingCommand request)
    {
        request.BatchId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("batches/{id:guid}/items/{itemId:guid}/reserverelease", Name = "ReleaseFactoringReserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> ReleaseReserve(Guid id, Guid itemId, [FromBody] ReleaseFactoringReserveCommand request)
    {
        request.BatchId = id;
        request.ItemId = itemId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("batches/{id:guid}/items/{itemId:guid}/chargeback", Name = "ChargeBackFactoredInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> ChargeBack(Guid id, Guid itemId, [FromBody] ChargeBackFactoredInvoiceCommand request)
    {
        request.BatchId = id;
        request.ItemId = itemId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
