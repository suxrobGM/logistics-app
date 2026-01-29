using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetEmergencyAlertByIdQuery(Guid Id) : IAppRequest<Result<EmergencyAlertDto>>;
