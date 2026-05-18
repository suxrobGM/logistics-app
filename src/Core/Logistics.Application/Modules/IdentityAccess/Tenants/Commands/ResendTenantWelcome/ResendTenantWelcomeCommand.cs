using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

public record ResendTenantWelcomeCommand(Guid TenantId) : ICommand<Result>;
