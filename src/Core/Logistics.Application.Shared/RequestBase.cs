using MediatR;

namespace Logistics.Application.Shared;

public abstract class RequestBase<T> : IRequest<T> where T : DataResult
{
}
