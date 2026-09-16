using System.Reflection;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Attributes;
using Logistics.Domain.Exceptions;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Behaviours;

/// <summary>
///     Short-circuits a request whose <see cref="RequiresFeatureAttribute" /> names a feature the
///     current tenant does not have, returning a failed result instead of running the handler.
/// </summary>
public sealed class FeatureCheckBehaviour<TRequest, TResponse>(
    IFeatureService featureService,
    ICurrentTenantAccessor tenantAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result, new()
{
    // Evaluated once per closed generic instantiation - avoids per-call reflection.
    private static readonly RequiresFeatureAttribute? Attribute =
        typeof(TRequest).GetCustomAttribute<RequiresFeatureAttribute>();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (Attribute is null)
        {
            return await next(cancellationToken);
        }

        Guid tenantId;
        try
        {
            tenantId = tenantAccessor.GetCurrentTenant().Id;
        }
        catch (InvalidTenantException)
        {
            // No tenant context, so there is no plan to check against - admin operations land here.
            return await next(cancellationToken);
        }

        var check = await featureService.CheckFeatureAsync(tenantId, Attribute.Feature);

        return check.IsSuccess
            ? await next(cancellationToken)
            : new TResponse { Error = check.Error, ErrorCode = check.ErrorCode };
    }
}
