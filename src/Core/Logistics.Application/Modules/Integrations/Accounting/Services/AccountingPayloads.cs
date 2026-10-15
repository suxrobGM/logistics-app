using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// Builds the provider-neutral push payloads from domain entities, each with a content hash of
/// exactly the fields that payload sends - an edit to anything else does not trigger a push.
/// </summary>
internal static class AccountingPayloads
{
    /// <summary>The hash a mapping records once the provider record was deactivated or voided.</summary>
    public const string RetiredHash = "retired";

    public static (QboCustomerPayload Payload, string Hash) Customer(Customer customer)
    {
        var address = customer.Address;
        var payload = new QboCustomerPayload
        {
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            BillingAddress = address is null ? null : new QboAddressPayload
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country
            }
        };

        var hash = Hash(customer.Name, customer.Email, customer.Phone, address?.Line1, address?.Line2,
            address?.City, address?.State, address?.ZipCode, address?.Country);
        return (payload, hash);
    }

    public static (QboInvoicePayload Payload, string Hash) Invoice(
        LoadInvoice invoice, IEnumerable<InvoiceLineItem> lineItems, string qboCustomerId,
        AccountingProviderConfiguration config, AccountingMappings mappings)
    {
        var lines = lineItems
            .OrderBy(li => li.Order)
            .Select(li => new QboInvoiceLinePayload
            {
                Description = li.Description,
                Amount = li.Amount.Amount,
                Quantity = li.Quantity,
                ItemId = mappings.Items.FirstOrDefault(m => m.LineItemType == li.Type)?.QboId ?? config.DefaultItemId
            })
            .ToList();

        var payload = new QboInvoicePayload
        {
            QboCustomerId = qboCustomerId,
            DocNumber = invoice.Number.ToString(CultureInfo.InvariantCulture),
            CurrencyCode = invoice.Total.Currency,
            TxnDate = invoice.CreatedAt,
            DueDate = invoice.DueDate,
            TaxTotal = invoice.TaxTotal.Amount,
            Lines = lines
        };

        var lineHash = string.Join('|', lines.Select(l =>
            $"{l.Description}:{Num(l.Amount)}:{l.Quantity}:{l.ItemId}"));
        var hash = Hash(payload.DocNumber, payload.CurrencyCode, qboCustomerId, Date(payload.TxnDate),
            Date(payload.DueDate), Num(payload.TaxTotal), lineHash);
        return (payload, hash);
    }

    public static (QboPaymentPayload Payload, string Hash) Payment(
        Payment payment, string qboCustomerId, string qboInvoiceId, string? depositAccountId)
    {
        var payload = new QboPaymentPayload
        {
            QboCustomerId = qboCustomerId,
            QboInvoiceId = qboInvoiceId,
            Amount = payment.Amount.Amount,
            CurrencyCode = payment.Amount.Currency,
            TxnDate = payment.RecordedAt ?? payment.CreatedAt,
            ReferenceNumber = payment.ReferenceNumber ?? payment.StripePaymentIntentId,
            DepositAccountId = depositAccountId
        };

        var hash = Hash(Num(payload.Amount), payload.CurrencyCode, qboCustomerId, qboInvoiceId,
            Date(payload.TxnDate), payload.ReferenceNumber, depositAccountId);
        return (payload, hash);
    }

    /// <summary>The account an expense posts to: its category's mapping, else the default expense account.</summary>
    public static string? ExpenseAccountId(
        Expense expense, AccountingProviderConfiguration config, AccountingMappings mappings)
    {
        var mapped = expense switch
        {
            CompanyExpense company => mappings.CompanyExpenseAccounts.FirstOrDefault(m => m.Category == company.Category)?.QboId,
            TruckExpense truck => mappings.TruckExpenseAccounts.FirstOrDefault(m => m.Category == truck.Category)?.QboId,
            _ => null
        };
        return mapped ?? config.DefaultExpenseAccountId;
    }

    /// <summary>The payee vendor id is resolved at push time (<c>payload with { VendorId }</c>); the hash keys on its name.</summary>
    public static (QboExpensePayload Payload, string Hash) Expense(
        Expense expense, string paymentAccountId, string? paymentAccountType, string expenseAccountId)
    {
        var payload = new QboExpensePayload
        {
            PaymentAccountId = paymentAccountId,
            PaymentAccountType = paymentAccountType,
            ExpenseAccountId = expenseAccountId,
            Amount = expense.Amount.Amount,
            CurrencyCode = expense.Amount.Currency,
            TxnDate = expense.ExpenseDate,
            DocNumber = $"EXP-{expense.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = ExpenseDescription(expense)
        };

        var hash = Hash(payload.DocNumber, Num(payload.Amount), payload.CurrencyCode, Date(payload.TxnDate),
            paymentAccountId, paymentAccountType, expenseAccountId, expense.VendorName, payload.Description);
        return (payload, hash);
    }

    /// <summary>The fee a factor kept on an invoice, paid out of the (bank-type) factoring clearing account.</summary>
    public static (QboExpensePayload Payload, string Hash) FactoringFee(
        FactoringBatchItem item, long batchNumber, long invoiceNumber, string clearingAccountId, string feeAccountId)
    {
        var payload = new QboExpensePayload
        {
            PaymentAccountId = clearingAccountId,
            PaymentAccountType = "Bank",
            ExpenseAccountId = feeAccountId,
            Amount = item.FeeAmount ?? 0m,
            CurrencyCode = item.Amount.Currency,
            TxnDate = item.FundedAt ?? item.CreatedAt,
            DocNumber = $"FCT-{batchNumber.ToString(CultureInfo.InvariantCulture)}-{invoiceNumber.ToString(CultureInfo.InvariantCulture)}",
            Description = $"Factoring fee on invoice {invoiceNumber.ToString(CultureInfo.InvariantCulture)} (schedule {batchNumber.ToString(CultureInfo.InvariantCulture)})"
        };

        var hash = Hash(payload.DocNumber, Num(payload.Amount), payload.CurrencyCode, Date(payload.TxnDate),
            clearingAccountId, feeAccountId);
        return (payload, hash);
    }

    public static (QboVendorPayload Payload, string Hash) Vendor(Employee employee)
    {
        var payload = new QboVendorPayload
        {
            DisplayName = $"{employee.FirstName} {employee.LastName}".Trim(),
            GivenName = employee.FirstName,
            FamilyName = employee.LastName,
            Email = employee.Email,
            Phone = employee.PhoneNumber
        };
        return (payload, Hash(payload.DisplayName, payload.Email, payload.Phone));
    }

    public static (QboBillPayload Payload, string Hash) Bill(
        PayrollInvoice payroll, string vendorId, string expenseAccountId)
    {
        var payload = new QboBillPayload
        {
            VendorId = vendorId,
            ExpenseAccountId = expenseAccountId,
            Amount = payroll.Total.Amount,
            CurrencyCode = payroll.Total.Currency,
            TxnDate = payroll.PeriodEnd,
            DueDate = payroll.DueDate,
            DocNumber = $"PAY-{payroll.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = $"Driver pay {Date(payroll.PeriodStart)} to {Date(payroll.PeriodEnd)}"
        };

        var hash = Hash(payload.DocNumber, Num(payload.Amount), payload.CurrencyCode, vendorId, expenseAccountId,
            Date(payload.TxnDate), Date(payload.DueDate), payload.Description);
        return (payload, hash);
    }

    /// <summary>
    /// Provider request id for a create: stable for the same record and content, so a retry after a crash
    /// between the provider's create and our save returns the record already created. 48 chars (QBO allows 50).
    /// </summary>
    public static string RequestId(Guid localId, string hash) => $"{localId:N}{Hash(hash)[..16]}";

    public static string Hash(params string?[] parts)
    {
        var joined = string.Join('|', parts.Select(p => p ?? string.Empty));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(joined)));
    }

    private static string? ExpenseDescription(Expense expense) =>
        string.IsNullOrEmpty(expense.VendorName)
            ? expense.Notes
            : string.IsNullOrEmpty(expense.Notes) ? expense.VendorName : $"{expense.VendorName} - {expense.Notes}";

    private static string Num(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string? Date(DateTime? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
