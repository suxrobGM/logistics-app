using Logistics.Domain.Exceptions;
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
        catch (TenantAccessDeniedException)
        {
            // An authorization failure must NOT be softened into an empty/failed Result (some
            // controllers return those as 200). Let it propagate to the exception middleware,
            // which maps it to 403 - so a cross-tenant access attempt is denied, not silently empty.
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);
            return new TResponse { Error = ex.Message };
        }
    }
}
