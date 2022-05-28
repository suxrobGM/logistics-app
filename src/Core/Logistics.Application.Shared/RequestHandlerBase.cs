using MediatR;
using Logistics.Domain.Shared.Exceptions;

namespace Logistics.Application.Shared;

public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : DataResult, new()
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(request, out var error))
            {
                return Task.FromResult<TResponse>(new() { Error = error });
            }

            return HandleValidated(request, cancellationToken);
        }
        catch (InvalidTenantException ex)
        {
            return Task.FromResult<TResponse>(new() { Error = ex.Message });
        }
    }

    protected abstract Task<TResponse> HandleValidated(TRequest request, CancellationToken cancellationToken);
    protected abstract bool Validate(TRequest request, out string errorDescription);
}
