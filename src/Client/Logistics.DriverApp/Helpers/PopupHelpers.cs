namespace Logistics.DriverApp.Helpers;

public static class PopupHelpers
{
    public static Task ShowError(string errorMessage)
    {
        return Shell.Current.DisplayAlert("Error", errorMessage, "OK");
    }

    public static Task ShowSuccess(string message)
    {
        return Shell.Current.DisplayAlert("Success", message, "OK");
    }
}
