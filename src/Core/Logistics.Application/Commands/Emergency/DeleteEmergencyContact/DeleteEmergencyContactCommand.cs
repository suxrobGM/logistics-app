using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record DeleteEmergencyContactCommand(Guid Id) : IAppRequest<Result>;
