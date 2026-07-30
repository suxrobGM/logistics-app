using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Providers;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>Everything both conversation builders need before they can shape their own prompt.</summary>
internal sealed record LlmSessionContext(
    Tenant Tenant,
    ILlmProvider Provider,
    LlmModelSelection Selection,
    IReadOnlySet<TenantFeature> EnabledFeatures);
