using System.Reflection;
using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Application.Services;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Behaviours;

/// <summary>
///     MediatR pipeline behavior that checks if a required feature is enabled for the current tenant.
///     If the feature is disabled, returns a failed result instead of executing the handler.
/// </summary>
public sealed class FeatureCheckBehaviour<TRequest, TResponse>(
    IFeatureService featureService,
    ITenantService tenantService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAppRequest<TResponse>
    where TResponse : IResult, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var attribute = typeof(TRequest).GetCustomAttribute<RequiresFeatureAttribute>();

        // If no RequiresFeature attribute, proceed normally
        if (attribute is null)
        {
            return await next(cancellationToken);
        }

        // Get current tenant - may throw if not in tenant context
        Guid tenantId;
        try
        {
            var tenant = tenantService.GetCurrentTenant();
            tenantId = tenant.Id;
        }
        catch (InvalidTenantException)
        {
            // If we can't determine tenant, skip the check (e.g., admin operations)
            return await next(cancellationToken);
        }

        // Check if feature is enabled
        var isEnabled = await featureService.IsFeatureEnabledAsync(tenantId, attribute.Feature);

        if (!isEnabled)
        {
            // Return a failed result with a descriptive error
            var featureName = attribute.Feature.GetDescription();
            var response = new TResponse();

            // Use reflection to set the Error property since we can't directly set it on IResult
            var errorProperty = typeof(TResponse).GetProperty(nameof(Result.Error));
            if (errorProperty?.CanWrite == true)
            {
                errorProperty.SetValue(response, $"The '{featureName}' feature is not enabled for this tenant.");
            }

            return response;
        }

        return await next(cancellationToken);
    }
}
