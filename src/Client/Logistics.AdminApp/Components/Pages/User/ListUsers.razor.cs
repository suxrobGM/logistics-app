using Logistics.AdminApp.Extensions;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Logistics.AdminApp.Components.Pages.User;

public partial class ListUsers : PageBase
{
    private IEnumerable<UserDto>? _users;
    private int _totalRecords = 10;
    
    
    #region Injectable services

    [Inject] 
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    
    private async void LoadData(LoadDataArgs e)
    {
        var orderBy = e.GetOrderBy();
        var page = e.GetPageNumber();
        var pageSize = e.GetPageSize();
        var pagedData = await CallApiAsync(api => api.GetUsersAsync(new SearchableQuery(null, orderBy, page, pageSize)));
        _users = pagedData?.Items;
        _totalRecords = pagedData?.TotalItems ?? 0;
        StateHasChanged();
    }
}
