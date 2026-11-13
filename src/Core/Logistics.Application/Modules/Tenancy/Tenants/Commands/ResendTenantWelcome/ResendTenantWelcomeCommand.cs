using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

public record ResendTenantWelcomeCommand(Guid TenantId) : ICommand<Result>;
