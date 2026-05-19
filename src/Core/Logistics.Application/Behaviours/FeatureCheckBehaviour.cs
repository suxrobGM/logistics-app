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

        // Check if feature is enabled
        var isEnabled = await featureService.IsFeatureEnabledAsync(tenantId, attribute.Feature);

        if (!isEnabled)
        {
            var featureName = attribute.Feature.GetDescription();

            // Determine if it's admin-locked or not in plan
            var allStatuses = await featureService.GetAllFeatureStatusAsync(tenantId);
            var featureStatus = allStatuses.FirstOrDefault(s => s.Feature == attribute.Feature);

            string errorCode;
            string errorMessage;

            if (featureStatus is { IsAdminLocked: true })
            {
                errorCode = ErrorCodes.FeatureDisabledByAdmin;
                errorMessage = $"The '{featureName}' feature has been disabled by your platform administrator.";
            }
            else
            {
                errorCode = ErrorCodes.FeatureNotInPlan;
                errorMessage = $"The '{featureName}' feature is not included in your current subscription plan. Please upgrade to access this feature.";
            }

            var response = new TResponse();
            var errorProperty = typeof(TResponse).GetProperty(nameof(Result.Error));
            if (errorProperty?.CanWrite == true)
            {
                errorProperty.SetValue(response, errorMessage);
            }

            var errorCodeProperty = typeof(TResponse).GetProperty(nameof(Result.ErrorCode));
            if (errorCodeProperty?.CanWrite == true)
            {
                errorCodeProperty.SetValue(response, errorCode);
            }

            return response;
        }

        return await next(cancellationToken);
    }
}
