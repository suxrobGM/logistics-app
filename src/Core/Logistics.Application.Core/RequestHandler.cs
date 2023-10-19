using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Core;

public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResponseResult, new()
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return HandleValidated(request, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(new TResponse { Error = ex.Message });
        }
    }

    protected abstract Task<TResponse> HandleValidated(TRequest req, CancellationToken cancellationToken);
}
