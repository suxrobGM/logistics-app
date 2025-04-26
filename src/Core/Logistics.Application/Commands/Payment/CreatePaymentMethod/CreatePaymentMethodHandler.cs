using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentMethodHandler : RequestHandler<CreatePaymentMethodCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<CreatePaymentMethodHandler> _logger;

    public CreatePaymentMethodHandler(
        ITenantUnityOfWork tenantUow,
        IStripeService stripeService,
        ILogger<CreatePaymentMethodHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        CreatePaymentMethodCommand req, CancellationToken cancellationToken)
    {
        // If there are no payment methods for the tenant, set the first one as default
        var paymentMethodsCount = await _tenantUow.Repository<PaymentMethod>().CountAsync();

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
        
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
    
    private async Task CreateCardPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = new CardPaymentMethod
        {
            Type = PaymentMethodType.Card,
            CardNumber = command.CardNumber!.Replace(" ", ""),
            Cvc = command.Cvc!,
            ExpMonth = command.ExpMonth!.Value,
            ExpYear = command.ExpYear!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
            CardHolderName = command.CardHolderName!,
        };
        
        //await _stripeService.AddPaymentMethodAsync(paymentMethod, tenant);
        await _tenantUow.Repository<CardPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
    }
    
    private async Task CreateUsBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = new UsBankAccountPaymentMethod
        {
            Type = PaymentMethodType.UsBankAccount,
            AccountNumber = command.AccountNumber!.Replace(" ", ""),
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            RoutingNumber = command.RoutingNumber!.Replace(" ", ""),
            AccountHolderType = command.AccountHolderType!.Value,
            AccountType = command.AccountType!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
        };
        
        //await _stripeService.AddPaymentMethodAsync(paymentMethod, tenant);
        await _tenantUow.Repository<UsBankAccountPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }
    
    private async Task CreateInternationalBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, bool setDefault = false)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var paymentMethod = new BankAccountPaymentMethod()
        {
            Type = PaymentMethodType.InternationalBankAccount,
            AccountNumber = command.AccountNumber!,
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            SwiftCode = command.SwiftCode!,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
        };
        
        // TODO: Stripe does not support international bank accounts yet
        
        await _tenantUow.Repository<BankAccountPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }
}
