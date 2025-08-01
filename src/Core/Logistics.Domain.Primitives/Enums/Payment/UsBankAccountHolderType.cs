using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum UsBankAccountHolderType
{
    [Description("Individual"), EnumMember(Value = "individual")] 
    Individual,
    
    [Description("Business"), EnumMember(Value = "business")] 
    Business,
}