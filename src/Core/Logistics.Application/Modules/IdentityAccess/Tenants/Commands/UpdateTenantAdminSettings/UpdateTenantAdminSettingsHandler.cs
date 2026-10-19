using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantAdminSettingsHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<UpdateTenantAdminSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantAdminSettingsCommand req, CancellationToken ct)
    {
        var tenantRepository = masterUow.Repository<Tenant>();
        var tenant = await tenantRepository.GetByIdAsync(req.TenantId, ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }

        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            var name = req.Name.Trim().ToLower();

            if (name != tenant.Name)
            {
                if (await tenantRepository.CountAsync(t => t.Name == name && t.Id != tenant.Id, ct) > 0)
                {
                    return Result.Fail($"Tenant name '{name}' is already taken, please choose another name");
                }

                tenant.Name = name;
            }
        }

        if (req.AIEnabled is { } aiEnabled)
        {
            tenant.Settings = tenant.Settings with { AIEnabled = aiEnabled };
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
