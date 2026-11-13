using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// NMFC freight class for LTL shipments (50 = dense and durable, 500 = light and bulky).
/// </summary>
public enum FreightClass
{
    [Description("50")] Class50,
    [Description("55")] Class55,
    [Description("60")] Class60,
    [Description("65")] Class65,
    [Description("70")] Class70,
    [Description("77.5")] Class77Point5,
    [Description("85")] Class85,
    [Description("92.5")] Class92Point5,
    [Description("100")] Class100,
    [Description("110")] Class110,
    [Description("125")] Class125,
    [Description("150")] Class150,
    [Description("175")] Class175,
    [Description("200")] Class200,
    [Description("250")] Class250,
    [Description("300")] Class300,
    [Description("400")] Class400,
    [Description("500")] Class500
}
