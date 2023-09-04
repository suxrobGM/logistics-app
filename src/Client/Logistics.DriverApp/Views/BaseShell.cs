using CommunityToolkit.Maui.Behaviors;

namespace Logistics.DriverApp.Views;

public abstract class BaseShell<TViewModel> : Shell where TViewModel : BaseViewModel
{
    protected BaseShell()
    {
        BindingContext = App.Current.Services.GetService<TViewModel>();
        Behaviors.Add(new EventToCommandBehavior
        {
            EventName = "Appearing",
            Command = (BindingContext as TViewModel)?.InitializeCommand
        });
    }
}