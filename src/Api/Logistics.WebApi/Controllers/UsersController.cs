using Microsoft.AspNetCore.Mvc;
using MediatR;
using Logistics.Application.Contracts.Commands;

namespace Logistics.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand request)
    {
        var result = await mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
