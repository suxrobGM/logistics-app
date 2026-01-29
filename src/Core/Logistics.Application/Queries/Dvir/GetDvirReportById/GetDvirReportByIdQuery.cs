using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetDvirReportByIdQuery(Guid Id) : IAppRequest<Result<DvirReportDto>>;
