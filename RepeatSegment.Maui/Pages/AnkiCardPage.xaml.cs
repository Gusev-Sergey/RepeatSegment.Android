using System.Text.Json;
using Android.Media;
using RepeatSegment.App;
using RepeatSegment.Maui.Services;
using AndroidUri = Android.Net.Uri;

namespace RepeatSegment.Maui.Pages;

[QueryProperty(nameof(SelectedWord), "selectedWord")]
[QueryProperty(nameof(Context), "context")]
[QueryProperty(nameof(WordStartSec), "wordStart")]
[QueryProperty(nameof(WordEndSec), "wordEnd")]
[QueryProperty(nameof(RuTranslation), "ruTranslation")]
[QueryProperty(nameof(MatchedWord), "matchedWord")]
public partial class AnkiCardPage : ContentPage
{
    public string SelectedWord { get; set; } = "";
    public string Context { get; set; } = "";
    public string WordStartSec { get; set; } = "0";
    public string WordEndSec { get; set; } = "0";
    public string RuTranslation { get; set; } = "";
    public string MatchedWord { get; set; } = "";

    private double _wordStart, _wordEnd;
    private AudioEngine? _audio;
    private TranslationProvider? _translationProvider;
    private TtsProvider? _ttsProvider;
    private List<WordTiming> _wordTimings = new();
    private ConfigManager _config = new(ConfigDir);

    private string? _savedImagePath;
    private string? _savedSentenceMp3Path;
    private string? _deepgramMp3Path;
    private string? _googleMp3Path;
    private string? _savedRecPath;
    private string _selectedTtsProvider = "google";
    private string _imageSearchProvider = "google";
    private double _sentenceStart, _sentenceEnd;

    private static string ConfigDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepeatSegment");

    public static AudioEngine? PendingAudio { get; set; }
    public static TranslationProvider? PendingTranslation { get; set; }
    public static List<WordTiming>? PendingWordTimings { get; set; }

    public AnkiCardPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _audio = PendingAudio;
        _translationProvider = PendingTranslation;
        _wordTimings = PendingWordTimings ?? new List<WordTiming>();
        PendingAudio = null;
        PendingTranslation = null;
        PendingWordTimings = null;

        if (!double.TryParse(WordStartSec, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _wordStart)) _wordStart = 0;
        if (!double.TryParse(WordEndSec, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _wordEnd)) _wordEnd = 0;

        _config.Load();
        _imageSearchProvider = _config.ImageSearchProvider == "yandex" ? "yandex" : "google";

        _ttsProvider = new TtsProvider(_config.DeepgramApiKey);
        if (!_ttsProvider.HasDeepgram)
        {
            RbDeepgramTts.IsEnabled = false;
            LblDeepgramAudio.Text = "(no key)";
        }

        TxtEn.Text = SelectedWord;
        TxtRu.Text = RuTranslation;
        TxtContext.Text = Context;

        LoadDecks();
        _ = LookupIpaAsync();

