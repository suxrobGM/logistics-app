using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;


namespace Logistics.Application.Commands;

internal sealed class CreatePaymentMethodHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<CreatePaymentMethodHandler> logger)
    : IAppRequestHandler<CreatePaymentMethodCommand, Result>
{
    public async Task<Result> Handle(
        CreatePaymentMethodCommand req, CancellationToken ct)
    {
        try
        {
            // If there are no payment methods for the tenant, set the first one as default
            var paymentMethodsCount = await tenantUow.Repository<PaymentMethod>().CountAsync(ct: ct);

            switch (req.Type)
            {
                case PaymentMethodType.Card:
                    await CreateCardPaymentMethod(req, paymentMethodsCount == 0);
                    break;
                case PaymentMethodType.UsBankAccount:
                    await CreateUsBankAccountPaymentMethod(req, paymentMethodsCount == 0);
                    break;
                case PaymentMethodType.InternationalBankAccount:
                    await CreateInternationalBankAccountPaymentMethod(req, paymentMethodsCount == 0);
                    break;
                default:
                    return Result.Fail($"Unsupported payment method type: {req.Type}");
            }

            await tenantUow.SaveChangesAsync(ct);
            return Result.Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error creating payment method: {Message}", ex.Message);
            return Result.Fail($"Failed to sync payment method with Stripe: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Validation error creating payment method: {Message}", ex.Message);
            return Result.Fail(ex.Message);
        }
    }

    private async Task CreateCardPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // Verify and attach Stripe payment method if provided
        if (!string.IsNullOrEmpty(command.StripePaymentMethodId))
        {
            var stripePaymentMethod = await stripePaymentService.GetPaymentMethodAsync(command.StripePaymentMethodId);
            if (stripePaymentMethod is null)
            {
                throw new ArgumentException($"Payment method {command.StripePaymentMethodId} not found in Stripe");
            }

            // Attach to customer if not already attached
            if (stripePaymentMethod.CustomerId != tenant.StripeCustomerId)
            {
                await stripePaymentService.AttachPaymentMethodAsync(command.StripePaymentMethodId, tenant);
            }
        }

        var paymentMethod = new CardPaymentMethod
        {
            CardNumber = command.CardNumber!.Replace(" ", ""),
            Cvc = command.Cvc!,
            ExpMonth = command.ExpMonth!.Value,
            ExpYear = command.ExpYear!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
            CardHolderName = command.CardHolderName!,
            VerificationStatus = PaymentMethodVerificationStatus.Verified,
            StripePaymentMethodId = command.StripePaymentMethodId
        };

        await tenantUow.Repository<CardPaymentMethod>().AddAsync(paymentMethod);

        if (setDefault && !string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            await stripePaymentService.SetDefaultPaymentMethodAsync(paymentMethod, tenant);
        }

        logger.LogInformation(
            "Created card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
    }

    private async Task CreateUsBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // Verify and attach Stripe payment method if provided
        if (!string.IsNullOrEmpty(command.StripePaymentMethodId))
        {
            var stripePaymentMethod = await stripePaymentService.GetPaymentMethodAsync(command.StripePaymentMethodId);
            if (stripePaymentMethod is null)
            {
                throw new ArgumentException($"Payment method {command.StripePaymentMethodId} not found in Stripe");
            }

            // Attach to customer if not already attached
            if (stripePaymentMethod.CustomerId != tenant.StripeCustomerId)
            {
                await stripePaymentService.AttachPaymentMethodAsync(command.StripePaymentMethodId, tenant);
            }
        }

        var paymentMethod = new UsBankAccountPaymentMethod
        {
            AccountNumber = command.AccountNumber!.Replace(" ", ""),
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            RoutingNumber = command.RoutingNumber!.Replace(" ", ""),
            AccountHolderType = command.AccountHolderType!.Value,
            AccountType = command.AccountType!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
            VerificationStatus = command.VerificationStatus ?? PaymentMethodVerificationStatus.Unverified,
            VerificationUrl = command.VerificationUrl,
            StripePaymentMethodId = command.StripePaymentMethodId
        };

        await tenantUow.Repository<UsBankAccountPaymentMethod>().AddAsync(paymentMethod);

        if (setDefault && !string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            await stripePaymentService.SetDefaultPaymentMethodAsync(paymentMethod, tenant);
        }

        logger.LogInformation(
            "Created US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }

    private async Task CreateInternationalBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var paymentMethod = new BankAccountPaymentMethod
        {
            AccountNumber = command.AccountNumber!,
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            SwiftCode = command.SwiftCode!,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
            VerificationStatus = PaymentMethodVerificationStatus.Verified
        };

        // TODO: Stripe does not support international bank accounts yet

        await tenantUow.Repository<BankAccountPaymentMethod>().AddAsync(paymentMethod);
        logger.LogInformation(
            "Created international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }
}
