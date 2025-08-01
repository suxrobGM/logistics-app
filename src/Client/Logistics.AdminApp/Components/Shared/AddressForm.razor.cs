using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Components.Shared;

public partial class AddressForm
{
    [Parameter, EditorRequired] 
    public required Address Address { get; set; }
    
    [Parameter]
    public EventCallback<Address> AddressChanged { get; set; }
}
