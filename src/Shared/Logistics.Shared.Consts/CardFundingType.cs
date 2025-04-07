using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum CardFundingType
{
    [Description("Debit")] Debit,
    [Description("Credit")] Credit,
    [Description("Prepaid")] Prepaid,
}