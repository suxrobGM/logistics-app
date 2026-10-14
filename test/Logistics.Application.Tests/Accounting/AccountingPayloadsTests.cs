using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class AccountingPayloadsTests
{
    private readonly AccountingProviderConfiguration _config = new()
    {
        ProviderType = AccountingProviderType.QuickBooksOnline,
        DefaultItemId = "1",
        DefaultExpenseAccountId = "60"
    };

    #region Invoice

    [Fact]
    public void Invoice_LineTypeMapped_UsesMappedItemElseDefault()
    {
        var invoice = NewInvoice();
        var mappings = new AccountingMappings
        {
            Items = [new LineItemTypeMapping { LineItemType = InvoiceLineItemType.FuelSurcharge, QboId = "3" }]
        };

        var (payload, _) = AccountingPayloads.Invoice(invoice,
            [Line(invoice, InvoiceLineItemType.BaseRate, 1000m, 0), Line(invoice, InvoiceLineItemType.FuelSurcharge, 120m, 1)],
            "42", _config, mappings);

        Assert.Equal(["1", "3"], payload.Lines.Select(l => l.ItemId));
        Assert.Equal(invoice.CreatedAt, payload.TxnDate);
        Assert.Equal(invoice.DueDate, payload.DueDate);
    }

    [Fact]
    public void Invoice_DueDateChange_ChangesTheHash()
    {
        var invoice = NewInvoice();
        var (_, before) = AccountingPayloads.Invoice(invoice, [], "42", _config, new AccountingMappings());

        invoice.DueDate = invoice.DueDate!.Value.AddDays(15);
        var (_, after) = AccountingPayloads.Invoice(invoice, [], "42", _config, new AccountingMappings());

        Assert.NotEqual(before, after);
    }

    [Fact]
    public void Invoice_TaxChange_ChangesTheHash()
    {
        var invoice = NewInvoice();
        var (_, before) = AccountingPayloads.Invoice(invoice, [], "42", _config, new AccountingMappings());

        invoice.TaxTotal = new Money { Amount = 19m, Currency = "USD" };
        var (_, after) = AccountingPayloads.Invoice(invoice, [], "42", _config, new AccountingMappings());

        Assert.NotEqual(before, after);
    }

    #endregion

    #region Customer

    [Fact]
    public void Customer_TaxIdChange_KeepsTheHash()
    {
        var customer = new Customer { Name = "Acme", TaxId = "11-111" };
        var (_, before) = AccountingPayloads.Customer(customer);

        customer.TaxId = "22-222";

        Assert.Equal(before, AccountingPayloads.Customer(customer).Hash);
    }

    [Fact]
    public void Customer_Line2OrCountryChange_ChangesTheHash()
    {
        var customer = new Customer { Name = "Acme", Address = new Address { Line1 = "1 Dock", City = "Newark", State = "NJ", ZipCode = "07102", Country = "US" } };
        var (_, before) = AccountingPayloads.Customer(customer);

        customer.Address = customer.Address with { Line2 = "Suite 4" };
        var (_, withLine2) = AccountingPayloads.Customer(customer);
        customer.Address = customer.Address with { Country = "CA" };

        Assert.NotEqual(before, withLine2);
        Assert.NotEqual(withLine2, AccountingPayloads.Customer(customer).Hash);
    }

    #endregion

    #region Expense

    [Fact]
    public void ExpenseAccountId_TruckCategoryMapped_UsesCategoryAccount()
    {
        var mappings = new AccountingMappings
        {
            TruckExpenseAccounts = [new TruckExpenseCategoryMapping { Category = TruckExpenseCategory.Fuel, QboId = "61" }]
        };

        var fuel = NewTruckExpense(TruckExpenseCategory.Fuel);
        var tolls = NewTruckExpense(TruckExpenseCategory.Toll);

        Assert.Equal("61", AccountingPayloads.ExpenseAccountId(fuel, _config, mappings));
        Assert.Equal("60", AccountingPayloads.ExpenseAccountId(tolls, _config, mappings));
    }

    [Fact]
    public void ExpenseAccountId_CompanyCategoryMapped_UsesCategoryAccount()
    {
        var mappings = new AccountingMappings
        {
            CompanyExpenseAccounts = [new CompanyExpenseCategoryMapping { Category = CompanyExpenseCategory.Insurance, QboId = "70" }]
        };
        var expense = new CompanyExpense
        {
            Category = CompanyExpenseCategory.Insurance,
            Amount = new Money { Amount = 900m, Currency = "USD" },
            ExpenseDate = DateTime.UtcNow
        };

        Assert.Equal("70", AccountingPayloads.ExpenseAccountId(expense, _config, mappings));
    }

    #endregion

    #region Driver pay

    [Fact]
    public void Bill_MapsPayrollTotalPeriodAndNumber()
    {
        var payroll = new PayrollInvoice
        {
            EmployeeId = Guid.NewGuid(),
            Number = 9,
            PeriodStart = new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            PeriodEnd = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc),
            Subtotal = new Money { Amount = 1500m, Currency = "USD" },
            TaxTotal = Money.Zero("USD"),
            Total = new Money { Amount = 1500m, Currency = "USD" }
        };

        var (bill, _) = AccountingPayloads.Bill(payroll, "56", "62");

        Assert.Equal("56", bill.VendorId);
        Assert.Equal("62", bill.ExpenseAccountId);
        Assert.Equal(1500m, bill.Amount);
        Assert.Equal(payroll.PeriodEnd, bill.TxnDate);
        Assert.Equal("PAY-9", bill.DocNumber);
    }

    [Fact]
    public void RequestId_SameRecordAndContent_IsStableAndFitsQuickBooksLimit()
    {
        var id = Guid.NewGuid();

        var first = AccountingPayloads.RequestId(id, "hash");

        Assert.Equal(first, AccountingPayloads.RequestId(id, "hash"));
        Assert.NotEqual(first, AccountingPayloads.RequestId(id, "other"));
        Assert.True(first.Length <= 50);
    }

    #endregion

    private static LoadInvoice NewInvoice() => new()
    {
        LoadId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Number = 1007,
        CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
        DueDate = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc),
        Subtotal = Money.Zero("USD"),
        TaxTotal = Money.Zero("USD"),
        Total = Money.Zero("USD")
    };

    private static InvoiceLineItem Line(LoadInvoice invoice, InvoiceLineItemType type, decimal amount, int order) => new()
    {
        InvoiceId = invoice.Id,
        Description = type.ToString(),
        Type = type,
        Amount = new Money { Amount = amount, Currency = "USD" },
        Order = order
    };

    private static TruckExpense NewTruckExpense(TruckExpenseCategory category) => new()
    {
        TruckId = Guid.NewGuid(),
        Category = category,
        Amount = new Money { Amount = 100m, Currency = "USD" },
        ExpenseDate = DateTime.UtcNow
    };
}
