using MediatR;

namespace Logistics.Application.Shared.Abstractions;

public abstract class RequestBase<T> : IRequest<T> where T : IDataResult
{
}
