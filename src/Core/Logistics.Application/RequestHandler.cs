using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application;

public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, new()
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return HandleValidated(request, cancellationToken);
    }

    protected abstract Task<TResponse> HandleValidated(TRequest req, CancellationToken ct);
}
