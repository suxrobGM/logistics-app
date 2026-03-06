using Logistics.Application.Abstractions;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class CreateTenantHandler(
    ITenantDatabaseService tenantDatabase,
    IMasterUnitOfWork masterUow,
    IFeatureService featureService,
    UserManager<User> userManager,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions)
    : IAppRequestHandler<CreateTenantCommand, Result>
{
    public async Task<Result> Handle(CreateTenantCommand req, CancellationToken ct)
    {
        var tenantName = req.Name.Trim().ToLower();
        var tenant = new Tenant
        {
            Name = tenantName,
            CompanyName = req.CompanyName,
            DotNumber = req.DotNumber,
            CompanyAddress = req.CompanyAddress,
            BillingEmail = req.BillingEmail!,
            ConnectionString = tenantDatabase.GenerateConnectionString(tenantName)
        };

        var existingTenant = await masterUow.Repository<Tenant>().GetAsync(i => i.Name == tenant.Name, ct);

        if (existingTenant is not null)
        {
            return Result.Fail($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await tenantDatabase.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return Result.Fail("Could not create the tenant's database");
        }

        await masterUow.Repository<Tenant>().AddAsync(tenant, ct);
        await masterUow.SaveChangesAsync(ct);

        // Initialize feature configurations for the new tenant based on defaults
        await featureService.InitializeFeaturesForTenantAsync(tenant.Id);

        // Create owner account and send welcome email
        var ownerResult = await CreateOwnerAccountAsync(req, tenant);
        if (!ownerResult.IsSuccess)
        {
            return ownerResult;
        }

        return Result.Ok();
    }

    private async Task<Result> CreateOwnerAccountAsync(CreateTenantCommand req, Tenant tenant)
    {
        var existingUser = await userManager.FindByEmailAsync(req.OwnerEmail);

        if (existingUser is not null)
        {
            if (existingUser.TenantId is not null && existingUser.TenantId != tenant.Id)
            {
                return Result.Fail($"A user with email '{req.OwnerEmail}' already belongs to another tenant");
            }

            // Assign existing unassigned user to this tenant
            existingUser.TenantId = tenant.Id;
            existingUser.FirstName = req.OwnerFirstName;
            existingUser.LastName = req.OwnerLastName;
            await userManager.UpdateAsync(existingUser);
            await SendWelcomeEmailAsync(existingUser, tenant);
            return Result.Ok();
        }

        var temporaryPassword = TokenGenerator.GenerateSecureToken(12);
        var owner = new User
        {
            UserName = req.OwnerEmail,
            Email = req.OwnerEmail,
            FirstName = req.OwnerFirstName,
            LastName = req.OwnerLastName,
            EmailConfirmed = true,
            TenantId = tenant.Id
        };

        var createResult = await userManager.CreateAsync(owner, temporaryPassword);
        if (!createResult.Succeeded)
        {
            return Result.Fail(
                $"Failed to create owner account: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        await SendWelcomeEmailAsync(owner, tenant, temporaryPassword);
        return Result.Ok();
    }

    private async Task SendWelcomeEmailAsync(User owner, Tenant tenant, string? temporaryPassword = null)
    {
        var loginUrl = $"{identityServerOptions.Value.Authority}/Account/Login";
        var companyName = tenant.CompanyName ?? tenant.Name;

        var model = new TenantWelcomeEmailModel
        {
            OwnerName = $"{owner.FirstName} {owner.LastName}",
            CompanyName = companyName,
            Email = owner.Email!,
            TemporaryPassword = temporaryPassword,
            LoginUrl = loginUrl
        };

        var subject = $"Welcome to LogisticsX — {companyName}";
        var body = await emailTemplateService.RenderAsync("TenantWelcome", model);

        await emailSender.SendEmailAsync(owner.Email!, subject, body);
    }
}
