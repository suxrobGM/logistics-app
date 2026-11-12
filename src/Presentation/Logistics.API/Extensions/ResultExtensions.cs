using Logistics.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        /// <summary>
        ///     204 on success. A failure answers 404 when it carries <see cref="ErrorCodes.NotFound" />,
        ///     otherwise 400.
        /// </summary>
        public IActionResult ToActionResult()
        {
            return result.IsSuccess ? new NoContentResult() : CreateErrorResult(result);
        }
    }

    extension<T>(Result<T> result)
    {
        /// <summary>
        ///     200 with the value on success. A failure answers 404 when it carries
        ///     <see cref="ErrorCodes.NotFound" />, otherwise 400.
        /// </summary>
        public IActionResult ToActionResult()
        {
            return result.IsSuccess ? new OkObjectResult(result.Value) : CreateErrorResult(result);
        }
    }

    private static IActionResult CreateErrorResult(Result result)
    {
        var error = ErrorResponse.FromResult(result);
        if (result.ErrorCode == ErrorCodes.NotFound)
        {
            return new NotFoundObjectResult(error);
        }

        return new BadRequestObjectResult(error);
    }
}
