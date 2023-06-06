using MediatR;

namespace Logistics.Application.Common;

public abstract class RequestBase<T> : IRequest<T> where T : IResponseResult
{
}
