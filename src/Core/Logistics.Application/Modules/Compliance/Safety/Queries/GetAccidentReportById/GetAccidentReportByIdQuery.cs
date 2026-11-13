using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Queries;

public record GetAccidentReportByIdQuery(Guid Id) : IQuery<Result<AccidentReportDto>>, IHaveId;
