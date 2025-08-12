using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum UsBankAccountType
{
    [Description("Checking"), EnumMember(Value = "checking")]
    Checking,

    [Description("Savings"), EnumMember(Value = "savings")]
    Savings,
}
