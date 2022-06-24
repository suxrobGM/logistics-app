namespace Logistics.OfficeApp.ViewModels.Pages.Employee;

public class EditEmployeeViewModel : PageViewModelBase
{
    public EditEmployeeViewModel(IApiClient apiClient)
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
        
        if (EditMode)
        {
            var result = await CallApi(i => i.GetEmployeeAsync(Id!));

            if (!result.Success)
                return;
            
            Employee = result.Value!;
        }
    }

    public async Task UpdateAsync()
    {
        Error = string.Empty;

        if (EditMode)
        {
            var result = await CallApi(i => i.UpdateEmployeeAsync(Employee));

            if (!result.Success)
                return;

            Toast?.Show("User has been saved successfully.", "Notification");
        }
    }
}
