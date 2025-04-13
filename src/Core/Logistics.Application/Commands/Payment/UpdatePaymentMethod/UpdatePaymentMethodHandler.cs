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
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<UpdatePaymentMethodHandler> _logger;

    public UpdatePaymentMethodHandler(
        IMasterUnityOfWork masterUow,
        IStripeService stripeService,
        ILogger<UpdatePaymentMethodHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        UpdatePaymentMethodCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);

        if (tenant is null)
        {
            return Result.Fail(
                $"Tenant with id {req.TenantId} not found");
        }

        return req.Type switch
        {
            PaymentMethodType.Card => await UpdateCardPaymentMethod(req, tenant),
            PaymentMethodType.UsBankAccount => await UpdateUsBankAccountPaymentMethod(req, tenant),
            PaymentMethodType.InternationalBankAccount => await UpdateInternationalBankAccountPaymentMethod(req, tenant),
            _ => Result.Fail($"Unsupported payment method type: {req.Type}")
        };
    }
    
    private async Task<Result> UpdateCardPaymentMethod(UpdatePaymentMethodCommand command, Tenant tenant)
    {
        var paymentMethod = await _masterUow.Repository<CardPaymentMethod>().GetByIdAsync(command.Id);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }
        
        paymentMethod.CardBrand = PropertyUpdater.UpdateIfChanged(command.CardBrand, paymentMethod.CardBrand);
        paymentMethod.CardNumber = PropertyUpdater.UpdateIfChanged(command.CardNumber, paymentMethod.CardNumber);
        paymentMethod.Cvc = PropertyUpdater.UpdateIfChanged(command.Cvc, paymentMethod.Cvc);
        paymentMethod.ExpMonth = PropertyUpdater.UpdateIfChanged(command.ExpMonth, paymentMethod.ExpMonth);
        paymentMethod.ExpYear = PropertyUpdater.UpdateIfChanged(command.ExpYear, paymentMethod.ExpYear);
        paymentMethod.FundingType = PropertyUpdater.UpdateIfChanged(command.FundingType, paymentMethod.FundingType);
        paymentMethod.BillingAddress = PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        paymentMethod.CardHolderName = PropertyUpdater.UpdateIfChanged(command.CardHolderName, paymentMethod.CardHolderName);

        await _stripeService.UpdatePaymentMethodAsync(paymentMethod);
        
        _masterUow.Repository<CardPaymentMethod>().Update(paymentMethod);
        await _masterUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
        return Result.Succeed();
    }
    
    private async Task<Result> UpdateUsBankAccountPaymentMethod(UpdatePaymentMethodCommand command, Tenant tenant)
    {
        var paymentMethod = await _masterUow.Repository<UsBankAccountPaymentMethod>().GetByIdAsync(command.Id);
        
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
        
        _masterUow.Repository<UsBankAccountPaymentMethod>().Update(paymentMethod);
        await _masterUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Succeed();
    }
    
    private async Task<Result> UpdateInternationalBankAccountPaymentMethod(UpdatePaymentMethodCommand command, Tenant tenant)
    {
        var paymentMethod = await _masterUow.Repository<BankAccountPaymentMethod>().GetByIdAsync(command.Id);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method with id {command.Id} not found");
        }
        
        paymentMethod.AccountNumber = PropertyUpdater.UpdateIfChanged(command.AccountNumber, paymentMethod.AccountNumber);
        paymentMethod.BankName = PropertyUpdater.UpdateIfChanged(command.BankName, paymentMethod.BankName);
        paymentMethod.AccountHolderName = PropertyUpdater.UpdateIfChanged(command.AccountHolderName, paymentMethod.AccountHolderName);
        paymentMethod.SwiftCode = PropertyUpdater.UpdateIfChanged(command.SwiftCode, paymentMethod.SwiftCode);
        paymentMethod.BillingAddress = PropertyUpdater.UpdateIfChanged(command.BillingAddress, paymentMethod.BillingAddress);
        
        _masterUow.Repository<BankAccountPaymentMethod>().Update(paymentMethod);
        await _masterUow.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
        return Result.Succeed();
    }
}
