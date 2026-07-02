namespace RepeatSegment.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("ankiCard", typeof(Pages.AnkiCardPage));
    }

    private async void OnMenuClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Pages.MenuPage());
    }
}
