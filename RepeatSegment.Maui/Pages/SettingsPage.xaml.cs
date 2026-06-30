using RepeatSegment.App;

namespace RepeatSegment.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private ConfigManager _config = new(ConfigDir);
    private static string ConfigDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepeatSegment");
    private static string ConfigIniPath => Path.Combine(ConfigDir, "config.ini");

    public SettingsPage()
    {
        InitializeComponent();
        EnsureConfigFileExists();
        PopulatePickers();
        LoadFromConfig();
    }

    private static void EnsureConfigFileExists()
    {
        Directory.CreateDirectory(ConfigDir);
        if (!File.Exists(ConfigIniPath))
        {
            File.WriteAllText(ConfigIniPath, @"[Settings]
path =
file =
position = 0
counter = 0
segment_duration_sec = 5.0
language =
theme = dark

[Transcription]
providers_enabled = deepgram
assemblyai_api_key = 5d343a133e014d3c866928299bc267f0
deepgram_api_key = 5f8efa436c8b19dc254bf10187621eb3dc988ac5
yandex_translate_api_key =
yandex_translate_folder_id =
translation_provider = google
transcription_language = en
chunk_minutes = 10
playback_latency = 0.32
mp3_bitrate = 64
image_search_provider = yandex
");
        }
    }

    private void PopulatePickers()
    {
        PckUiLang.Items.Add("English (en)");
        PckUiLang.Items.Add("Русский (ru)");
        PckUiLang.Items.Add("Deutsch (de)");
        PckUiLang.Items.Add("Français (fr)");
        PckUiLang.Items.Add("Español (es)");

        PckTransLang.Items.Add("English (en)");
        PckTransLang.Items.Add("Русский (ru)");

        PckMp3Bitrate.Items.Add("64 kbps");
        PckMp3Bitrate.Items.Add("128 kbps");

        PckImageProvider.Items.Add("Google");
        PckImageProvider.Items.Add("Yandex");
    }

    private void LoadFromConfig()
    {
        _config = new ConfigManager(ConfigDir);
        _config.Load();

        if (string.IsNullOrEmpty(_config.DeepgramApiKey))
            _config.DeepgramApiKey = "";
        if (string.IsNullOrEmpty(_config.AssemblyAiApiKey))
            _config.AssemblyAiApiKey = "";
        if (string.IsNullOrEmpty(_config.YandexTranslateApiKey))
            _config.YandexTranslateApiKey = "";
        if (string.IsNullOrEmpty(_config.YandexTranslateFolderId))
            _config.YandexTranslateFolderId = "";

        EntryDeepgramKey.Text = _config.DeepgramApiKey;
        EntryAssemblyAiKey.Text = _config.AssemblyAiApiKey;

        CbDeepgram.IsChecked = _config.ProvidersEnabled.Contains("deepgram");
        CbAssemblyAi.IsChecked = _config.ProvidersEnabled.Contains("assemblyai");

        RbGoogle.IsChecked = _config.TranslationProviderPreference == "google";
        RbYandex.IsChecked = _config.TranslationProviderPreference == "yandex";
        EntryYandexKey.Text = _config.YandexTranslateApiKey;
        EntryYandexFolder.Text = _config.YandexTranslateFolderId;
        PanelYandex.IsVisible = RbYandex.IsChecked;

        PckUiLang.SelectedIndex = _config.Language == "ru" ? 1
            : _config.Language == "de" ? 2
            : _config.Language == "fr" ? 3
            : _config.Language == "es" ? 4 : 0;
        PckTransLang.SelectedIndex = _config.TranscriptionLanguage == "ru" ? 1 : 0;
        EntryChunk.Text = _config.ChunkMinutes.ToString();
        EntryLatency.Text = _config.PlaybackLatency.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        PckMp3Bitrate.SelectedIndex = _config.Mp3BitrateKbps >= 128 ? 1 : 0;
        PckImageProvider.SelectedIndex = _config.ImageSearchProvider == "yandex" ? 1 : 0;
    }

    private void OnTranslationChanged(object? sender, CheckedChangedEventArgs e)
        => PanelYandex.IsVisible = RbYandex.IsChecked;

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        _config.DeepgramApiKey = EntryDeepgramKey.Text ?? "";
        _config.AssemblyAiApiKey = EntryAssemblyAiKey.Text ?? "";

        var providers = new List<string>();
        if (CbDeepgram.IsChecked) providers.Add("deepgram");
        if (CbAssemblyAi.IsChecked) providers.Add("assemblyai");
        if (providers.Count == 0) providers.Add("deepgram");
        _config.ProvidersEnabled = providers;

        _config.TranslationProviderPreference = RbYandex.IsChecked ? "yandex" : "google";
        _config.YandexTranslateApiKey = EntryYandexKey.Text ?? "";
        _config.YandexTranslateFolderId = EntryYandexFolder.Text ?? "";

        string[] langs = { "en", "ru", "de", "fr", "es" };
        int uiIdx = PckUiLang.SelectedIndex;
        _config.Language = (uiIdx >= 0 && uiIdx < langs.Length) ? langs[uiIdx] : "en";
        _config.TranscriptionLanguage = PckTransLang.SelectedIndex == 1 ? "ru" : "en";

        if (int.TryParse(EntryChunk.Text, out int chunk)) _config.ChunkMinutes = Math.Clamp(chunk, 1, 60);
        if (double.TryParse(EntryLatency.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double lat))
            _config.PlaybackLatency = Math.Clamp(lat, 0.0, 2.0);

        _config.Mp3BitrateKbps = PckMp3Bitrate.SelectedIndex == 1 ? 128 : 64;
        _config.ImageSearchProvider = PckImageProvider.SelectedIndex == 1 ? "yandex" : "google";

        _config.Save(_config.Path, _config.FileName, _config.Position, _config.Counter);

        await DisplayAlert("Settings", "Saved successfully!", "OK");
    }
}
