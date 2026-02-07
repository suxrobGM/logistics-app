using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentMethodHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<UpdatePaymentMethodHandler> logger) : IAppRequestHandler<UpdatePaymentMethodCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePaymentMethodCommand req, CancellationToken ct)
    {
        return req.Type switch
        {
            PaymentMethodType.Card => await UpdateCardPaymentMethod(req),
            PaymentMethodType.UsBankAccount => await UpdateUsBankAccountPaymentMethod(req),
            PaymentMethodType.InternationalBankAccount => await UpdateInternationalBankAccountPaymentMethod(req),
            _ => Result.Fail($"Unsupported payment method type: {req.Type}")
        };
    }

    private async Task<Result> UpdateCardPaymentMethod(UpdatePaymentMethodCommand command)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var paymentMethod = await tenantUow.Repository<CardPaymentMethod>().GetByIdAsync(command.Id);

        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }

        paymentMethod.CardNumber = PropertyUpdater.UpdateIfChanged(command.CardNumber, paymentMethod.CardNumber);
        paymentMethod.Cvc = PropertyUpdater.UpdateIfChanged(command.Cvc, paymentMethod.Cvc);
        paymentMethod.ExpMonth = PropertyUpdater.UpdateIfChanged(command.ExpMonth, paymentMethod.ExpMonth);
        paymentMethod.ExpYear = PropertyUpdater.UpdateIfChanged(command.ExpYear, paymentMethod.ExpYear);
        paymentMethod.BillingAddress =
            PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        paymentMethod.CardHolderName =
            PropertyUpdater.UpdateIfChanged(command.CardHolderName, paymentMethod.CardHolderName);

        await stripePaymentService.UpdatePaymentMethodAsync(paymentMethod);
        tenantUow.Repository<CardPaymentMethod>().Update(paymentMethod);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Updated card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
        return Result.Ok();
    }

    private async Task<Result> UpdateUsBankAccountPaymentMethod(UpdatePaymentMethodCommand command)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var paymentMethod = await tenantUow.Repository<UsBankAccountPaymentMethod>().GetByIdAsync(command.Id);

        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }

        paymentMethod.AccountNumber =
            PropertyUpdater.UpdateIfChanged(command.AccountNumber, paymentMethod.AccountNumber);
        paymentMethod.BankName = PropertyUpdater.UpdateIfChanged(command.BankName, paymentMethod.BankName);
        paymentMethod.RoutingNumber =
            PropertyUpdater.UpdateIfChanged(command.RoutingNumber, paymentMethod.RoutingNumber);
        paymentMethod.AccountHolderType =
            PropertyUpdater.UpdateIfChanged(command.AccountHolderType, paymentMethod.AccountHolderType);
        paymentMethod.AccountHolderName =
            PropertyUpdater.UpdateIfChanged(command.AccountHolderName, paymentMethod.AccountHolderName);
        paymentMethod.AccountType = PropertyUpdater.UpdateIfChanged(command.AccountType, paymentMethod.AccountType);
        paymentMethod.BillingAddress =
            PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);

        tenantUow.Repository<UsBankAccountPaymentMethod>().Update(paymentMethod);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Updated US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Ok();
    }

    private async Task<Result> UpdateInternationalBankAccountPaymentMethod(UpdatePaymentMethodCommand command)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var paymentMethod = await tenantUow.Repository<BankAccountPaymentMethod>().GetByIdAsync(command.Id);

        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }

        paymentMethod.AccountNumber =
            PropertyUpdater.UpdateIfChanged(command.AccountNumber, paymentMethod.AccountNumber);
        paymentMethod.BankName = PropertyUpdater.UpdateIfChanged(command.BankName, paymentMethod.BankName);
        paymentMethod.AccountHolderName =
            PropertyUpdater.UpdateIfChanged(command.AccountHolderName, paymentMethod.AccountHolderName);
        paymentMethod.SwiftCode = PropertyUpdater.UpdateIfChanged(command.SwiftCode, paymentMethod.SwiftCode);
        paymentMethod.BillingAddress =
            PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);

        tenantUow.Repository<BankAccountPaymentMethod>().Update(paymentMethod);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation(
            "Updated international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Ok();
    }
}
