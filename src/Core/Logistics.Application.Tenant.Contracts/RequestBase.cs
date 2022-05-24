namespace Logistics.Application.Contracts;

public abstract class RequestBase<T> : IRequest<T> where T : DataResult
{
}
