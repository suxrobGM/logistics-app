using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetAccidentReportByIdQuery(Guid Id) : IAppRequest<Result<AccidentReportDto>>;
