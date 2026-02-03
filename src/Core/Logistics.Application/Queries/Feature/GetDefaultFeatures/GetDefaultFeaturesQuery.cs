using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get the default feature configuration for new tenants.
/// </summary>
public class GetDefaultFeaturesQuery : IAppRequest<Result<IReadOnlyList<DefaultFeatureStatusDto>>>;
