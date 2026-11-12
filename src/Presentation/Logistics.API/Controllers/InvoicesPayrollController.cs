using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.Payroll.Commands;
using Logistics.Application.Modules.Financial.Payroll.Queries;

namespace Logistics.API.Controllers;

/// <summary>
///     Payroll endpoints under /invoices/payrolls. The "Invoices" tag and a name that sorts right after
///     <see cref="InvoicesController" /> keep the OpenAPI document, and so the generated client, unchanged.
/// </summary>
[ApiController]
[Route("invoices")]
[Tags("Invoices")]
[Produces("application/json")]
public class InvoicesPayrollController(IMediator mediator) : ControllerBase
{
    [HttpGet("payrolls/preview", Name = "PreviewPayrollInvoice")]
    [ProducesResponseType(typeof(PayrollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> PreviewPayrollInvoice([FromQuery] PreviewPayrollInvoiceQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("payrolls/previews", Name = "PreviewPayrollInvoices")]
    [ProducesResponseType(typeof(InvoiceDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> PreviewPayrollInvoices([FromBody] PreviewPayrollInvoicesQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("payrolls", Name = "CreatePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> CreatePayrollInvoice([FromBody] CreatePayrollInvoiceCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Batch create payroll invoices for multiple employees.
    /// </summary>
    [HttpPost("payrolls/batch", Name = "BatchCreatePayrollInvoices")]
    [ProducesResponseType(typeof(BatchCreatePayrollInvoicesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> BatchCreatePayrollInvoices([FromBody] BatchCreatePayrollInvoicesRequest request)
    {
        var result = await mediator.Send(new BatchCreatePayrollInvoicesCommand
        {
            EmployeeIds = request.EmployeeIds,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd
        });
        return result.ToActionResult();
    }

    [HttpPut("payrolls/{id:guid}", Name = "UpdatePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> UpdatePayrollInvoice(Guid id, [FromBody] UpdatePayrollInvoiceCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Submit a payroll invoice for approval.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/submit", Name = "SubmitPayrollForApproval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> SubmitPayrollForApproval(Guid id)
    {
        var result = await mediator.Send(new SubmitPayrollForApprovalCommand { Id = id });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Approve a payroll invoice.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/approve", Name = "ApprovePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> ApprovePayrollInvoice(Guid id, [FromBody] ApprovePayrollRequest? request = null)
    {
        var result = await mediator.Send(new ApprovePayrollInvoiceCommand
        {
            Id = id,
            Notes = request?.Notes
        });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Reject a payroll invoice.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/reject", Name = "RejectPayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> RejectPayrollInvoice(Guid id, [FromBody] RejectPayrollRequest request)
    {
        var result = await mediator.Send(new RejectPayrollInvoiceCommand
        {
            Id = id,
            Reason = request.Reason
        });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Batch approve multiple payroll invoices.
    /// </summary>
    [HttpPost("payrolls/batch-approve", Name = "BatchApprovePayroll")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> BatchApprovePayroll([FromBody] BatchApprovePayrollRequest request)
    {
        var result = await mediator.Send(new BatchApprovePayrollCommand
        {
            Ids = request.Ids,
            Notes = request.Notes
        });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Pay a payroll invoice via Stripe Connect transfer to the employee's bank account.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/pay", Name = "PayPayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> PayPayrollInvoice(Guid id)
    {
        var result = await mediator.Send(new PayPayrollInvoiceCommand { InvoiceId = id });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Download a payroll invoice as PDF pay stub.
    /// </summary>
    [HttpGet("payrolls/{id:guid}/pdf", Name = "DownloadPayrollPayStubPdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> DownloadPayrollPayStubPdf(Guid id)
    {
        var result = await mediator.Send(new GetPayrollPayStubPdfQuery { PayrollInvoiceId = id });

        if (!result.IsSuccess)
        {
            return NotFound(ErrorResponse.FromResult(result));
        }

        return File(result.Value!.PdfBytes, "application/pdf", result.Value.FileName);
    }
}
