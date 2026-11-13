using Logistics.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        /// <summary>204, or 404 for <see cref="ErrorCodes.NotFound" />, else 400.</summary>
        public IActionResult ToActionResult()
        {
            return result.IsSuccess ? new NoContentResult() : CreateErrorResult(result);
        }
    }

    extension<T>(Result<T> result)
    {
        /// <summary>200 with the value, or 404 for <see cref="ErrorCodes.NotFound" />, else 400.</summary>
        public IActionResult ToActionResult()
        {
            return result.IsSuccess ? new OkObjectResult(result.Value) : CreateErrorResult(result);
        }

        /// <summary>201 at the named route with the value, or the same error mapping as <c>ToActionResult</c>.</summary>
        public IActionResult ToCreatedResult(string routeName, Func<T, object> routeValues)
        {
            return result.IsSuccess
                ? new CreatedAtRouteResult(routeName, routeValues(result.Value!), result.Value)
                : CreateErrorResult(result);
        }

        /// <summary>The file built from the value, or the same error mapping as <c>ToActionResult</c>.</summary>
        public IActionResult ToFileResult(Func<T, FileResult> file)
        {
            return result.IsSuccess ? file(result.Value!) : CreateErrorResult(result);
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
