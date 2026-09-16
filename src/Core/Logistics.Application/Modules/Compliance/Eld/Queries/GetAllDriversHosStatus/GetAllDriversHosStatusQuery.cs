using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetAllDriversHosStatusQuery : IQuery<Result<List<DriverHosStatusDto>>>;
