using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record ResendTenantWelcomeCommand(Guid TenantId) : IAppRequest<Result>;
