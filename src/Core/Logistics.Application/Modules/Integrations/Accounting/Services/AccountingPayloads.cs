using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>Builds QuickBooks payloads and their content hashes. Each hash covers only the fields its payload sends, so edits to other fields do not trigger a re-push.</summary>
internal static class AccountingPayloads
{
    /// <summary>Fixed serializer options for hashing, so changing the API's JSON settings does not alter every hash and re-push every record.</summary>
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
        AccountingProviderConfiguration config, AccountingMappings mappings, string currency)
    {
        var lines = lineItems
            .OrderBy(li => li.Position)
            .Select(li => new QboInvoiceLinePayload
            {
                Description = li.Description,
                Amount = li.Amount,
                Quantity = li.Quantity,
                ItemId = mappings.Items.FirstOrDefault(m => m.LineItemType == li.Type)?.QboId ?? config.DefaultItemId
            })
            .ToList();

        return WithHash(new QboInvoicePayload
        {
            QboCustomerId = qboCustomerId,
            DocNumber = invoice.Number.ToString(CultureInfo.InvariantCulture),
            CurrencyCode = currency,
            TxnDate = invoice.CreatedAt,
            DueDate = invoice.DueDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TaxTotal = invoice.TaxTotal,
            Lines = lines
        });
    }

    public static (QboPaymentPayload Payload, string Hash) Payment(
        Payment payment, string qboCustomerId, string qboInvoiceId, string? depositAccountId, string currency) =>
        WithHash(new QboPaymentPayload
        {
            QboCustomerId = qboCustomerId,
            QboInvoiceId = qboInvoiceId,
            Amount = payment.Amount,
            CurrencyCode = currency,
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
        Expense expense, string paymentAccountId, string? paymentAccountType, string expenseAccountId, string currency) =>
        WithHash(new QboExpensePayload
        {
            PaymentAccountId = paymentAccountId,
            PaymentAccountType = paymentAccountType,
            ExpenseAccountId = expenseAccountId,
            Amount = expense.Amount,
            CurrencyCode = currency,
            TxnDate = expense.ExpenseDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            DocNumber = $"EXP-{expense.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = ExpenseDescription(expense)
        });

    /// <summary>The fee a factor kept on an invoice, paid out of the (bank-type) factoring clearing account.</summary>
    public static (QboExpensePayload Payload, string Hash) FactoringFee(
        FactoringBatchItem item, long batchNumber, long invoiceNumber, string clearingAccountId, string feeAccountId,
        string currency)
    {
        var batch = batchNumber.ToString(CultureInfo.InvariantCulture);
        var invoice = invoiceNumber.ToString(CultureInfo.InvariantCulture);
        return WithHash(new QboExpensePayload
        {
            PaymentAccountId = clearingAccountId,
            PaymentAccountType = "Bank",
            ExpenseAccountId = feeAccountId,
            Amount = item.FeeAmount ?? 0m,
            CurrencyCode = currency,
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
        PayrollInvoice payroll, string vendorId, string expenseAccountId, string currency) =>
        WithHash(new QboBillPayload
        {
            VendorId = vendorId,
            ExpenseAccountId = expenseAccountId,
            Amount = payroll.Total,
            CurrencyCode = currency,
            TxnDate = payroll.PeriodEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            DueDate = payroll.DueDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            DocNumber = $"PAY-{payroll.Number.ToString(CultureInfo.InvariantCulture)}",
            Description = $"Driver pay {Date(payroll.PeriodStart)} to {Date(payroll.PeriodEnd)}"
        });

    /// <summary>QuickBooks request id for a create, stable per record and content, so a retry after a crash returns the record already created instead of a duplicate. QuickBooks allows at most 50 characters.</summary>
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

    private static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
