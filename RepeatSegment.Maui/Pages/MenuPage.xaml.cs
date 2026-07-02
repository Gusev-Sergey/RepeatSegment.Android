using RepeatSegment.App;
using RepeatSegment.Maui.Services;

namespace RepeatSegment.Maui.Pages;

public partial class MenuPage : ContentPage
{
    private bool _fileExpanded, _segDurExpanded, _transExpanded;

    public MenuPage()
    {
        InitializeComponent();
        RecentManager.Changed += () => Dispatcher.Dispatch(BuildRecentMenu);
    }

    protected override bool OnBackButtonPressed()
    {
        _ = Shell.Current.GoToAsync("//player");
        return true;
    }

    // ── Navigation ─────────────────────────────────────────────────

    private async void OnPlayerTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//player");

    private async void OnSettingsTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//settings");

    // ── File ───────────────────────────────────────────────────────

    private void OnToggleFile(object? sender, EventArgs e)
    {
        _fileExpanded = !_fileExpanded;
        SecFile.IsVisible = _fileExpanded;
        LblFileArrow.Text = _fileExpanded ? "▾" : "▸";
        if (_fileExpanded) BuildRecentMenu();
    }

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//player");
        if (Shell.Current.CurrentPage is PlayerPage player)
            player.TriggerLoad();
    }

    // ── Recent files (uses static RecentManager) ───────────────────

    private void BuildRecentMenu()
    {
        RecentList.Children.Clear();
        var files = RecentManager.Files;
        if (files.Count == 0)
        {
            LblNoRecent.IsVisible = true;
            return;
        }
        LblNoRecent.IsVisible = false;

        foreach (var fp in files)
        {
            string name = Path.GetFileName(fp);
            var lbl = new Label
            {
                Text = $"  📄  {name}",
                FontSize = 13,
                TextColor = Color.FromArgb("#999"),
                Padding = new Thickness(16, 8),
                BackgroundColor = Colors.Transparent
            };
            var captured = fp;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await Shell.Current.GoToAsync("//player");
                if (Shell.Current.CurrentPage is PlayerPage player)
                    player.LoadFile(captured);
            };
            lbl.GestureRecognizers.Add(tap);
            RecentList.Children.Add(lbl);
        }
    }

    // ── Segment Duration ──────────────────────────────────────────

    private void OnToggleSegDuration(object? sender, EventArgs e)
    {
        _segDurExpanded = !_segDurExpanded;
        SecSegDur.IsVisible = _segDurExpanded;
        LblSegDurArrow.Text = _segDurExpanded ? "▾" : "▸";
    }

    private void SetDuration(double sec)
    {
        NavigateAndDo(player => player.UpdateSegmentDuration(sec));
        // Update checkmark
        SetSegCheckmark(sec);
        _segDurExpanded = false;
        SecSegDur.IsVisible = false;
        LblSegDurArrow.Text = "▸";
    }

    private void SetSegCheckmark(double sec)
    {
        // Reset all to plain
        void Reset(Label lbl, string text) { lbl.Text = text; lbl.TextColor = Color.FromArgb("#CCC"); }
        Reset(LblSeg2, "  2 sec");
        Reset(LblSeg5, "  5 sec");
        Reset(LblSeg10, "  10 sec");
        Reset(LblSeg20, "  20 sec");

        // Check the selected one
        (Label? target, string label) = sec switch
        {
            2 => (LblSeg2, "✓  2 sec"),
            5 => (LblSeg5, "✓  5 sec"),
            10 => (LblSeg10, "✓  10 sec"),
            20 => (LblSeg20, "✓  20 sec"),
            _ => (null, "")
        };
        if (target != null)
        {
            target.Text = label;
            target.TextColor = Color.FromArgb("#00B4A6");
        }
    }

    private void OnSeg2Clicked(object? s, EventArgs e) => SetDuration(2);
    private void OnSeg5Clicked(object? s, EventArgs e) => SetDuration(5);
    private void OnSeg10Clicked(object? s, EventArgs e) => SetDuration(10);
    private void OnSeg20Clicked(object? s, EventArgs e) => SetDuration(20);

    private async void OnSegCustomClicked(object? s, EventArgs e)
    {
        await Shell.Current.GoToAsync("//player");
        if (Shell.Current.CurrentPage is PlayerPage player)
            player.OnSegmentDurationCustom();
        _segDurExpanded = false;
        SecSegDur.IsVisible = false;
        LblSegDurArrow.Text = "▸";
    }

    // ── Transcription ─────────────────────────────────────────────

    private void OnToggleTranscription(object? sender, EventArgs e)
    {
        _transExpanded = !_transExpanded;
        SecTrans.IsVisible = _transExpanded;
        LblTransArrow.Text = _transExpanded ? "▾" : "▸";
    }

    private void OnTranscribeCache(object? s, EventArgs e)
    {
        NavigateAndDo(player => player.OnTranscribeCache());
        _transExpanded = false;
        SecTrans.IsVisible = false;
        LblTransArrow.Text = "▸";
    }

    private void OnTranscribeApi(object? s, EventArgs e)
    {
        NavigateAndDo(player => player.OnTranscribeApi());
        _transExpanded = false;
        SecTrans.IsVisible = false;
        LblTransArrow.Text = "▸";
    }

    // ── About ──────────────────────────────────────────────────────

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("RepeatSegment",
            "Smart audio segment repeater\n" +
            "Auto-split by pauses · transcribe\n" +
            "translate in-place · export to Anki\n\n" +
            "v1.0.0 MAUI · AstrorumArbor © 2026",
            "OK");
    }

    // ── Helpers ────────────────────────────────────────────────────

    private async void NavigateAndDo(Action<PlayerPage> action)
    {
        await Shell.Current.GoToAsync("//player");
        if (Shell.Current.CurrentPage is PlayerPage player)
            action(player);
    }
}
