using MediatR;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Behaviours;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest,TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        _logger.LogInformation("Handling request {ReqType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {ReqType}", typeof(TRequest).Name);
        return response;
    }
}