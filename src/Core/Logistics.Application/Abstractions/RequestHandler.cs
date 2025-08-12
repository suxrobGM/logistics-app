using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Abstractions;

public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, new()
{
    public abstract Task<TResponse> Handle(TRequest req, CancellationToken ct);
}