        if (_audio != null)
        {
            if (FindSentenceBounds())
            {
                _savedSentenceMp3Path = _audio.SaveSnippetMp3(_sentenceStart, _sentenceEnd);
                LblSentenceAudio.Text = $"sentence {_sentenceStart:F1}s–{_sentenceEnd:F1}s ✓";
            }
            else
            {
                LblSentenceAudio.Text = "(tap ▶ to extract)";
            }
        }
    }

    // ── IPA lookup ──

    private async Task LookupIpaAsync()
    {
        string fullPhrase = MatchedWord.Length > 0 ? MatchedWord : SelectedWord;
        if (string.IsNullOrWhiteSpace(fullPhrase)) return;

        string[] wordsToTry = fullPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (wordsToTry.Length == 0) return;

        var ipaParts = new List<string>();

        foreach (string w in wordsToTry)
        {
            string word = w.Trim();
            if (word.Length < 2) { ipaParts.Add(word); continue; }
            string? ipa = null;

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{Uri.EscapeDataString(word.ToLower())}";
                string json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var entry = root[0];
                    if (entry.TryGetProperty("phonetic", out var ph) && !string.IsNullOrWhiteSpace(ph.GetString()))
                        ipa = ph.GetString();
                    else if (entry.TryGetProperty("phonetics", out var phArr) && phArr.ValueKind == JsonValueKind.Array && phArr.GetArrayLength() > 0)
                        for (int i = 0; i < phArr.GetArrayLength(); i++)
                            if (phArr[i].TryGetProperty("text", out var pt) && !string.IsNullOrWhiteSpace(pt.GetString()))
                            { ipa = pt.GetString(); break; }
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(ipa))
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    string url = $"https://en.wiktionary.org/api/rest_v1/page/mobile-sections/{Uri.EscapeDataString(word.ToLower())}";
                    string json = await http.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("lead", out var lead))
                    {
                        string leadText = lead.TryGetProperty("sections", out var sections)
                            ? string.Join(" ", sections.EnumerateArray().Select(s => s.TryGetProperty("text", out var t) ? t.GetString() ?? "" : ""))
                            : "";
                        var match = System.Text.RegularExpressions.Regex.Match(leadText, @"/[^/]+/");
                        if (match.Success && match.Value.Length > 4) ipa = match.Value.Trim('/');
                    }
                }
                catch { }
            }
            ipaParts.Add(ipa ?? word);
        }

        string result = string.Join(" ", ipaParts);
        MainThread.BeginInvokeOnMainThread(() => TxtTranscription.Text = result);
    }

    private void LoadDecks()
    {
        PckDeck.Items.Clear();
        var decks = AnkiExportManager.ListDecks();
        foreach (var d in decks) PckDeck.Items.Add(d);
        if (PckDeck.Items.Count > 0)
        {
            string? last = AnkiExportManager.LastDeck;
            if (last != null)
            {
                int idx = decks.ToList().IndexOf(last);
                PckDeck.SelectedIndex = idx >= 0 ? idx : 0;
            }
            else PckDeck.SelectedIndex = 0;
        }
        else PckDeck.Items.Add("(no decks — create new)");
        if (PckDeck.SelectedIndex < 0 && PckDeck.Items.Count > 0)
            PckDeck.SelectedIndex = 0;
    }

    private async void OnNewDeckClicked(object? s, EventArgs e)
    {
        string? name = await DisplayPromptAsync("New Deck", "Deck name:", "OK", "Cancel", placeholder: "My Deck");
        if (string.IsNullOrWhiteSpace(name)) return;
        PckDeck.Items.Add(name);
        PckDeck.SelectedItem = name;
    }

    private void OnRefreshDecksClicked(object? s, EventArgs e)
    {
        LoadDecks();
        LblStatus.Text = "Decks refreshed";
    }

    /// <summary>Public Downloads folder for decks.</summary>
    private static string PublicDecksDir()
    {
        string dDir = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads)!.AbsolutePath;
        string rsDir = Path.Combine(dDir, "RepeatSegment");
        System.IO.Directory.CreateDirectory(rsDir);
        return rsDir;
    }

    private void OnOpenDecksClicked(object? s, EventArgs e)
    {
        string decksDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RepeatSegment", "decks");
        Directory.CreateDirectory(decksDir);

        // Copy .apkg files to public Downloads/RepeatSegment/
        string rsDir = PublicDecksDir();
        foreach (var apkg in System.IO.Directory.GetFiles(decksDir, "*.apkg"))
        {
            string dest = Path.Combine(rsDir, Path.GetFileName(apkg));
            System.IO.File.Copy(apkg, dest, overwrite: true);
        }

        // Create a dummy .txt if folder is empty
        if (System.IO.Directory.GetFiles(rsDir, "*").Length == 0)
            System.IO.File.WriteAllText(Path.Combine(rsDir, "decks_here.txt"),
                "Anki decks folder. Open .apkg files with AnkiDroid.");

        // Two-step approach: content:// URI (System DocumentsUI) → fallback DownloadManager
        string folderName = "RepeatSegment";
        try
        {
            string uriString = $"content://com.android.externalstorage.documents/document/primary%3ADownload%2F{folderName}";
            var uri = Android.Net.Uri.Parse(uriString);
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
            intent.SetDataAndType(uri, "vnd.android.document/directory");
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission |
                             Android.Content.ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }
        catch (Android.Content.ActivityNotFoundException)
        {
            // Fallback: open system Downloads via DownloadManager
            try
            {
                var fbIntent = new Android.Content.Intent(Android.App.DownloadManager.ActionViewDownloads);
                fbIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(fbIntent);
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Clipboard.Default.SetTextAsync(rsDir);
                    await DisplayAlert("Decks folder",
                        "Folder: Download ▸ RepeatSegment\n\nPath copied to clipboard:\n" + rsDir, "OK");
                });
            }
        }
    }

    private void OnPreviewSentenceClicked(object? s, EventArgs e)
    {
        try
        {
            if (_audio == null) { LblStatus.Text = "No audio loaded"; return; }
            if (!FindSentenceBounds())
            {
                _sentenceStart = _wordStart;
                _sentenceEnd = _wordEnd;
            }
            _savedSentenceMp3Path = _audio.SaveSnippetMp3(_sentenceStart, _sentenceEnd);
            LblSentenceAudio.Text = $"sentence {_sentenceStart:F1}s–{_sentenceEnd:F1}s ✓";
            LblStatus.Text = "Sentence extracted ✓";
            PlayAudioFile(_savedSentenceMp3Path);
        }
        catch (Exception ex) { LblStatus.Text = $"Sentence: {ex.Message}"; }
    }

    private bool FindSentenceBounds()
    {
        if (_wordTimings.Count == 0) return false;

        int idx = 0;
        double bestDist = double.MaxValue;
        for (int i = 0; i < _wordTimings.Count; i++)
        {
            double dist = Math.Abs(_wordTimings[i].Start - _wordStart);
            if (dist < bestDist) { bestDist = dist; idx = i; }
        }

        int left = idx;
        while (left > 0)
        {
            string w = _wordTimings[left - 1].Word.Trim();
            if (w.EndsWith(".") || w.EndsWith("!") || w.EndsWith("?")) break;
            left--;
        }

        int right = idx;
        while (right < _wordTimings.Count)
        {
            string w = _wordTimings[right].Word.Trim();
            if (w.EndsWith(".") || w.EndsWith("!") || w.EndsWith("?"))
            {
                _sentenceStart = _wordTimings[left].Start;
                _sentenceEnd = _wordTimings[right].End + 0.12;
                return true;
            }
            right++;
        }

        if (right > left)
        {
            _sentenceStart = _wordTimings[left].Start;
            _sentenceEnd = _wordTimings[right - 1].End + 0.12;
            return true;
        }

        return false;
    }

    private void OnTtsProviderChanged(object? s, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        if (s == RbGoogleTts) _selectedTtsProvider = "google";
        else if (s == RbDeepgramTts) _selectedTtsProvider = "deepgram";
        else if (s == RbRecord) _selectedTtsProvider = "record";
    }

    private async void OnPreviewGoogleClicked(object? s, EventArgs e)
    {
        BtnPreviewGoogle.IsEnabled = false;
        LblStatus.Text = "Downloading Google TTS...";
        try
        {
            if (_ttsProvider == null) { LblStatus.Text = "TTS not available"; return; }
            string? path = await _ttsProvider.DownloadGoogleTtsToFile(SelectedWord);
            if (path != null)
            {
                _googleMp3Path = path;
                LblGoogleAudio.Text = $"Google: {SelectedWord} ✓";
                PlayAudioFile(path);
                LblStatus.Text = "Google TTS ready ✓";
            }
            else LblStatus.Text = "Google TTS failed";
        }
        catch (Exception ex) { LblStatus.Text = $"Google: {ex.Message}"; }
        finally { BtnPreviewGoogle.IsEnabled = true; }
    }

    private async void OnPreviewDeepgramClicked(object? s, EventArgs e)
    {
        BtnPreviewDeepgram.IsEnabled = false;
        LblStatus.Text = "Downloading Deepgram TTS...";
        try
        {
            if (_ttsProvider == null || !_ttsProvider.HasDeepgram)
            { LblStatus.Text = "Deepgram not available (no API key)"; return; }
            string? path = await _ttsProvider.DownloadDeepgramTtsToFile(SelectedWord);
            if (path != null)
            {
                _deepgramMp3Path = path;
                LblDeepgramAudio.Text = $"Deepgram: {SelectedWord} ✓";
                PlayAudioFile(path);
                LblStatus.Text = "Deepgram TTS ready ✓";
            }
            else LblStatus.Text = "Deepgram TTS failed";
        }
        catch (Exception ex) { LblStatus.Text = $"Deepgram: {ex.Message}"; }
        finally { BtnPreviewDeepgram.IsEnabled = true; }
    }

    private void PlayAudioFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            _audio?.Stop();
            var snippetAudio = new AudioEngine();
            snippetAudio.Load(path);
            snippetAudio.Play();
        }
        catch { }
    }

    private MediaRecorder? _recorder;
    private bool _isRecording;

    private async void OnRecordClicked(object? s, EventArgs e)
    {
#if ANDROID
        if (_isRecording)
        {
            StopRecording();
            return;
        }

        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            LblStatus.Text = "Microphone permission denied";
            return;
        }

        try
        {
            string mediaDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RepeatSegment", "decks", "media");
            Directory.CreateDirectory(mediaDir);
            _savedRecPath = Path.Combine(mediaDir, $"rec_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.mp4");

            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.Mpeg4);
            _recorder.SetAudioEncoder(AudioEncoder.Aac);
            _recorder.SetAudioSamplingRate(44100);
            _recorder.SetAudioEncodingBitRate(96000);
            _recorder.SetOutputFile(_savedRecPath);
            _recorder.Prepare();
            _recorder.Start();

            _isRecording = true;
            BtnRecordAudio.Text = "⏹ Stop";
            BtnRecordAudio.BackgroundColor = Color.FromArgb("#E04040");
            LblRecAudio.Text = "Recording...";
            LblStatus.Text = "Recording... tap Stop when done";
        }
        catch (Exception ex)
        {
            LblStatus.Text = $"Record: {ex.Message}";
            _recorder?.Release();
            _recorder = null;
            _savedRecPath = null;
        }
