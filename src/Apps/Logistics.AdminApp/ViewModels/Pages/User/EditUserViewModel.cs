using Logistics.AdminApp.Shared.Components;
using Microsoft.AspNetCore.Components;

namespace Logistics.AdminApp.ViewModels.Pages.User;

public class EditUserViewModel : PageViewModelBase
{

    public EditUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        User = new UserDto();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }


    #region Binding properties

    public UserDto User { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    #endregion


    public override async Task OnInitializedAsync()
    {
        if (EditMode)
        {
            IsBusy = true;
            var user = await FetchUserAsync(Id!);

            if (user != null)
                User = user;
            
            IsBusy = false;
        }
    }

    public async Task UpdateAsync()
    {
        IsBusy = true;
        if (EditMode)
        {
            await Task.Run(async () => await apiClient.UpdateUserAsync(User!));
            Toast?.Show("User has been saved successfully.", "Notification");
        }
        IsBusy = false;
    }

    private Task<UserDto?> FetchUserAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetUserAsync(id);
        });
    }
}
