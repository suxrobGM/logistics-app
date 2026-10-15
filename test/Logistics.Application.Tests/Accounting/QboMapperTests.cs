using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class QboMapperTests
{
    [Fact]
    public void MapCustomer_Create_MapsFieldsAndOmitsId()
    {
        var payload = new QboCustomerPayload
        {
            Name = "Acme Freight",
            Email = "ap@acme.test",
            Phone = "+1-555-0100",
            BillingAddress = new QboAddressPayload
            {
                Line1 = "1 Dock St",
                City = "Newark",
                State = "NJ",
                ZipCode = "07102",
                Country = "US"
            }
        };

        var result = QboMapper.MapCustomer(payload, qboId: null, syncToken: null);

        Assert.Null(result.Id);
        Assert.Null(result.SyncToken);
        Assert.Equal("Acme Freight", result.DisplayName);
        Assert.Equal("ap@acme.test", result.PrimaryEmailAddr?.Address);
        Assert.Equal("+1-555-0100", result.PrimaryPhone?.FreeFormNumber);
        Assert.Equal("Newark", result.BillAddr?.City);
        Assert.Equal("NJ", result.BillAddr?.CountrySubDivisionCode);
        Assert.Equal("07102", result.BillAddr?.PostalCode);
    }

    [Fact]
    public void MapCustomer_Update_CarriesIdAndSyncToken()
    {
        var payload = new QboCustomerPayload { Name = "Acme" };

        var result = QboMapper.MapCustomer(payload, qboId: "42", syncToken: "3");

        Assert.Equal("42", result.Id);
        Assert.Equal("3", result.SyncToken);
    }

    [Fact]
    public void MapCustomer_NoContact_LeavesEmailAndPhoneNull()
    {
        var result = QboMapper.MapCustomer(new QboCustomerPayload { Name = "Acme" }, null, null);

        Assert.Null(result.PrimaryEmailAddr);
        Assert.Null(result.PrimaryPhone);
        Assert.Null(result.BillAddr);
    }

    [Fact]
    public void MapInvoice_MultiLine_ComputesAmountsAndRefs()
    {
        var payload = new QboInvoicePayload
        {
            QboCustomerId = "42",
            DocNumber = "1007",
            CurrencyCode = "USD",
            DueDate = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            TaxTotal = 19.00m,
            Lines =
            [
                new QboInvoiceLinePayload { Description = "Line haul", Amount = 1000m, Quantity = 1 },
                new QboInvoiceLinePayload { Description = "Detention", Amount = 50m, Quantity = 2 }
            ]
        };

        var result = QboMapper.MapInvoice(payload, null, null);

        Assert.Equal("42", result.CustomerRef?.Value);
        Assert.Equal("USD", result.CurrencyRef?.Value);
        Assert.Equal("1007", result.DocNumber);
        Assert.Equal("2026-07-20", result.DueDate);
        Assert.Equal(19.00m, result.TxnTaxDetail?.TotalTax);
        Assert.Equal(2, result.Line!.Count);
        Assert.Equal(1000m, result.Line[0].Amount);
        Assert.Equal(100m, result.Line[1].Amount); // 50 * 2
        Assert.Equal(2m, result.Line[1].SalesItemLineDetail?.Qty);
        Assert.Equal("SalesItemLineDetail", result.Line[0].DetailType);
    }

    [Fact]
    public void MapInvoice_ZeroTax_OmitsTxnTaxDetail()
    {
        var payload = new QboInvoicePayload
        {
            QboCustomerId = "42",
            CurrencyCode = "USD",
            TaxTotal = 0m,
            Lines = [new QboInvoiceLinePayload { Description = "Haul", Amount = 500m, Quantity = 1 }]
        };

        var result = QboMapper.MapInvoice(payload, null, null);

        Assert.Null(result.TxnTaxDetail);
    }

    [Fact]
    public void MapPayment_LinksToInvoice()
    {
        var payload = new QboPaymentPayload
        {
            QboCustomerId = "42",
            QboInvoiceId = "1007",
            Amount = 1119m,
            CurrencyCode = "USD",
            ReferenceNumber = "CHK-88"
        };

        var result = QboMapper.MapPayment(payload, null, null);

        Assert.Equal("42", result.CustomerRef?.Value);
        Assert.Equal(1119m, result.TotalAmt);
        Assert.Equal("CHK-88", result.PaymentRefNum);
        var link = Assert.Single(result.Line!);
        Assert.Equal(1119m, link.Amount);
        Assert.Equal("1007", link.LinkedTxn?[0].TxnId);
        Assert.Equal("Invoice", link.LinkedTxn?[0].TxnType);
    }

    [Fact]
    public void MapPurchase_SetsPaymentAndExpenseAccountRefs()
    {
        var payload = new QboExpensePayload
        {
            PaymentAccountId = "35",
            ExpenseAccountId = "60",
            Amount = 250.55m,
            CurrencyCode = "USD",
            TxnDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            Description = "Pilot - fuel"
        };

        var result = QboMapper.MapPurchase(payload, null, null);

        Assert.Equal("35", result.AccountRef?.Value);
        Assert.Equal("2026-07-01", result.TxnDate);
        var line = Assert.Single(result.Line!);
        Assert.Equal("AccountBasedExpenseLineDetail", line.DetailType);
        Assert.Equal("60", line.AccountBasedExpenseLineDetail?.AccountRef?.Value);
        Assert.Equal(250.55m, line.Amount);
    }

    #region Fields added for production sync

    [Fact]
    public void MapInvoice_SendsTxnDateAndPerLineItemRef()
    {
        var payload = new QboInvoicePayload
        {
            QboCustomerId = "42",
            CurrencyCode = "USD",
            TxnDate = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
            Lines =
            [
                new QboInvoiceLinePayload { Description = "Line haul", Amount = 1000m, ItemId = "7" },
                new QboInvoiceLinePayload { Description = "Misc", Amount = 10m, ItemId = null }
            ]
        };

        var result = QboMapper.MapInvoice(payload, null, null);

        Assert.Equal("2026-07-05", result.TxnDate);
        Assert.Equal("7", result.Line![0].SalesItemLineDetail?.ItemRef?.Value);
        Assert.Null(result.Line[1].SalesItemLineDetail?.ItemRef);
        Assert.Null(result.Sparse);
    }

    [Fact]
    public void MapInvoice_Update_IsSparse()
    {
        var payload = new QboInvoicePayload { QboCustomerId = "42", CurrencyCode = "USD" };

        var result = QboMapper.MapInvoice(payload, "130", "2");

        Assert.True(result.Sparse);
        Assert.Equal("2", result.SyncToken);
    }

    [Fact]
    public void MapInvoiceVoid_CarriesOnlyIdentity()
    {
        var result = QboMapper.MapInvoiceVoid("130", "4");

        Assert.Equal("130", result.Id);
        Assert.Equal("4", result.SyncToken);
        Assert.Null(result.Line);
        Assert.Null(result.CustomerRef);
    }

    [Fact]
    public void MapPayment_SendsTxnDateAndDepositAccount()
    {
        var payload = new QboPaymentPayload
        {
            QboCustomerId = "42",
            QboInvoiceId = "130",
            Amount = 500m,
            CurrencyCode = "USD",
            TxnDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            DepositAccountId = "35"
        };

        var result = QboMapper.MapPayment(payload, null, null);

        Assert.Equal("2026-08-01", result.TxnDate);
        Assert.Equal("35", result.DepositToAccountRef?.Value);
    }

    [Fact]
    public void MapPayment_NoDepositAccount_LeavesQuickBooksDefault()
    {
        var payload = new QboPaymentPayload { QboCustomerId = "42", QboInvoiceId = "130", CurrencyCode = "USD" };

        Assert.Null(QboMapper.MapPayment(payload, null, null).DepositToAccountRef);
    }

    [Theory]
    [InlineData("Credit Card", "CreditCard")]
    [InlineData("Bank", "Cash")]
    [InlineData(null, "Cash")]
    public void MapPurchase_PaymentTypeFollowsThePaymentAccount(string? accountType, string expected)
    {
        var payload = new QboExpensePayload
        {
            PaymentAccountId = "41",
            PaymentAccountType = accountType,
            ExpenseAccountId = "61",
            CurrencyCode = "USD"
        };

        Assert.Equal(expected, QboMapper.MapPurchase(payload, null, null).PaymentType);
    }

    [Fact]
    public void MapBill_PostsToVendorAndPayrollAccount()
    {
        var payload = new QboBillPayload
        {
            VendorId = "56",
            ExpenseAccountId = "62",
            Amount = 1834.456m,
            CurrencyCode = "USD",
            TxnDate = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc),
            DueDate = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc),
            DocNumber = "PAY-9",
            Description = "Driver pay"
        };

        var result = QboMapper.MapBill(payload, null, null);

        Assert.Equal("56", result.VendorRef?.Value);
        Assert.Equal("2026-09-30", result.TxnDate);
        Assert.Equal("2026-10-03", result.DueDate);
        Assert.Equal("PAY-9", result.DocNumber);
        var line = Assert.Single(result.Line!);
        Assert.Equal(1834.46m, line.Amount);
        Assert.Equal("62", line.AccountBasedExpenseLineDetail?.AccountRef?.Value);
    }

    [Fact]
    public void MapCustomerDeactivation_IsSparseInactiveUpdate()
    {
        var result = QboMapper.MapCustomerDeactivation("58", "3");

        Assert.True(result.Sparse);
        Assert.False(result.Active);
        Assert.Null(result.DisplayName);
    }

    #endregion
}
