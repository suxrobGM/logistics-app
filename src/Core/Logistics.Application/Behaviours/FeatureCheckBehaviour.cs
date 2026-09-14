using System.Reflection;
using Logistics.Application.Attributes;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Tenancy;

namespace Logistics.Application.Behaviours;

/// <summary>
///     MediatR pipeline behavior that checks if a required feature is enabled for the current tenant.
///     If the feature is disabled, returns a failed result instead of executing the handler.
/// </summary>
public sealed class FeatureCheckBehaviour<TRequest, TResponse>(
    IFeatureService featureService,
    ICurrentTenantAccessor tenantAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, new()
{
    // Evaluated once per closed generic instantiation - avoids per-call reflection.
    private static readonly RequiresFeatureAttribute? Attribute =
        typeof(TRequest).GetCustomAttribute<RequiresFeatureAttribute>();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var attribute = Attribute;

        // If no RequiresFeature attribute, proceed normally
        if (attribute is null)
        {
            return await next(cancellationToken);
        }

        // Get current tenant - may throw if not in tenant context
        Guid tenantId;
        try
        {
            var tenant = tenantAccessor.GetCurrentTenant();
            tenantId = tenant.Id;
        }
        catch (InvalidTenantException)
        {
            // If we can't determine tenant, skip the check (e.g., admin operations)
            return await next(cancellationToken);
        }

        var check = await featureService.CheckFeatureAsync(tenantId, attribute.Feature);

        if (!check.IsSuccess)
        {
            var response = new TResponse();
            var errorProperty = typeof(TResponse).GetProperty(nameof(Result.Error));
            if (errorProperty?.CanWrite == true)
            {
                errorProperty.SetValue(response, check.Error);
            }

            var errorCodeProperty = typeof(TResponse).GetProperty(nameof(Result.ErrorCode));
            if (errorCodeProperty?.CanWrite == true)
            {
                errorCodeProperty.SetValue(response, check.ErrorCode);
            }

            return response;
        }

        return await next(cancellationToken);
    }
}
