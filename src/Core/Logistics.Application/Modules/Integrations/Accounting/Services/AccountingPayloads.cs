using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    /// <summary>
    /// Pinned rather than shared with the API's options, so a change to those never reshapes the JSON
    /// and re-pushes every record.
    /// </summary>
    private static readonly JsonSerializerOptions HashJsonOptions = new(JsonSerializerDefaults.General);

    public static (QboCustomerPayload Payload, string Hash) Customer(Customer customer)
    {
        var address = customer.Address;
        return WithHash(new QboCustomerPayload
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
        });
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

        return WithHash(new QboInvoicePayload
        {
            QboCustomerId = qboCustomerId,
            DocNumber = invoice.Number.ToString(CultureInfo.InvariantCulture),
            CurrencyCode = invoice.Total.Currency,
            TxnDate = invoice.CreatedAt,
            DueDate = invoice.DueDate,
            TaxTotal = invoice.TaxTotal.Amount,
            Lines = lines
        });
    }

    public static (QboPaymentPayload Payload, string Hash) Payment(
        Payment payment, string qboCustomerId, string qboInvoiceId, string? depositAccountId) =>
        WithHash(new QboPaymentPayload
        {
            QboCustomerId = qboCustomerId,
            QboInvoiceId = qboInvoiceId,
            Amount = payment.Amount.Amount,
            CurrencyCode = payment.Amount.Currency,
            TxnDate = payment.RecordedAt ?? payment.CreatedAt,
            ReferenceNumber = payment.ReferenceNumber ?? payment.StripePaymentIntentId,
            DepositAccountId = depositAccountId
        });

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

    public static (QboExpensePayload Payload, string Hash) Expense(
        Expense expense, string paymentAccountId, string? paymentAccountType, string expenseAccountId) =>
        WithHash(new QboExpensePayload
        {
            PaymentAccountId = paymentAccountId,
            PaymentAccountType = paymentAccountType,
            ExpenseAccountId = expenseAccountId,
            Amount = expense.Amount.Amount,
            CurrencyCode = expense.Amount.Currency,
            TxnDate = expense.ExpenseDate,
            DocNumber = $"EXP-{expense.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = ExpenseDescription(expense)
        });

    /// <summary>The fee a factor kept on an invoice, paid out of the (bank-type) factoring clearing account.</summary>
    public static (QboExpensePayload Payload, string Hash) FactoringFee(
        FactoringBatchItem item, long batchNumber, long invoiceNumber, string clearingAccountId, string feeAccountId)
    {
        var batch = batchNumber.ToString(CultureInfo.InvariantCulture);
        var invoice = invoiceNumber.ToString(CultureInfo.InvariantCulture);
        return WithHash(new QboExpensePayload
        {
            PaymentAccountId = clearingAccountId,
            PaymentAccountType = "Bank",
            ExpenseAccountId = feeAccountId,
            Amount = item.FeeAmount ?? 0m,
            CurrencyCode = item.Amount.Currency,
            TxnDate = item.FundedAt ?? item.CreatedAt,
            DocNumber = $"FCT-{batch}-{invoice}",
            Description = $"Factoring fee on invoice {invoice} (schedule {batch})"
        });
    }

    public static (QboVendorPayload Payload, string Hash) Vendor(Employee employee) =>
        WithHash(new QboVendorPayload
        {
            DisplayName = $"{employee.FirstName} {employee.LastName}".Trim(),
            GivenName = employee.FirstName,
            FamilyName = employee.LastName,
            Email = employee.Email,
            Phone = employee.PhoneNumber
        });

    public static (QboBillPayload Payload, string Hash) Bill(
        PayrollInvoice payroll, string vendorId, string expenseAccountId) =>
        WithHash(new QboBillPayload
        {
            VendorId = vendorId,
            ExpenseAccountId = expenseAccountId,
            Amount = payroll.Total.Amount,
            CurrencyCode = payroll.Total.Currency,
            TxnDate = payroll.PeriodEnd,
            DueDate = payroll.DueDate,
            DocNumber = $"PAY-{payroll.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = $"Driver pay {Date(payroll.PeriodStart)} to {Date(payroll.PeriodEnd)}"
        });

    /// <summary>
    /// Provider request id for a create: stable for the same record and content, so a retry after a crash
    /// between the provider's create and our save returns the record already created. 48 chars (QBO allows 50).
    /// </summary>
    public static string RequestId(Guid localId, string hash) => $"{localId:N}{Hash(hash)[..16]}";

    private static (T Payload, string Hash) WithHash<T>(T payload) =>
        (payload, Hash(JsonSerializer.Serialize(payload, HashJsonOptions)));

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string? ExpenseDescription(Expense expense) =>
        string.IsNullOrEmpty(expense.VendorName)
            ? expense.Notes
            : string.IsNullOrEmpty(expense.Notes) ? expense.VendorName : $"{expense.VendorName} - {expense.Notes}";

    private static string Date(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
