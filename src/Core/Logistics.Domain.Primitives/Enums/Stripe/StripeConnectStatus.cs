using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum StripeConnectStatus
{
    [Description("Not Connected"), EnumMember(Value = "not_connected")]
    NotConnected,

    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,

    [Description("Active"), EnumMember(Value = "active")]
    Active,

    [Description("Restricted"), EnumMember(Value = "restricted")]
    Restricted,

    [Description("Disabled"), EnumMember(Value = "disabled")]
    Disabled
}
