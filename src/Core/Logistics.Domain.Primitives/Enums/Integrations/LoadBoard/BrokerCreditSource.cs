using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum BrokerCreditSource
{
    [Description("DAT")]
    Dat = 1,

    Truckstop = 2,

    [Description("FMCSA")]
    Fmcsa = 3,

    Demo = 99
}
