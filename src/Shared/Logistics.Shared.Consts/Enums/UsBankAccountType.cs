using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum UsBankAccountType
{
    [Description("Checking")] Checking,
    [Description("Savings")] Savings,
}