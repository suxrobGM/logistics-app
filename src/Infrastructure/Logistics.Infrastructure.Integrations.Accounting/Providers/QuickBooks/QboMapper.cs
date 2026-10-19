using System.Globalization;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

/// <summary>
///     Maps provider-neutral push payloads to QuickBooks Online wire models. With an existing
///     <c>qboId</c>/<c>syncToken</c> the result is a sparse update of that record, otherwise a create.
/// </summary>
internal static class QboMapper
{
    private const string CreditCardAccountType = "Credit Card";

    private static string Date(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static bool? SparseIfUpdate(string? qboId) => qboId is null ? null : true;

    public static QboCustomer MapCustomer(QboCustomerPayload p, string? qboId, string? syncToken)
    {
        return new QboCustomer
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            Active = qboId is null ? null : true,
            DisplayName = p.Name,
            PrimaryEmailAddr = string.IsNullOrWhiteSpace(p.Email) ? null : new QboEmail { Address = p.Email },
            PrimaryPhone = string.IsNullOrWhiteSpace(p.Phone) ? null : new QboPhone { FreeFormNumber = p.Phone },
            BillAddr = MapAddress(p.BillingAddress)
        };
    }

    public static QboCustomer MapCustomerDeactivation(string qboId, string? syncToken) =>
        new() { Id = qboId, SyncToken = syncToken, Sparse = true, Active = false };

    public static QboVendor MapVendor(QboVendorPayload p, string? qboId, string? syncToken)
    {
        return new QboVendor
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            Active = qboId is null ? null : true,
            DisplayName = p.DisplayName,
            GivenName = p.GivenName,
            FamilyName = p.FamilyName,
            PrimaryEmailAddr = string.IsNullOrWhiteSpace(p.Email) ? null : new QboEmail { Address = p.Email },
            PrimaryPhone = string.IsNullOrWhiteSpace(p.Phone) ? null : new QboPhone { FreeFormNumber = p.Phone }
        };
    }

    private static QboPhysicalAddress? MapAddress(QboAddressPayload? a)
    {
        if (a is null)
        {
            return null;
        }

        return new QboPhysicalAddress
        {
            Line1 = a.Line1,
            Line2 = a.Line2,
            City = a.City,
            CountrySubDivisionCode = a.State,
            PostalCode = a.ZipCode,
            Country = a.Country
        };
    }

    public static QboInvoice MapInvoice(QboInvoicePayload p, string? qboId, string? syncToken)
    {
        var lines = p.Lines
            .Select(l => new QboLine
            {
                DetailType = "SalesItemLineDetail",
                Description = l.Description,
                Amount = decimal.Round(l.Amount * l.Quantity, 2),
                SalesItemLineDetail = new QboSalesItemLineDetail
                {
                    Qty = l.Quantity,
                    UnitPrice = decimal.Round(l.Amount, 2),
                    ItemRef = QboRef.OfOptional(l.ItemId)
                }
            })
            .ToList();

        return new QboInvoice
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            DocNumber = p.DocNumber,
            TxnDate = Date(p.TxnDate),
            CustomerRef = QboRef.Create(p.QboCustomerId),
            CurrencyRef = QboRef.Create(p.CurrencyCode),
            DueDate = p.DueDate.HasValue ? Date(p.DueDate.Value) : null,
            TxnTaxDetail = p.TaxTotal > 0 ? new QboTxnTaxDetail { TotalTax = decimal.Round(p.TaxTotal, 2) } : null,
            Line = lines
        };
    }

    /// <summary>A void request carries only the identity; QBO zeroes the amounts and keeps the record.</summary>
    public static QboInvoice MapInvoiceVoid(string qboId, string? syncToken) =>
        new() { Id = qboId, SyncToken = syncToken };

    public static QboPayment MapPaymentVoid(string qboId, string? syncToken) =>
        new() { Id = qboId, SyncToken = syncToken, Sparse = true };

    public static QboPayment MapPayment(QboPaymentPayload p, string? qboId, string? syncToken)
    {
        return new QboPayment
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            TxnDate = Date(p.TxnDate),
            CustomerRef = QboRef.Create(p.QboCustomerId),
            CurrencyRef = QboRef.Create(p.CurrencyCode),
            DepositToAccountRef = QboRef.OfOptional(p.DepositAccountId),
            TotalAmt = decimal.Round(p.Amount, 2),
            PaymentRefNum = p.ReferenceNumber,
            Line =
            [
                new QboLine
                {
                    Amount = decimal.Round(p.Amount, 2),
                    LinkedTxn = [new QboLinkedTxn { TxnId = p.QboInvoiceId, TxnType = "Invoice" }]
                }
            ]
        };
    }

    public static QboPurchase MapPurchase(QboExpensePayload p, string? qboId, string? syncToken)
    {
        return new QboPurchase
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            AccountRef = QboRef.Create(p.PaymentAccountId),
            CurrencyRef = QboRef.Create(p.CurrencyCode),
            PaymentType = p.PaymentAccountType == CreditCardAccountType ? "CreditCard" : "Cash",
            TxnDate = Date(p.TxnDate),
            DocNumber = p.DocNumber,
            Line = [ExpenseLine(p.Description, p.Amount, p.ExpenseAccountId)]
        };
    }

    public static QboBill MapBill(QboBillPayload p, string? qboId, string? syncToken)
    {
        return new QboBill
        {
            Id = qboId,
            SyncToken = syncToken,
            Sparse = SparseIfUpdate(qboId),
            VendorRef = QboRef.Create(p.VendorId),
            CurrencyRef = QboRef.Create(p.CurrencyCode),
            TxnDate = Date(p.TxnDate),
            DueDate = p.DueDate.HasValue ? Date(p.DueDate.Value) : null,
            DocNumber = p.DocNumber,
            Line = [ExpenseLine(p.Description, p.Amount, p.ExpenseAccountId)]
        };
    }

    private static QboLine ExpenseLine(string? description, decimal amount, string accountId) => new()
    {
        DetailType = "AccountBasedExpenseLineDetail",
        Description = description,
        Amount = decimal.Round(amount, 2),
        AccountBasedExpenseLineDetail = new QboAccountBasedExpenseLineDetail { AccountRef = QboRef.Create(accountId) }
    };
}
