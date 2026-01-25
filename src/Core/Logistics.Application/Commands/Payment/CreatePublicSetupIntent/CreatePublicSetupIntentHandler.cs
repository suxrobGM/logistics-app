using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreatePublicSetupIntentHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<CreatePublicSetupIntentHandler> logger)
    : IAppRequestHandler<CreatePublicSetupIntentCommand, Result<SetupIntentDto>>
{
    public async Task<Result<SetupIntentDto>> Handle(CreatePublicSetupIntentCommand req, CancellationToken ct)
    {
        // Get the tenant
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result<SetupIntentDto>.Fail("Invalid payment link.");
        }

        // Check if tenant has Stripe Connect enabled
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<SetupIntentDto>.Fail("This company is not set up to receive payments.");
        }

        if (!tenant.ChargesEnabled)
        {
            return Result<SetupIntentDto>.Fail("This company cannot currently accept payments. Please contact them directly.");
        }

        // Switch to tenant database
        tenantUow.SetCurrentTenant(tenant);

        // Find the payment link by token
        var paymentLink = await tenantUow.Repository<PaymentLink>()
            .GetAsync(p => p.Token == req.Token, ct);

        if (paymentLink is null || !paymentLink.IsValid)
        {
            return Result<SetupIntentDto>.Fail("This payment link has expired or been revoked.");
        }

        try
        {
            // Create SetupIntent on the connected account
            var setupIntent = await stripeConnectService.CreateConnectedSetupIntentAsync(
                tenant.StripeConnectedAccountId);

            logger.LogInformation(
                "Created public SetupIntent {SetupIntentId} for tenant {TenantId}",
                setupIntent.Id, tenant.Id);

            return Result<SetupIntentDto>.Ok(new SetupIntentDto
            {
                ClientSecret = setupIntent.ClientSecret
            });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create public SetupIntent for tenant {TenantId}", tenant.Id);
            return Result<SetupIntentDto>.Fail($"Failed to initialize payment: {ex.Message}");
        }
    }
}
