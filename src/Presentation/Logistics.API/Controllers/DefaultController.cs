using Logistics.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("")]
public class DefaultController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(Result<string>.Succeed("Logistics API is running"));
    }
}
