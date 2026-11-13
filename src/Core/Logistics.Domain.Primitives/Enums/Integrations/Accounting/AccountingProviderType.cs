using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum AccountingProviderType
{
    [Description("QuickBooks Online")]
    QuickBooksOnline = 1,
    Demo = 99
}
