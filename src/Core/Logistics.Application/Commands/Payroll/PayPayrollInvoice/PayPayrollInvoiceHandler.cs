using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class PayPayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<PayPayrollInvoiceHandler> logger)
    : IAppRequestHandler<PayPayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(PayPayrollInvoiceCommand req, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);

        if (invoice is not PayrollInvoice payrollInvoice)
        {
            return Result.Fail("Invoice not found or is not a payroll invoice.");
        }

        if (payrollInvoice.Status is not (InvoiceStatus.Approved or InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid))
        {
            return Result.Fail($"Payroll invoice must be approved before payment. Current status: {payrollInvoice.Status}");
        }

        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(payrollInvoice.EmployeeId, ct);

        if (employee is null)
        {
            return Result.Fail("Employee not found.");
        }

        if (string.IsNullOrEmpty(employee.StripeConnectedAccountId))
        {
            return Result.Fail("Employee does not have a payout account set up. Please complete Stripe onboarding first.");
        }

        // Calculate amount due
        var totalPaid = payrollInvoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = payrollInvoice.Total.Amount - totalPaid;

        if (amountDue <= 0)
        {
            return Result.Fail("This payroll invoice has already been fully paid.");
        }

        var tenant = tenantUow.GetCurrentTenant();

        try
        {
            var amountInCents = (long)(amountDue * 100);
            var transfer = await stripeConnectService.CreateTransferAsync(
                amountInCents,
                payrollInvoice.Total.Currency,
                employee.StripeConnectedAccountId,
                $"Payroll #{payrollInvoice.Number} for {employee.GetFullName()}");

            var payment = new Payment
            {
                Amount = new Money { Amount = amountDue, Currency = payrollInvoice.Total.Currency },
                Status = PaymentStatus.Paid,
                StripePaymentMethodId = null,
                StripePaymentIntentId = transfer.Id,
                TenantId = tenant.Id,
                Description = $"Stripe payout for Payroll #{payrollInvoice.Number}",
                BillingAddress = tenant.CompanyAddress
            };

            await tenantUow.Repository<Payment>().AddAsync(payment, ct);
            payrollInvoice.ApplyPaymentWithEvent(payment);
            tenantUow.Repository<Invoice>().Update(payrollInvoice);
            await tenantUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Paid payroll invoice {InvoiceId} for employee {EmployeeId} via Stripe Transfer {TransferId}, amount {Amount}",
                payrollInvoice.Id, employee.Id, transfer.Id, amountDue);

            return Result.Ok();
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to pay payroll invoice {InvoiceId} for employee {EmployeeId}",
                payrollInvoice.Id, employee.Id);
            return Result.Fail($"Payroll payout failed: {ex.Message}");
        }
    }
}
