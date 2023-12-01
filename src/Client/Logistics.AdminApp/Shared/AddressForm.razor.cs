using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.Shared;

public partial class AddressForm
{
    [Parameter] 
    public AddressDto Address { get; set; } = AddressDto.Empty();
    
    [Parameter]
    public EventCallback<AddressDto> AddressChanged { get; set; }
}
