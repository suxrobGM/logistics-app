using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Email;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class ResendTenantWelcomeHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions)
    : IRequestHandler<ResendTenantWelcomeCommand, Result>
{
    public async Task<Result> Handle(ResendTenantWelcomeCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);

        if (tenant is null)
        {
            return Result.Fail("Tenant not found");
        }

        tenantUow.SetCurrentTenant(tenant);
        var ownerId = await tenantUow.Repository<Employee>().Query()
            .Where(e => e.Role != null && e.Role.Name == TenantRoles.Owner)
            .OrderBy(e => e.JoinedDate)
            .Select(e => (Guid?)e.Id)
            .FirstOrDefaultAsync(ct);

        var owner = ownerId is null ? null : await masterUow.Repository<User>().GetByIdAsync(ownerId.Value, ct);

        if (owner is null)
        {
            return Result.Fail("No owner found for this tenant");
        }

        var loginUrl = $"{identityServerOptions.Value.UserFacingAuthority}/Account/Login";
        var companyName = tenant.CompanyName ?? tenant.Name;

        var model = new TenantWelcomeEmailModel
        {
            OwnerName = $"{owner.FirstName} {owner.LastName}",
            CompanyName = companyName,
            Email = owner.Email!,
            LoginUrl = loginUrl
        };

        var subject = $"Welcome to LogisticsX - {companyName}";
        var body = await emailTemplateService.RenderAsync("TenantWelcome", model);

        await emailSender.SendEmailAsync(owner.Email!, subject, body);

        return Result.Ok();
    }
}
