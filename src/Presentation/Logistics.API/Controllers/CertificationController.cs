using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("certifications")]
[Produces("application/json")]
public class CertificationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get driver certifications with optional filters
    /// </summary>
    [HttpGet(Name = "GetCertifications")]
    [ProducesResponseType(typeof(PagedResponse<DriverCertificationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Certification.View)]
    public async Task<IActionResult> GetCertifications([FromQuery] GetDriverCertificationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<DriverCertificationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get certifications expiring soon
    /// </summary>
    [HttpGet("expiring", Name = "GetExpiringCertifications")]
    [ProducesResponseType(typeof(List<DriverCertificationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Certification.View)]
    public async Task<IActionResult> GetExpiringCertifications([FromQuery] int daysUntilExpiration = 30)
    {
        var result = await mediator.Send(new GetExpiringCertificationsQuery { DaysUntilExpiration = daysUntilExpiration });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get certifications for a specific employee
    /// </summary>
    [HttpGet("employee/{employeeId:guid}", Name = "GetEmployeeCertifications")]
    [ProducesResponseType(typeof(PagedResponse<DriverCertificationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Certification.View)]
    public async Task<IActionResult> GetEmployeeCertifications(Guid employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetDriverCertificationsQuery { EmployeeId = employeeId, Page = page, PageSize = pageSize };
        var result = await mediator.Send(query);
        return Ok(PagedResponse<DriverCertificationDto>.FromPagedResult(result, page, pageSize));
    }

    /// <summary>
    /// Create a new driver certification
    /// </summary>
    [HttpPost(Name = "CreateCertification")]
    [ProducesResponseType(typeof(DriverCertificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Certification.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateDriverCertificationCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetEmployeeCertifications", new { employeeId = result.Value!.EmployeeId }, result.Value);
    }
}
