using CommunityToolkit.Maui.Behaviors;

namespace Logistics.DriverApp.Views;

public abstract class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
    protected BaseContentPage()
    {
        BindingContext = App.Current.Services.GetService<TViewModel>();
        Behaviors.Add(new EventToCommandBehavior
        {
            EventName = "Appearing",
            Command = (BindingContext as TViewModel)?.InitializeCommand
        });
    }
}