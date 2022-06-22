namespace Logistics.OfficeApp.ViewModels.Pages.Employee;

public class EditUserViewModel : PageViewModelBase
{
    public EditUserViewModel(IApiClient apiClient)
        : base(apiClient)
    {
        Employee = new EmployeeDto();
    }

    [Parameter]
    public string? Id { get; set; }

    


    #region Binding properties

    public EmployeeDto Employee { get; set; }
    public bool EditMode => !string.IsNullOrEmpty(Id);

    #endregion


    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        try
        {
            if (EditMode)
            {
                IsBusy = true;
                var user = await apiClient.GetEmployeeAsync(Id!);

                Employee = user;
                IsBusy = false;
            }
        }
        catch (ApiException e)
        {
            Error = e.Message;
        }
        finally
        {
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
                await apiClient.UpdateEmployeeAsync(Employee);
                Toast?.Show("User has been saved successfully.", "Notification");
            }
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
