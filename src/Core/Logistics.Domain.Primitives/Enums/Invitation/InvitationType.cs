using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvitationType
{
    [Description("Employee")] [EnumMember(Value = "employee")]
    Employee,

    [Description("Customer User")] [EnumMember(Value = "customer_user")]
    CustomerUser
}
