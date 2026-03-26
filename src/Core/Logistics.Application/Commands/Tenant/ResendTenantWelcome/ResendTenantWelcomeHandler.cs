using Logistics.Application.Abstractions;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class ResendTenantWelcomeHandler(
    IMasterUnitOfWork masterUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions)
    : IAppRequestHandler<ResendTenantWelcomeCommand, Result>
{
    public async Task<Result> Handle(ResendTenantWelcomeCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);

        if (tenant is null)
        {
            return Result.Fail("Tenant not found");
        }

        var owner = await masterUow.Repository<User>().GetAsync(u => u.TenantId == req.TenantId, ct);

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

        var subject = $"Welcome to LogisticsX — {companyName}";
        var body = await emailTemplateService.RenderAsync("TenantWelcome", model);

        await emailSender.SendEmailAsync(owner.Email!, subject, body);

        return Result.Ok();
    }
}
