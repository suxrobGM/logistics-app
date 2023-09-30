namespace Logistics.DriverApp.Controls;

public partial class DateRangePicker : ContentView
{
    public DateRangePicker()
    {
        InitializeComponent();
    }
    
    public static readonly BindableProperty StartDateProperty = 
        BindableProperty.Create(nameof(StartDate), typeof(DateTime), typeof(DateRangePicker), DateTime.Today.AddDays(-1), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnStartDateChanged);

    public static readonly BindableProperty EndDateProperty =
        BindableProperty.Create(nameof(EndDate), typeof(DateTime), typeof(DateRangePicker), DateTime.Today, defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnEndDateChanged);

    public DateTime StartDate
    {
        get => (DateTime)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public DateTime EndDate
    {
        get => (DateTime)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    private static void OnStartDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DateRangePicker)bindable;
        var newStartDate = (DateTime)newValue;

        // If the new StartDate is greater than EndDate, revert StartDate to oldValue
        if (newStartDate > control.EndDate)
        {
            control.StartDate = (DateTime)oldValue;
        }
    }

    private static void OnEndDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DateRangePicker)bindable;
        var newEndDate = (DateTime)newValue;

        // If the new EndDate is less than StartDate, revert EndDate to oldValue
        if (newEndDate < control.StartDate)
        {
            control.EndDate = (DateTime)oldValue;
        }
    }
}
