namespace Logistics.DriverApp.Controls;

public partial class Card : Frame
{
    public Card()
    {
        BackgroundColor = new Color(245, 245, 245);
        // BorderColor = Colors.Gray;
        // CornerRadius = 10;
        Padding = 10;
        HasShadow = true;
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Card), string.Empty);
    
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
