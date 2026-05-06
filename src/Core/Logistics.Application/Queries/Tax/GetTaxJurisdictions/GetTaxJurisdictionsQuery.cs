using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetTaxJurisdictionsQuery : IAppRequest<Result<IReadOnlyList<TaxJurisdictionInfoDto>>>;
