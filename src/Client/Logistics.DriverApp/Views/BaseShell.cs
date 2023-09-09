using CommunityToolkit.Maui.Behaviors;

namespace Logistics.DriverApp.Views;

public abstract class BaseShell<TViewModel> : Shell where TViewModel : BaseViewModel
{
    protected BaseShell()
    {
        ViewModel = App.Current.GetRequiredService<TViewModel>();
        BindingContext = ViewModel;
        Behaviors.Add(new EventToCommandBehavior
        {
            EventName = "Appearing",
            Command = (BindingContext as TViewModel)?.InitializeCommand
        });
    }
    
    public TViewModel ViewModel { get; }
}
