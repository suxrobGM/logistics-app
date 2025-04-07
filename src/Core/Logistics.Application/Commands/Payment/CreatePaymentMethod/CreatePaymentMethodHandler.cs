using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentMethodHandler : RequestHandler<CreatePaymentMethodCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ILogger<CreatePaymentMethodHandler> _logger;

    public CreatePaymentMethodHandler(
        IMasterUnityOfWork masterUow, 
        ILogger<CreatePaymentMethodHandler> logger)
    {
        _masterUow = masterUow;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        CreatePaymentMethodCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);

        if (tenant is null)
        {
            return Result.Fail(
                $"Tenant with id {req.TenantId} not found");
        }
        
        // If there are no payment methods for the tenant, set the first one as default
        var paymentMethodsCount = await _masterUow.Repository<PaymentMethod>()
            .CountAsync(i => i.TenantId == req.TenantId);

        switch (req.Type)
        {
            case PaymentMethodType.Card:
                await CreateCardPaymentMethod(req, tenant, paymentMethodsCount == 0);
                break;
            case PaymentMethodType.UsBankAccount:
                await CreateUsBankAccountPaymentMethod(req, tenant, paymentMethodsCount == 0);
                break;
            case PaymentMethodType.InternationalBankAccount:
                await CreateInternationalBankAccountPaymentMethod(req, tenant, paymentMethodsCount == 0);
                break;
            default:
                return Result.Fail($"Unsupported payment method type: {req.Type}");
        }
        
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
    
    private async Task CreateCardPaymentMethod(
        CreatePaymentMethodCommand command, Tenant tenant, bool setDefault = false)
    {
        var paymentMethod = new CardPaymentMethod
        {
            Type = PaymentMethodType.Card,
            Tenant = tenant,
            TenantId = tenant.Id,
            CardBrand = command.CardBrand!,
            CardNumber = command.CardNumber!,
            Cvv = command.Cvv!,
            ExpMonth = command.ExpMonth!.Value,
            ExpYear = command.ExpYear!.Value,
            FundingType = command.FundingType!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
            CardHolderName = command.CardHolderName!,
        };
        
        await _masterUow.Repository<CardPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created card payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.CardNumber![^4..]);
    }
    
    private async Task CreateUsBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, Tenant tenant, bool setDefault = false)
    {
        var paymentMethod = new UsBankAccountPaymentMethod
        {
            Type = PaymentMethodType.UsBankAccount,
            Tenant = tenant,
            TenantId = tenant.Id,
            AccountNumber = command.AccountNumber!,
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            RoutingNumber = command.RoutingNumber!,
            AccountHolderType = command.AccountHolderType!.Value,
            AccountType = command.AccountType!.Value,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
        };
        
        await _masterUow.Repository<UsBankAccountPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created US bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }
    
    private async Task CreateInternationalBankAccountPaymentMethod(
        CreatePaymentMethodCommand command, Tenant tenant, bool setDefault = false)
    {
        var paymentMethod = new BankAccountPaymentMethod()
        {
            Type = PaymentMethodType.InternationalBankAccount,
            Tenant = tenant,
            TenantId = tenant.Id,
            AccountNumber = command.AccountNumber!,
            AccountHolderName = command.AccountHolderName!,
            BankName = command.BankName!,
            SwiftCode = command.SwiftCode!,
            BillingAddress = command.BillingAddress,
            IsDefault = setDefault,
        };
        
        await _masterUow.Repository<BankAccountPaymentMethod>().AddAsync(paymentMethod);
        _logger.LogInformation(
            "Created international bank account payment method for tenant {TenantId} with last 4 digits {Last4}",
            tenant.Id, command.AccountNumber![^4..]);
    }
}
