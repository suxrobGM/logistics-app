namespace Logistics.DriverApp.Controls;

public partial class HyperlinkSpan
{
	public HyperlinkSpan()
	{
		InitializeComponent();

        TextDecorations = TextDecorations.Underline;
        TextColor = Colors.Blue;
        GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Launcher.OpenAsync(Url))
        });
    }

    public static readonly BindableProperty UrlProperty =
        BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkSpan), null);

    public string Url
    {
        get => (string)GetValue(UrlProperty); 
        set => SetValue(UrlProperty, value);
    }
}