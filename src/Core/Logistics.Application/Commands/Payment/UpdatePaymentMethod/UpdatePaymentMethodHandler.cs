using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentMethodHandler : RequestHandler<UpdatePaymentMethodCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<UpdatePaymentMethodHandler> _logger;

    public UpdatePaymentMethodHandler(
        ITenantUnityOfWork tenantUow,
        IStripeService stripeService,
        ILogger<UpdatePaymentMethodHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        UpdatePaymentMethodCommand req, CancellationToken cancellationToken)
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
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = await _tenantUow.Repository<CardPaymentMethod>().GetByIdAsync(command.Id);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }
        
        paymentMethod.CardNumber = PropertyUpdater.UpdateIfChanged(command.CardNumber, paymentMethod.CardNumber);
        paymentMethod.Cvc = PropertyUpdater.UpdateIfChanged(command.Cvc, paymentMethod.Cvc);
        paymentMethod.ExpMonth = PropertyUpdater.UpdateIfChanged(command.ExpMonth, paymentMethod.ExpMonth);
        paymentMethod.ExpYear = PropertyUpdater.UpdateIfChanged(command.ExpYear, paymentMethod.ExpYear);
        paymentMethod.BillingAddress = PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        paymentMethod.CardHolderName = PropertyUpdater.UpdateIfChanged(command.CardHolderName, paymentMethod.CardHolderName);

        await _stripeService.UpdatePaymentMethodAsync(paymentMethod);
        _tenantUow.Repository<CardPaymentMethod>().Update(paymentMethod);
        await _tenantUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
        return Result.Succeed();
    }
    
    private async Task<Result> UpdateUsBankAccountPaymentMethod(UpdatePaymentMethodCommand command)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = await _tenantUow.Repository<UsBankAccountPaymentMethod>().GetByIdAsync(command.Id);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }
        
        paymentMethod.AccountNumber = PropertyUpdater.UpdateIfChanged(command.AccountNumber, paymentMethod.AccountNumber);
        paymentMethod.BankName = PropertyUpdater.UpdateIfChanged(command.BankName, paymentMethod.BankName);
        paymentMethod.RoutingNumber = PropertyUpdater.UpdateIfChanged(command.RoutingNumber, paymentMethod.RoutingNumber);
        paymentMethod.AccountHolderType = PropertyUpdater.UpdateIfChanged(command.AccountHolderType, paymentMethod.AccountHolderType);
        paymentMethod.AccountHolderName = PropertyUpdater.UpdateIfChanged(command.AccountHolderName, paymentMethod.AccountHolderName);
        paymentMethod.AccountType = PropertyUpdater.UpdateIfChanged(command.AccountType, paymentMethod.AccountType);
        paymentMethod.BillingAddress = PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        
        _tenantUow.Repository<UsBankAccountPaymentMethod>().Update(paymentMethod);
        await _tenantUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Succeed();
    }
    
    private async Task<Result> UpdateInternationalBankAccountPaymentMethod(UpdatePaymentMethodCommand command)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = await _tenantUow.Repository<BankAccountPaymentMethod>().GetByIdAsync(command.Id);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }
        
        paymentMethod.AccountNumber = PropertyUpdater.UpdateIfChanged(command.AccountNumber, paymentMethod.AccountNumber);
        paymentMethod.BankName = PropertyUpdater.UpdateIfChanged(command.BankName, paymentMethod.BankName);
        paymentMethod.AccountHolderName = PropertyUpdater.UpdateIfChanged(command.AccountHolderName, paymentMethod.AccountHolderName);
        paymentMethod.SwiftCode = PropertyUpdater.UpdateIfChanged(command.SwiftCode, paymentMethod.SwiftCode);
        paymentMethod.BillingAddress = PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        
        _tenantUow.Repository<BankAccountPaymentMethod>().Update(paymentMethod);
        await _tenantUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Succeed();
    }
}
