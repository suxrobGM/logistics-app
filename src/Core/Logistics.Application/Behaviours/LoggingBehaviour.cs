using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Behaviours;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAppRequest<TResponse> where TResponse : IResult, new()
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

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
        var response = await next(ct);
        _logger.LogInformation("Handled {ReqType}", typeof(TRequest).Name);
        return response;
    }
}
