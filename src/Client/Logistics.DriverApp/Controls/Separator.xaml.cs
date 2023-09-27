namespace Logistics.DriverApp.Controls;

public partial class Separator : ContentView
{
    public Separator()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(Separator), Colors.Gray);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
}
