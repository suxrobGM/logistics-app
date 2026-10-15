using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

public class GetFactoringCompaniesQuery : IQuery<Result<List<FactoringCompanyDto>>>;
