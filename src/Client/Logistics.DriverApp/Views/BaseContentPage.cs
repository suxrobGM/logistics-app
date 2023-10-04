using CommunityToolkit.Maui.Behaviors;

namespace Logistics.DriverApp.Views;

public abstract class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
    protected BaseContentPage()
    {
        ViewModel = App.Current.GetRequiredService<TViewModel>();
        BindingContext = ViewModel;
        Behaviors.Add(new EventToCommandBehavior
        {
            EventName = "Appearing",
            Command = (BindingContext as TViewModel)?.InitializeCommand
        });
        Behaviors.Add(new EventToCommandBehavior
        {
            EventName = "Disappearing",
            Command = (BindingContext as TViewModel)?.DisappearingCommand
        });
    }
    
    public TViewModel ViewModel { get; }
}
