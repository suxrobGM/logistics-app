using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Common;

public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponseResult, new()
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(request, out var error))
            {
                return Task.FromResult(new TResponse { Error = error });
            }

            return HandleValidated(request, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(new TResponse { Error = ex.Message });
        }
    }

    protected abstract Task<TResponse> HandleValidated(TRequest request, CancellationToken cancellationToken);

    protected virtual bool Validate(TRequest request, out string errorDescription)
    {
        errorDescription = string.Empty;
        return true;
    }
}
