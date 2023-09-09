namespace Logistics.BlazorComponents;

public partial class Alert : ComponentBase
{
    [Parameter]
    public AlertType Type { get; set; }
    
    [Parameter]
    public string? Message { get; set; }

    [Parameter] 
    public bool Dismissible { get; set; } = true;
    
    public enum AlertType
    {
        Success,
        Primary,
        Warning,
        Error
    }
    
    public void Show(string message, AlertType type = AlertType.Success)
    {
        Type = type;
        Message = message;
        StateHasChanged();
    }

    private static string GetAlertTypeClass(AlertType alertType)
    {
        return alertType switch
        {
            AlertType.Success => "alert-success",
            AlertType.Primary => "alert-primary",
            AlertType.Warning => "alert-warning",
            AlertType.Error => "alert-danger",
            _ => "alert-success"
        };
    }
}
