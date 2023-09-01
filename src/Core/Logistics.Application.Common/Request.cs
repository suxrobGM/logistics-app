using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Common;

public abstract class Request<T> : IRequest<T> where T : IResponseResult
{
}
