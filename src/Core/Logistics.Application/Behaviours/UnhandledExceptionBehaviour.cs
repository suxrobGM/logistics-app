using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Behaviours;

public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            // Rethrow rather than folding the message into a failed Result: controllers surface a
            // Result error verbatim, so ex.Message would reach the client with filesystem paths,
            // SQL, or connection detail in it. The middleware turns this into a sanitized 500, and
            // it is also what maps TenantAccessDeniedException to a 403 instead of an empty 200.
            logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}",
                typeof(TRequest).Name, request);
            throw;
        }
    }
}
