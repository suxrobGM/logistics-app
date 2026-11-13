using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.ProductLicense.Queries;

/// <summary>
/// The public discovery document behind /.well-known/logisticsx.json.
/// </summary>
public sealed class GetProductLicenseDiscoveryQuery : IQuery<Result<ProductLicenseDiscoveryDto>>;
