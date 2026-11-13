using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.ProductLicense.Queries;

/// <summary>
/// Full commercial license state for the admin portal License page.
/// </summary>
public sealed class GetProductLicenseStatusQuery : IQuery<Result<ProductLicenseStatusDto>>;
