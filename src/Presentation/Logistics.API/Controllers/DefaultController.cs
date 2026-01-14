using Logistics.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("")]
[Produces("application/json")]
public class DefaultController : ControllerBase
{
    [HttpGet(Name = "GetApiStatus")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(Result<string>.Ok("Logistics API is running"));
    }
}
