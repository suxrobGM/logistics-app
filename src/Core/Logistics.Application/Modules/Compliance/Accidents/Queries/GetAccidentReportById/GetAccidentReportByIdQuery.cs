using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Accidents.Queries;

public record GetAccidentReportByIdQuery(Guid Id) : IQuery<Result<AccidentReportDto>>;
