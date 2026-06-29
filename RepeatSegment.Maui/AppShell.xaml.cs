namespace RepeatSegment.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void OnMenuClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Pages.MenuPage());
    }
}
