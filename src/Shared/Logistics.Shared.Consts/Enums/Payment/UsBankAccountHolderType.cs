using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum UsBankAccountHolderType
{
    [Description("Individual"), EnumMember(Value = "individual")] 
    Individual,
    
    [Description("Business"), EnumMember(Value = "business")] 
    Business,
}