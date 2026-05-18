using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Features.Queries;

/// <summary>
/// Query to get the default feature configuration for new tenants.
/// </summary>
public class GetDefaultFeaturesQuery : IQuery<Result<IReadOnlyList<DefaultFeatureStatusDto>>>;
