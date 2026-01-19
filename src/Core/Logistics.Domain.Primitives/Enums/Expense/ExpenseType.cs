using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum ExpenseType
{
    [Description("Company"), EnumMember(Value = "company")]
    Company,

    [Description("Truck"), EnumMember(Value = "truck")]
    Truck,

    [Description("Body Shop"), EnumMember(Value = "body_shop")]
    BodyShop
}
