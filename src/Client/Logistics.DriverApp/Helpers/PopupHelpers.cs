namespace Logistics.DriverApp.Helpers;

public static class PopupHelpers
{
    public static Task ShowErrorAsync(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return Task.CompletedTask;
        
        return Shell.Current.DisplayAlert("Error", errorMessage, "OK");
    }

    public static Task ShowSuccessAsync(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return Task.CompletedTask;
        
        return Shell.Current.DisplayAlert("Success", message, "OK");
    }
}
