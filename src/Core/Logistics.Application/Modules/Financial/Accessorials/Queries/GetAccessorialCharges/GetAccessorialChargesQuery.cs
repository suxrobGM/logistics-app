using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

/// <summary>The whole catalog, by name. Tenants keep a handful, so it is not paged.</summary>
public class GetAccessorialChargesQuery : IQuery<Result<List<AccessorialChargeDto>>>;
