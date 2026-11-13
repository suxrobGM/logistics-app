using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Logistics.Application.Modules.Financial.Expenses.Commands;
using Logistics.Application.Modules.Financial.Expenses.Queries;

namespace Logistics.API.Controllers.Financial;

[ApiController]
[Route("expenses")]
[Produces("application/json")]
public class ExpenseController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    #region Queries

    [HttpGet("{id:guid}", Name = "GetExpenseById")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Expense.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetExpenseByIdQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/receipt", Name = "DownloadExpenseReceipt")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Expense.View)]
    public async Task<IActionResult> DownloadReceipt(Guid id)
    {
        var requestedById = UserId;

        var result = await mediator.Send(new DownloadExpenseReceiptQuery
        {
            ExpenseId = id,
            RequestedById = requestedById
        });

        return result.ToFileResult(x => File(x.FileContent, x.ContentType, x.OriginalFileName));
    }

    [HttpGet(Name = "GetExpenses")]
    [ProducesResponseType(typeof(PagedResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Expense.View)]
    public async Task<IActionResult> GetList([FromQuery] GetExpensesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<ExpenseDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("stats", Name = "GetExpenseStats")]
    [ProducesResponseType(typeof(ExpenseStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.View)]
    public async Task<IActionResult> GetStats([FromQuery] GetExpenseStatsQuery query)
    {
        var result = await mediator.Send(query);
        return result.ToActionResult();
    }

    #endregion

    #region Receipt Upload

    [HttpPost("receipt", Name = "UploadExpenseReceipt")]
    [ProducesResponseType(typeof(UploadReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB limit
    public async Task<IActionResult> UploadReceipt([FromForm] UploadReceiptRequest request)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(new ErrorResponse("No file provided"));
        }

        var result = await mediator.Send(new UploadExpenseReceiptCommand
        {
            FileContent = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length
        });

        return result.IsSuccess
            ? Ok(new UploadReceiptResponse(result.Value!))
            : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Create Commands

    [HttpPost("company", Name = "CreateCompanyExpense")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Manage)]
    public async Task<IActionResult> CreateCompanyExpense([FromBody] CreateCompanyExpenseCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToCreatedResult("GetExpenseById", id => new { id });
    }

    [HttpPost("truck", Name = "CreateTruckExpense")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Manage)]
    public async Task<IActionResult> CreateTruckExpense([FromBody] CreateTruckExpenseCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToCreatedResult("GetExpenseById", id => new { id });
    }

    [HttpPost("bodyshop", Name = "CreateBodyShopExpense")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Manage)]
    public async Task<IActionResult> CreateBodyShopExpense([FromBody] CreateBodyShopExpenseCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToCreatedResult("GetExpenseById", id => new { id });
    }

    #endregion

    #region Update/Delete Commands

    [HttpPut("{id:guid}", Name = "UpdateExpense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteExpense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Expense.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteExpenseCommand { Id = id });
        return result.ToActionResult();
    }

    #endregion

    #region Approval Commands

    [HttpPost("{id:guid}/approve", Name = "ApproveExpense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Approve)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await mediator.Send(new ApproveExpenseCommand { Id = id, ApproverId = UserId });
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/reject", Name = "RejectExpense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Expense.Approve)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequest request)
    {
        var result = await mediator.Send(new RejectExpenseCommand
        {
            Id = id,
            ApproverId = UserId,
            Reason = request.Reason
        });
        return result.ToActionResult();
    }

    #endregion
}

public record RejectExpenseRequest(string Reason);
public record UploadReceiptRequest(IFormFile File);
public record UploadReceiptResponse(string BlobPath);
