using Logistics.WebApi.Client.Exceptions;

namespace Logistics.OfficeApp.ViewModels.Pages.User;

public class EditUserViewModel : PageViewModelBase
{
    public EditUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        User = new EmployeeDto();
    }

    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public Toast? Toast { get; set; }


    #region Binding properties

    public EmployeeDto User { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);
    public string Error { get; set; } = string.Empty;

    #endregion


    public override async Task OnInitializedAsync()
    {
        Error = string.Empty;

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
        Error = string.Empty;

        try
        {
            if (EditMode)
            {
                await apiClient.UpdateEmployeeAsync(User!);
                Toast?.Show("User has been saved successfully.", "Notification");
            }
            IsBusy = false;
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
            IsBusy = false;
        }
        
    }

    private Task<EmployeeDto> FetchUserAsync(string id)
    {
        return Task.Run(async () =>
        {
            return await apiClient.GetEmployeeAsync(id);
        });
    }
}
