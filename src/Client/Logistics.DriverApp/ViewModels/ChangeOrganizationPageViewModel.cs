using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Logistics.Models;

namespace Logistics.DriverApp.ViewModels;

public class ChangeOrganizationPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly ITenantService _tenantService;

    public ChangeOrganizationPageViewModel(
        IApiClient apiClient, 
        IAuthService authService,
        ITenantService tenantService)
    {
        _authService = authService;
        _apiClient = apiClient;
        _tenantService = tenantService;
        _organizations = Array.Empty<OrganizationDto>();
    }


    #region Bindable properties

    private OrganizationDto? _currentOrganization;
    public OrganizationDto? CurrentOrganization
    {
        get => _currentOrganization;
        set => SetProperty(ref _currentOrganization, value);
    }

    private OrganizationDto? _selectedOrganization;
    public OrganizationDto? SelectedOrganization
    {
        get => _selectedOrganization;
        set
        {
            SetProperty(ref _selectedOrganization, value);

            if (value != null)
            {
                Task.Run(async () => await SaveTenantIdAsync(value, true));
                CurrentOrganization = value;
            }
        }
    }

    private OrganizationDto[] _organizations;
    public OrganizationDto[] Organizations
    {
        get => _organizations;
        set => SetProperty(ref _organizations, value);
    }

    #endregion

    
    protected override async Task OnInitializedAsync()
    {
        await FetchUserOrganizationsAsync();

        if (Organizations.Length > 0)
            CurrentOrganization = Organizations.First();
    }

    private async Task FetchUserOrganizationsAsync()
    {
        if (string.IsNullOrEmpty(_authService.User?.Id))
            return;

        IsLoading = true;
        var result = await _apiClient.GetUserOrganizations(_authService.User.Id);

        if (!result.IsSuccess)
        {
            await PopupHelpers.ShowErrorAsync(result.Error);
            IsLoading = false;
            return;
        }
        
        Organizations = result.Data!;

        if (!Organizations.Any())
        {
            await PopupHelpers.ShowErrorAsync("You are not registered with any organization, ask your company to add you to the list of employees as a driver");
        }
        
        IsLoading = false;
    }

    private async Task SaveTenantIdAsync(OrganizationDto organization, bool displaySuccessMessage = false)
    {
        if (CurrentOrganization == organization)
            return;

        _authService.User!.CurrentTenantId = organization.TenantId;
        await _tenantService.SaveTenantIdAsync(organization.TenantId);
        Messenger.Send(new TenantIdChangedMessage(organization.TenantId));

        if (displaySuccessMessage)
        {
            await MainThread.InvokeOnMainThreadAsync(() => 
                PopupHelpers.ShowSuccessAsync($"Successfully switched to the organization: ${organization.DisplayName}"));
        }
    }
}