#else
        LblStatus.Text = "Recording only on Android";
#endif
    }

    private void StopRecording()
    {
        try { _recorder?.Stop(); _recorder?.Release(); } catch { }
        _recorder = null;
        _isRecording = false;
        BtnRecordAudio.Text = "🎤 Rec";
        BtnRecordAudio.BackgroundColor = Color.FromArgb("#444");
        LblRecAudio.Text = $"Recorded ✓";
        LblStatus.Text = "Recording saved ✓";
    }

    private void OnPlayRecordClicked(object? s, EventArgs e)
    {
        if (string.IsNullOrEmpty(_savedRecPath) || !File.Exists(_savedRecPath))
        { LblStatus.Text = "No recording to play"; return; }
        PlayAudioFile(_savedRecPath);
        LblStatus.Text = "Playing recording...";
    }

    private async void OnSearchImagesClicked(object? s, EventArgs e)
    {
        string query = MatchedWord.Length > 0 ? MatchedWord : SelectedWord;
        var searchPage = new ImageSearchPage(query, _imageSearchProvider);
        await Navigation.PushModalAsync(new NavigationPage(searchPage) { BarBackgroundColor = Color.FromArgb("#1E1E1E"), BarTextColor = Color.FromArgb("#E0E0E0") });
        string? imagePath = await searchPage.Result;

        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            _savedImagePath = imagePath;
            ImgPreview.Source = ImageSource.FromFile(imagePath);
            ImgPreview.HeightRequest = 120;
            LblStatus.Text = "Picture selected ✓";
        }
    }

    private async void OnCreateClicked(object? s, EventArgs e)
    {
        string deckName = PckDeck.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(deckName) || deckName.StartsWith("("))
        { LblStatus.Text = "Select or create a deck first"; return; }

        BtnCreate.IsEnabled = false;
        try
        {
            var mgr = new AnkiExportManager(deckName);

            string imgId = "";
            if (!string.IsNullOrEmpty(_savedImagePath) && File.Exists(_savedImagePath))
                try { imgId = mgr.AddMedia(_savedImagePath); } catch { }

            if (string.IsNullOrEmpty(_savedSentenceMp3Path) || !File.Exists(_savedSentenceMp3Path))
            {
                if (_audio != null && FindSentenceBounds())
                    _savedSentenceMp3Path = _audio.SaveSnippetMp3(_sentenceStart, _sentenceEnd);
                else if (_audio != null)
                    _savedSentenceMp3Path = _audio.SaveSnippetMp3(_wordStart, _wordEnd);
            }
            string sentenceAudId = "";
            if (!string.IsNullOrEmpty(_savedSentenceMp3Path) && File.Exists(_savedSentenceMp3Path))
                try { sentenceAudId = mgr.AddMedia(_savedSentenceMp3Path); } catch { }

            string? chosenTtsPath = _selectedTtsProvider == "deepgram" ? _deepgramMp3Path
                : _selectedTtsProvider == "record" ? _savedRecPath : _googleMp3Path;
            if (string.IsNullOrEmpty(chosenTtsPath) || !File.Exists(chosenTtsPath))
            {
                if (_ttsProvider != null)
                {
                    LblStatus.Text = $"Downloading {_selectedTtsProvider} TTS...";
                    try
                    {
                        if (_selectedTtsProvider == "deepgram")
                            chosenTtsPath = await _ttsProvider.DownloadDeepgramTtsToFile(SelectedWord);
                        else if (_selectedTtsProvider == "google")
                            chosenTtsPath = await _ttsProvider.DownloadGoogleTtsToFile(SelectedWord);
                        if (_selectedTtsProvider == "deepgram") _deepgramMp3Path = chosenTtsPath;
                        else _googleMp3Path = chosenTtsPath;
                    }
                    catch { chosenTtsPath = null; }
                }
            }
            string ttsAudId = "";
            if (!string.IsNullOrEmpty(chosenTtsPath) && File.Exists(chosenTtsPath))
                try { ttsAudId = mgr.AddMedia(chosenTtsPath); } catch { }

            mgr.AddNote(TxtEn.Text.Trim(), TxtTranscription.Text.Trim(), TxtRu.Text.Trim(),
                imgId, sentenceAudId, ttsAudId, TxtContext.Text.Trim());
            string apkgPath = mgr.Finalize();

            // Also copy .apkg to public Downloads/RepeatSegment/
            string publicDir = PublicDecksDir();
            string publicApkg = Path.Combine(publicDir, Path.GetFileName(apkgPath));
            System.IO.File.Copy(apkgPath, publicApkg, overwrite: true);

            LblStatus.Text = "Cards created ✓";
            await DisplayAlert("Success",
                $"Cards added to deck!\n\nDeck saved to:\n" + publicDir,
                "OK");
        }
        catch (Exception ex)
        {
            string detail = ex.InnerException?.Message ?? ex.Message;
            LblStatus.Text = $"Error: {detail}";
            await DisplayAlert("Error", $"Failed: {detail}", "OK");
        }
        finally { BtnCreate.IsEnabled = true; }
    }

    private async void OnCancelClicked(object? s, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
        return true;
    }
}
