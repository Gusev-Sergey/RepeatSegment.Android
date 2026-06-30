using Android.Content;
using Android.Media;
using RepeatSegment.App;
using RepeatSegment.Maui.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace RepeatSegment.Maui.Pages;

public partial class PlayerPage : ContentPage
{
    private AudioEngine _audio = new();
    private readonly SilenceDetector _detector = new();

    private List<(double T1, double T2)> _fragments = new();
    private int _counter;
    private double _t1, _t2;
    private double _positionSeconds, _durationSeconds;
    private double _dt;
    private bool _pop = true, _pte, _playGoMode, _sliderDragInProgress, _btnBlocked, _pausedByCall;
    private long _playStartNanos;
    private Android.OS.Handler? _renderHandler;
    private IDispatcherTimer? _uiTimer;
    private System.Threading.Timer? _wordHighlightTimer;
    private const int RENDER_INTERVAL_MS = 16;
    private const int UI_INTERVAL = 100;
    private const int HIGHLIGHT_INTERVAL = 250;

    private TranscriptionProvider? _transcriptionProvider;
    private ConfigManager _config = new(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "RepeatSegment"));
    private int _lastHlIdx = -1, _lastScrollWordIdx = -1;

    private bool _touchDragging;
    private double _dragStartSec, _dragEndSec;
    private (double T1, double T2)? _userSegment;
    private float _touchW, _touchH;

    private string[]? _transcriptionWords;
    private int[]? _wordCharPositions;
    private int _totalChars;
    private double _prevScrollOffset = -1;
    private PlayerFocusListener? _playerFocusListener;

    private const double WindowWidthSec = 15.0;
    private static readonly double[] Speeds = { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.4, 1.5 };
    private static readonly string[] SpeedLabels = { "0.4×", "0.5×", "0.6×", "0.7×", "0.8×", "0.9×", "1.0×", "1.1×", "1.2×", "1.3×", "1.4×", "1.5×" };
    private bool _firstAppearance = true;

    // Static paints — no allocations per frame
    private static readonly SKPaint _paintBg = new() { Color = new SKColor(0x25, 0x25, 0x25) };
    private static readonly SKPaint _paintCur = new() { Color = new SKColor(0xE0, 0x40, 0x40), StrokeWidth = 2f, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint _paintWave = new() { Color = new SKColor(0x5A, 0x9B, 0xE6), StrokeWidth = 1.2f, Style = SKPaintStyle.Stroke, IsAntialias = true };
    private static readonly SKPaint _paintSegFill = new() { Style = SKPaintStyle.Fill };
    private static readonly SKPaint _paintSegLine = new() { Color = new SKColor(0xFF, 0x8C, 0x00), StrokeWidth = 3f, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint _paintRulerMajor = new() { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 1.2f, Style = SKPaintStyle.Stroke, IsAntialias = true };
    private static readonly SKPaint _paintRulerMinor = new() { Color = new SKColor(0x88, 0x88, 0x88), StrokeWidth = 0.8f, Style = SKPaintStyle.Stroke };
    private static readonly SKPaint _paintRulerText = new() { Color = new SKColor(0xEE, 0xEE, 0xEE), IsAntialias = true };
    private static readonly SKFont _fontRuler = new() { Size = 18 };
    private static readonly SKFont _fontPlaceholder = new() { Size = 19 };
    private static readonly SKPaint _paintPlaceholder = new() { Color = new SKColor(0x00, 0xB4, 0xA6), IsAntialias = true };
    private static readonly SKPathEffect _dashEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0);

    public PlayerPage()
    {
        InitializeComponent();
        foreach (var l in SpeedLabels) PckSpeed.Items.Add(l); PckSpeed.SelectedIndex = 6;
        PlaybackBridge.Cmd += OnPlaybackCmd;
        RequestAudioFocusViaService(); // once per lifetime
    }
    private void OnPlaybackCmd(string c) => Dispatcher.Dispatch(() =>
    {
        switch (c)
        {
            case "focus_loss":
            case "focus_loss_transient":
            case "focus_duck":
                if (!_pop)
                {
                    _pausedByCall = true;
                    _audio?.Pause();
                    StopTm();
                }
                break;
            case "focus_gain":
                if (_pausedByCall)
                {
                    _pausedByCall = false;
                    _audio?.Resume();
                    _dt = _positionSeconds - _t1;
                    _playStartNanos = Java.Lang.JavaSystem.NanoTime();
                    StartTm();
                    UpdateSkinsPlaying();
                }
                break;
            default:
                Btn(c);
                break;
        }
    });

    protected override void OnAppearing() { base.OnAppearing(); if (_firstAppearance) { _firstAppearance = false; var r = Services.RecentManager.Files; if (r.Count > 0 && System.IO.File.Exists(r[0])) LoadFile(r[0]); } }
    private async void OnLoadMenuClicked(object? s, EventArgs e) { var r = Services.RecentManager.Files; var o = new List<string> { "📂 Browse files..." }; foreach (var f in r.Take(5)) o.Add($"📄 {System.IO.Path.GetFileName(f)}"); string? c = await DisplayActionSheet("Load Audio", "Cancel", null, o.ToArray()); if (string.IsNullOrEmpty(c) || c == "Cancel") return; if (c == "📂 Browse files...") await Pick(); else { int i = o.IndexOf(c); if (i > 0 && i <= r.Count) LoadFile(r[i - 1]); } }
    private async Task Pick() { try { var r = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select audio", FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/x-wav" } } }) }); if (r == null) return; LoadFile(r.FullPath); } catch (Exception ex) { LblStatus.Text = ex.Message; } }

    private void OnPlayClicked(object? s, EventArgs e) => Btn("play");
    private void OnPlayGoClicked(object? s, EventArgs e) => Btn("play_go");
    private void OnRepeatClicked(object? s, EventArgs e) => Btn("replay");
    private void OnFirstClicked(object? s, EventArgs e) => Btn("first");
    private void OnPrevClicked(object? s, EventArgs e) => Btn("preplay");
    private void OnNextClicked(object? s, EventArgs e) => Btn("next_play");
    private void OnLastClicked(object? s, EventArgs e) => Btn("last");

    private void Btn(string k)
    {
        if (_btnBlocked || _audio?.Samples == null) return; _btnBlocked = true;
        try
        {
            if (k is "preplay" or "next_play" or "first" or "last") { _sliderDragInProgress = true; try { Skip(k); } finally { _sliderDragInProgress = false; } _btnBlocked = false; return; }
            if (_pop) { PlaybackServiceManager.Start(); _pte = (k == "play"); _playGoMode = (k == "play_go"); if (k == "play") { bool vs = (_counter == 0 && Math.Abs(_positionSeconds - _t1) < 0.05); if (!vs) { if (_counter + 1 < _fragments.Count) _counter++; else _counter = 0; } } Skip(k, keepPte: (k == "play" || k == "replay")); UpdateSkinsPlaying(); _pop = false; StartPlay(); }
            else { PlaybackServiceManager.Stop(); StopPlay(); UpdateSkinsStopped(); _pop = true; _pte = false; _playGoMode = false; if (_positionSeconds > _t2) _positionSeconds = _t2; for (int i = 0; i < _fragments.Count; i++) if (_fragments[i].T1 <= _positionSeconds && _positionSeconds <= _fragments[i].T2) { _counter = i; _t1 = _fragments[i].T1; _t2 = _fragments[i].T2; break; } StopTm(); SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); WaveformCanvas.InvalidateSurface(); LblStatus.Text = "Ready"; }
        }
        finally { _btnBlocked = false; }
    }
    private void Skip(string k, bool keepPte = false) { _audio?.Pause(); StopTm(); if (!keepPte) { _pte = false; _playGoMode = false; } if (_fragments.Count == 0) return; if (k != "replay" && k != "play") { switch (k) { case "preplay": if (_positionSeconds > 0) _counter--; break; case "next_play": _counter++; break; case "play_go": if (_positionSeconds > 0) _counter++; break; case "first": _counter = 0; break; case "last": _counter = _fragments.Count - 1; break; } } _counter = Math.Max(0, Math.Min(_counter, _fragments.Count - 1)); _t1 = _fragments[_counter].T1; _t2 = _fragments[_counter].T2; _positionSeconds = _t1; SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); UpdateSeg(); WaveformCanvas.InvalidateSurface(); }

    private void StartPlay() { StopTm(); StartPlayI(); }
    private void StartPlayI() { StopTm(); var ss = _audio!.GetPlaySamples(_t1, _t2); if (ss == null) return; _audio.Stop(); _audio.PlaySegment(ss); _playStartNanos = Java.Lang.JavaSystem.NanoTime(); _dt = 0; StartTm(); }
    private void StopPlay() { _audio?.Stop(); StopTm(); }
    private void UpdateSkinsPlaying() { BtnPlay.Source = "stop_play"; BtnRepeat.Source = "stop_play"; BtnPlayGo.Source = "stop_play"; }
    private void UpdateSkinsStopped() { BtnPlay.Source = "play"; BtnRepeat.Source = "repeat"; BtnPlayGo.Source = "play_go"; }

    private void StartTm()
    {
        StopTm();
        _renderHandler = new Android.OS.Handler(Android.OS.Looper.MainLooper!);
        ScheduleRender();
        _uiTimer = Dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromMilliseconds(UI_INTERVAL);
        _uiTimer.Tick += UiTick;
        _uiTimer.Start();
        _wordHighlightTimer = new System.Threading.Timer(_ => HlWordBg(), null, HIGHLIGHT_INTERVAL, HIGHLIGHT_INTERVAL);
    }
    private void ScheduleRender()
    {
        if (_renderHandler == null || _pop || _audio == null || _audio.IsDisposed) return;
        _renderHandler.PostDelayed(() =>
        {
            if (_pop || _audio == null || _audio.IsDisposed) return;
            WaveformCanvas.InvalidateSurface();
            ScheduleRender();
        }, RENDER_INTERVAL_MS);
    }
    private void StopTm()
    {
        _renderHandler?.RemoveCallbacks((Java.Lang.IRunnable?)null);
        _renderHandler = null;
        _uiTimer?.Stop();
        _wordHighlightTimer?.Dispose();
        _wordHighlightTimer = null;
        _prevScrollOffset = -1;
    }

    private void UiTick(object? s, EventArgs e)
    {
        if (_audio == null || _audio.IsDisposed || _pop) return;
        try
        {
            double nowPos = _t1 + _dt +
                (Java.Lang.JavaSystem.NanoTime() - _playStartNanos) / 1_000_000_000.0 * _audio.PlaybackSpeed;
            _positionSeconds = nowPos;
            if (_pte && nowPos >= _t2) { if (_counter + 1 < _fragments.Count) { _counter++; _t1 = _fragments[_counter].T1; _t2 = _fragments[_counter].T2; _playStartNanos = Java.Lang.JavaSystem.NanoTime(); _dt = 0; UpdateSeg(); StartPlayI(); return; } StopPlay(); UpdateSkinsStopped(); _pop = true; _positionSeconds = 0; _counter = 0; if (_fragments.Count > 0) { _t1 = _fragments[0].T1; _t2 = _fragments[0].T2; } SliderPosition.Value = 0; LblPosition.Text = Fmt(0); LblStatus.Text = "Playback finished"; return; }
            if (!_pte && nowPos >= _t2) { _positionSeconds = _t2; StopPlay(); UpdateSkinsStopped(); _pop = true; _playGoMode = false; SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); LblStatus.Text = "Segment ended"; return; }
            if (!_sliderDragInProgress) SliderPosition.Value = Math.Min(nowPos, SliderPosition.Maximum);
            LblPosition.Text = Fmt(nowPos);
        }
        catch { }
    }

    private void OnSpeedChanged(object? s, EventArgs e) { int i = PckSpeed.SelectedIndex; if (i >= 0 && i < Speeds.Length) { _audio.PlaybackSpeed = Speeds[i]; if (!_pop && _audio != null) { double p = _positionSeconds; if (!_pte && p >= _t2) p = _t2; StopPlay(); _dt = p - _t1; StartPlayI(); } } }
    private AudioManager? _am; private AudioManager GetAM() { if (_am == null) _am = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager; return _am!; }
    private void OnVolUpClicked(object? s, EventArgs e) => ChVol(+1);
    private void OnVolDownClicked(object? s, EventArgs e) => ChVol(-1);
    private void ChVol(int d) { var a = GetAM(); if (a == null) return; int mx = a.GetStreamMaxVolume(global::Android.Media.Stream.Music), cur = a.GetStreamVolume(global::Android.Media.Stream.Music), nv = Math.Clamp(cur + d, 0, mx); a.SetStreamVolume(global::Android.Media.Stream.Music, nv, VolumeNotificationFlags.PlaySound); _audio.Volume = (float)nv / mx; LblVolume.Text = $"{(int)(_audio.Volume * 100)}%"; }
    private void OnCollapseClicked(object? s, EventArgs e) { LblTranscription.IsVisible = !LblTranscription.IsVisible; BtnCollapse.Text = LblTranscription.IsVisible ? "▲" : "▼"; }
    private void OnSliderValueChanged(object? s, ValueChangedEventArgs e) { if (_sliderDragInProgress) { _positionSeconds = e.NewValue; LblPosition.Text = Fmt(_positionSeconds); } }
    private void OnSliderDragStarted(object? s, EventArgs e) => _sliderDragInProgress = true;
    private void OnSliderDragCompleted(object? s, EventArgs e) { _sliderDragInProgress = false; var sv = SliderPosition.Value; for (int i = 0; i < _fragments.Count; i++) if (_fragments[i].T1 <= sv && sv < _fragments[i].T2) { _counter = i; _t1 = _fragments[i].T1; _t2 = _fragments[i].T2; _positionSeconds = sv; if (!_pop) { StopPlay(); StartPlay(); } WaveformCanvas.InvalidateSurface(); LblPosition.Text = Fmt(_positionSeconds); UpdateSeg(); return; } if (_fragments.Count > 0) { _counter = _fragments.Count - 1; _t1 = _fragments[_counter].T1; _t2 = _fragments[_counter].T2; } _positionSeconds = sv; LblPosition.Text = Fmt(_positionSeconds); WaveformCanvas.InvalidateSurface(); UpdateSeg(); }

    private void OnWaveformTouch(object? s, SKTouchEventArgs e) { if (_audio.SamplesSmall == null) return; float cw = _touchW > 0 ? _touchW : (float)WaveformCanvas.Width; if (cw <= 0) return; var (vl, vr, vw) = W(); if (vw <= 0) return; double sec = vl + (e.Location.X / cw) * vw; if (e.ActionType == SKTouchAction.Pressed) { _touchDragging = true; _dragStartSec = sec; _dragEndSec = sec; e.Handled = true; } else if (e.ActionType == SKTouchAction.Moved && _touchDragging) { _dragEndSec = Math.Clamp(sec, 0, _durationSeconds); WaveformCanvas.InvalidateSurface(); e.Handled = true; } else if ((e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled) && _touchDragging) { _touchDragging = false; double t1 = Math.Min(_dragStartSec, _dragEndSec), t2 = Math.Max(_dragStartSec, _dragEndSec); if (t2 - t1 >= 0.5) { var nf = new List<(double, double)>(); foreach (var f in _fragments) { if (f.T2 <= t1) nf.Add(f); else if (f.T1 >= t2) nf.Add(f); else if (f.T1 < t1 && f.T2 > t2) { if (t1 - f.T1 >= 0.5) nf.Add((f.T1, t1)); if (f.T2 - t2 >= 0.5) nf.Add((t2, f.T2)); } else if (f.T1 < t1) nf.Add((f.T1, t1)); else if (f.T2 > t2) nf.Add((t2, f.T2)); } nf.Add((t1, t2)); nf.Sort((a, b) => a.Item1.CompareTo(b.Item1)); _fragments = nf; _positionSeconds = t1; _t1 = t1; _t2 = t2; _counter = _fragments.FindIndex(f => Math.Abs(f.T1 - t1) < 0.001 && Math.Abs(f.T2 - t2) < 0.001); if (_counter < 0) _counter = 0; _userSegment = (t1, t2); SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); UpdateSeg(); LblStatus.Text = $"Segments: {_fragments.Count}"; } _dragStartSec = 0; _dragEndSec = 0; WaveformCanvas.InvalidateSurface(); e.Handled = true; } }

    private (double l, double r, double w) W() { double d = _durationSeconds > 0 ? _durationSeconds : 1, p = Math.Clamp(_positionSeconds, 0, d), hw = WindowWidthSec / 2, vl = p - hw, vr = p + hw; if (vl < 0) { vr -= vl; vl = 0; } if (vr > d) { vl -= vr - d; vr = d; if (vl < 0) vl = 0; } return (vl, vr, vr - vl); }

    private void OnWaveformPaint(object? s, SKPaintSurfaceEventArgs e)
    {
        var c = e.Surface.Canvas; var info = e.Info; float w = info.Width, h = info.Height; _touchW = w; _touchH = h;
        c.Clear(_paintBg.Color); var small = _audio.SamplesSmall;
        if (small == null || small.Length == 0 || _durationSeconds <= 0) { c.DrawText("Load audiobook to see waveform", w / 2f, h / 2f + 6, SKTextAlign.Center, _fontPlaceholder, _paintPlaceholder); return; }
        double nowPos;
        if (!_pop && _playStartNanos > 0)
            nowPos = _t1 + _dt + (Java.Lang.JavaSystem.NanoTime() - _playStartNanos) / 1_000_000_000.0 * _audio.PlaybackSpeed;
        else
            nowPos = _positionSeconds;
        _positionSeconds = nowPos;
        double d = _durationSeconds > 0 ? _durationSeconds : 1;
        double pc = Math.Clamp(nowPos, 0, d);
        double hw = WindowWidthSec / 2;
        double vl = pc - hw, vr = pc + hw;
        if (vl < 0) { vr -= vl; vl = 0; }
        if (vr > d) { vl -= vr - d; vr = d; if (vl < 0) vl = 0; }
        double vw = vr - vl;
        if (vw <= 0) return;
        float ah = h - 22f; if (ah < 10) ah = h;
        DrawHL(c, w, ah, vl, vw); DrawUser(c, w, ah, vl, vw); DrawPers(c, w, ah, vl, vw); DrawLines(c, w, ah, vl, vw); DrawWave(c, small, w, ah, vl, vw); DrawCur(c, w, ah, nowPos, vl, vw); DrawRuler(c, w, h, vl, vw);
    }

    private void DrawR(SKCanvas c, float w, float h, double t1, double t2, double vl, double vw, SKColor cl) { if (t2 <= vl || t1 >= vl + vw) return; float x1 = (float)((t1 - vl) / vw * w), x2 = (float)((t2 - vl) / vw * w); x1 = Math.Clamp(x1, 0, w); x2 = Math.Clamp(x2, 0, w); if (x2 - x1 >= 1) { _paintSegFill.Color = cl; c.DrawRect(x1, 0, x2 - x1, h, _paintSegFill); } }
    private void DrawUser(SKCanvas c, float w, float h, double vl, double vw) { if (!_touchDragging) return; double t1 = Math.Min(_dragStartSec, _dragEndSec), t2 = Math.Max(_dragStartSec, _dragEndSec); if (t2 - t1 < 0.25) return; DrawR(c, w, h, t1, t2, vl, vw, new SKColor(0x40, 0xE0, 0x40, 0x60)); }
    private void DrawPers(SKCanvas c, float w, float h, double vl, double vw) { if (_userSegment == null) return; var (t1, t2) = _userSegment.Value; if (t2 - t1 < 0.5) return; DrawR(c, w, h, t1, t2, vl, vw, new SKColor(0x40, 0xE0, 0x40, 0x40)); }
    private void DrawHL(SKCanvas c, float w, float h, double vl, double vw) { if (_fragments.Count == 0 || _counter >= _fragments.Count) return; var (t1, t2) = _fragments[_counter]; DrawR(c, w, h, t1, t2, vl, vw, new SKColor(0x40, 0x5A, 0x9B, 0xE6)); }
    private void DrawLines(SKCanvas c, float w, float h, double vl, double vw) { if (_fragments.Count == 0) return; _paintSegLine.PathEffect = _dashEffect; foreach (var (t1, t2) in _fragments) { if (t1 >= vl && t1 <= vl + vw) c.DrawLine((float)((t1 - vl) / vw * w), 0, (float)((t1 - vl) / vw * w), h, _paintSegLine); if (t2 >= vl && t2 <= vl + vw && t2 < _durationSeconds - 0.01) c.DrawLine((float)((t2 - vl) / vw * w), 0, (float)((t2 - vl) / vw * w), h, _paintSegLine); } }
    private void DrawWave(SKCanvas c, float[] small, float w, float h, double vl, double vw) { int sr = _audio.SampleRateSmall > 0 ? _audio.SampleRateSmall : 1000; int si = (int)(vl * sr), ei = (int)((vl + vw) * sr); si = Math.Max(0, si); ei = Math.Min(small.Length, ei); int ct = ei - si; if (ct <= 1) return; float my = h / 2f, sy = h / 2f * 0.85f; var path = new SKPath(); bool ff = true; int st = Math.Max(1, ct / (int)(w * 4)); for (int i = 0; i < ct; i += st) { float v = small[si + i], x = (float)i / ct * w, y = Math.Clamp(my - v * sy, 0, h); if (ff) { path.MoveTo(x, y); ff = false; } else path.LineTo(x, y); } c.DrawPath(path, _paintWave); path.Dispose(); }
    private void DrawCur(SKCanvas c, float w, float h, double pos, double vl, double vw) { float cx = (float)((pos - vl) / vw * w); cx = Math.Clamp(cx, 0, w); c.DrawLine(cx, 0, cx, h, _paintCur); }
    private void DrawRuler(SKCanvas c, float w, float h, double vl, double vw) { float rt = h - 20, rm = h - 10, rb = h - 4; for (double t = Math.Ceiling((vl - 1) / 5.0) * 5.0; t <= vl + vw; t += 5.0) { float x = (float)((t - vl) / vw * w); c.DrawLine(x, rt, x, rb, _paintRulerMajor); c.DrawText($"{(int)(t / 60)}:{(int)(t % 60):D2}", x + 2, h - 1, SKTextAlign.Left, _fontRuler, _paintRulerText); } for (double t = Math.Ceiling(vl); t <= vl + vw; t += 1.0) { if (Math.Abs(t % 5.0) < 0.01) continue; c.DrawLine((float)((t - vl) / vw * w), rt + 6, (float)((t - vl) / vw * w), rm, _paintRulerMinor); } }

    private void UpdateSeg() { LblSegmentInfo.Text = _fragments.Count > 0 && _counter < _fragments.Count ? $"Seg {_counter + 1}/{_fragments.Count}" : ""; }
    private static string Fmt(double s) { if (s < 0) s = 0; var ts = TimeSpan.FromSeconds(s); return ts.Hours > 0 ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}" : $"{ts.Minutes:D2}:{ts.Seconds:D2}"; }

    public void TriggerLoad() => _ = Pick();
    public void UpdateSegmentDuration(double sec) { if (_audio?.Samples == null) return; _detector.Detect(_audio.Samples!, _audio.SampleRate, _durationSeconds, sec); _fragments = _detector.T1T2Array.ToList(); if (_fragments.Count > 0) { _counter = Math.Min(_counter, _fragments.Count - 1); _t1 = _fragments[_counter].T1; _t2 = _fragments[_counter].T2; _positionSeconds = _t1; SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); UpdateSeg(); WaveformCanvas.InvalidateSurface(); LblStatus.Text = $"Segments: {_fragments.Count}"; } }
    public async void OnSegmentDurationCustom() { string? r = await DisplayPromptAsync("Custom", "Seconds (1-300):", "OK", "Cancel", "5", maxLength: 3, keyboard: Keyboard.Numeric); if (double.TryParse(r, out double s) && s >= 1 && s <= 300) UpdateSegmentDuration(s); }
    public async void OnTranscribeCache() { if (_audio?.Samples == null) { LblStatus.Text = "Load audio file first"; return; } _config = new ConfigManager(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "RepeatSegment")); _config.Load(); ApDef(); _transcriptionProvider = new TranscriptionProvider(_config, _audio); _transcriptionProvider.SetSilenceZones(_detector.Silence); _transcriptionProvider.StatusChanged += msg => Dispatcher.Dispatch(() => LblStatus.Text = msg); try { BtnLoad.IsEnabled = false; LoadingOverlay.IsVisible = true; LoadingSpinner.IsRunning = true; LblLoadingStatus.Text = "Loading from cache..."; await Task.Run(() => _transcriptionProvider.Transcribe(_audio.FilePath, false)); int wc = _transcriptionProvider.WordTimings.Count; if (wc > 0) { BuildWL(); LblStatus.Text = $"Cache: {wc} words"; } else LblStatus.Text = "No cache found"; } catch (Exception ex) { LblStatus.Text = ex.Message; } finally { BtnLoad.IsEnabled = true; LoadingOverlay.IsVisible = false; LoadingSpinner.IsRunning = false; } }
    private void ApDef() { if (string.IsNullOrEmpty(_config.DeepgramApiKey)) _config.DeepgramApiKey = "5f8efa436c8b19dc254bf10187621eb3dc988ac5"; if (string.IsNullOrEmpty(_config.AssemblyAiApiKey)) _config.AssemblyAiApiKey = "5d343a133e014d3c866928299bc267f0"; }
    public async void OnTranscribeApi() { if (_audio?.Samples == null) { LblStatus.Text = "Load audio file first"; return; } _config = new ConfigManager(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "RepeatSegment")); _config.Load(); ApDef(); if (string.IsNullOrEmpty(_config.DeepgramApiKey) && string.IsNullOrEmpty(_config.AssemblyAiApiKey)) { LblStatus.Text = "No API keys."; return; } _transcriptionProvider = new TranscriptionProvider(_config, _audio); _transcriptionProvider.SetSilenceZones(_detector.Silence); _transcriptionProvider.StatusChanged += msg => Dispatcher.Dispatch(() => LblStatus.Text = msg); try { BtnLoad.IsEnabled = false; LoadingOverlay.IsVisible = true; LoadingSpinner.IsRunning = true; LblLoadingStatus.Text = "Transcribing..."; await Task.Run(() => _transcriptionProvider.Transcribe(_audio.FilePath, true)); int wc = _transcriptionProvider.WordTimings.Count; if (wc > 0) { BuildWL(); LblStatus.Text = $"API: {wc} words"; } else { LblTranscription.Text = "No results."; LblStatus.Text = "0 words"; } } catch (Exception ex) { LblStatus.Text = ex.Message; } finally { BtnLoad.IsEnabled = true; LoadingOverlay.IsVisible = false; LoadingSpinner.IsRunning = false; } }
    public async void LoadFile(string fp) { if (!System.IO.File.Exists(fp)) { LblStatus.Text = "File not found"; return; } _transcriptionProvider = null; _lastHlIdx = -1; _lastScrollWordIdx = -1; _transcriptionWords = null; _wordCharPositions = null; _totalChars = 0; _prevScrollOffset = -1; LblTranscription.FormattedText = null; LblTranscription.Text = ""; BtnLoad.IsEnabled = false; LoadingOverlay.IsVisible = true; LoadingSpinner.IsRunning = true; LblLoadingStatus.Text = "Building waveform..."; LblFileName.Text = System.IO.Path.GetFileName(fp); _audio?.Stop(); _audio?.Dispose(); _audio = new AudioEngine(); _audio.PlaybackSpeed = Speeds[PckSpeed.SelectedIndex]; bool ok = await Task.Run(() => _audio.Load(fp)); if (!ok) { LblStatus.Text = "Error loading file"; BtnLoad.IsEnabled = true; LoadingOverlay.IsVisible = false; LoadingSpinner.IsRunning = false; return; } _durationSeconds = _audio.Duration.TotalSeconds; _positionSeconds = 0; _counter = 0; _fragments.Clear(); _pop = true; _pte = false; _playGoMode = false; _dt = 0; _userSegment = null; LblLoadingStatus.Text = "Detecting segments..."; await Task.Run(() => { _detector.Detect(_audio.Samples!, _audio.SampleRate, _durationSeconds, 5.0); _fragments = _detector.T1T2Array.ToList(); }); if (_fragments.Count > 0) { _t1 = _fragments[0].T1; _t2 = _fragments[0].T2; _positionSeconds = _t1; } else { _t1 = 0; _t2 = _durationSeconds; } SliderPosition.Minimum = 0; SliderPosition.Maximum = _durationSeconds; SliderPosition.Value = _positionSeconds; LblPosition.Text = Fmt(_positionSeconds); LblDuration.Text = Fmt(_durationSeconds); UpdateSeg(); _btnBlocked = false; WaveformCanvas.InvalidateSurface(); LblStatus.Text = $"Loaded: {_fragments.Count} segments"; BtnLoad.IsEnabled = true; LoadingOverlay.IsVisible = false; LoadingSpinner.IsRunning = false; Services.RecentManager.AddFile(fp); await ShowTD(fp); }
    private async Task ShowTD(string p) { string? hc = ChkCache(p); var o = new List<string> { "Yes, from API" }; o.Add(hc != null ? $"Yes, from cache ({hc} words)" : "Cache not available"); string? c = await DisplayActionSheet("Transcribe?", "Not now", null, o.ToArray()); if (c == null || c == "Not now") return; if (c == "Yes, from API") OnTranscribeApi(); else if (c == o[1] && hc != null) OnTranscribeCache(); }
    private string? ChkCache(string p) { string d = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "RepeatSegment", "output"); if (!Directory.Exists(d)) return null; string n = System.IO.Path.GetFileNameWithoutExtension(p); var fs = System.IO.Directory.GetFiles(d, $"{n}_chunk*_deepgram_*.json").Concat(System.IO.Directory.GetFiles(d, $"{n}_chunk*_assemblyai_*.json")).ToArray(); return fs.Length > 0 ? $"{fs.Length} chunks" : null; }

    private void BuildWL()
    {
        if (_transcriptionProvider?.WordTimings == null || _transcriptionProvider.WordTimings.Count == 0) return;
        var ws = _transcriptionProvider.WordTimings;
        _transcriptionWords = new string[ws.Count];
        _wordCharPositions = new int[ws.Count];
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < ws.Count; i++)
        {
            _transcriptionWords[i] = ws[i].Word;
            _wordCharPositions[i] = sb.Length;
            sb.Append(ws[i].Word);
            sb.Append(' ');
        }
        _totalChars = sb.Length;
        var fmt = new FormattedString();
        fmt.Spans.Add(new Span { Text = sb.ToString(), TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#CCC"), FontSize = 16 });
        LblTranscription.FormattedText = fmt;
    }

    private void HlWordBg()
    {
        if (_transcriptionProvider?.WordTimings == null || _transcriptionWords == null || _wordCharPositions == null || _totalChars == 0) return;
        var ws = _transcriptionProvider.WordTimings;
        if (ws.Count == 0) return;
        double pos = _positionSeconds;
        int last = _lastHlIdx;
        int idx = -1;
        for (int i = Math.Max(0, last - 5); i < Math.Min(ws.Count, last + 10); i++)
        {
            if (ws[i].Start <= pos && pos < ws[i].End) { idx = i; break; }
        }
        if (idx < 0)
        {
            int lo = 0, hi = ws.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (ws[mid].Start <= pos) { idx = mid; lo = mid + 1; }
                else hi = mid - 1;
            }
        }
        if (idx < 0 || idx >= ws.Count) return;
        int hlFirst = idx, hlLast = idx;
        double totalDur = ws[idx].End - ws[idx].Start;
        while (hlLast + 1 < ws.Count && totalDur + (ws[hlLast + 1].End - ws[hlLast + 1].Start) < 0.35)
        {
            hlLast++;
            totalDur += ws[hlLast].End - ws[hlLast].Start;
        }
        if (hlFirst == hlLast && idx == last) return;
        int newIdx = hlFirst;
        var words = _transcriptionWords;
        var fmt = new FormattedString();
        var tsb = new System.Text.StringBuilder();
        int hlStart = -1, hlEnd = -1;
        for (int i = 0; i < words.Length; i++)
        {
            if (i == hlFirst) hlStart = tsb.Length;
            tsb.Append(words[i]);
            tsb.Append(' ');
            if (i == hlLast) hlEnd = tsb.Length;
        }
        string fullText = tsb.ToString();
        if (hlStart >= 0)
        {
            if (hlStart > 0)
                fmt.Spans.Add(new Span { Text = fullText.Substring(0, hlStart), TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#CCC"), FontSize = 16 });
            fmt.Spans.Add(new Span { Text = fullText.Substring(hlStart, hlEnd - hlStart), TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#111"), BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFD700"), FontAttributes = FontAttributes.Bold, FontSize = 16 });
            if (hlEnd < fullText.Length)
                fmt.Spans.Add(new Span { Text = fullText.Substring(hlEnd), TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#CCC"), FontSize = 16 });
        }
        else
        {
            fmt.Spans.Add(new Span { Text = fullText, TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#CCC"), FontSize = 16 });
        }
        var finalFmt = fmt;
        int finalIdx = newIdx;
        int charPos = _wordCharPositions[finalIdx];
        // Proportional scroll: MAUI Label knows exact content height
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblTranscription.FormattedText = finalFmt;
            _lastHlIdx = finalIdx;
            double contentHeight = LblTranscription.Height;
            if (contentHeight <= 0.1) contentHeight = _totalChars * 0.22; // fallback
            double wordY = ((double)charPos / _totalChars) * contentHeight;
            double viewH = TransScrollView.Height;
            if (viewH <= 0) viewH = 190;
            double targetOffset = wordY - viewH / 2;
            if (targetOffset < 0) targetOffset = 0;
            double maxOffset = Math.Max(0, contentHeight - viewH);
            if (targetOffset > maxOffset) targetOffset = maxOffset;
            if (Math.Abs(targetOffset - _prevScrollOffset) > 3)
            {
                _prevScrollOffset = targetOffset;
                _ = TransScrollView.ScrollToAsync(0, targetOffset, animated: true);
            }
        });
    }

    private void RequestAudioFocusViaService()
    {
        var am = Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService) as Android.Media.AudioManager;
        if (am == null) return;
        Android.Util.Log.Info("RepeatSegment", "AUDIOFOCUS: requesting (legacy API)...");
        _playerFocusListener = new PlayerFocusListener();
        var result = am.RequestAudioFocus(_playerFocusListener, global::Android.Media.Stream.Music, Android.Media.AudioFocus.Gain);
        Android.Util.Log.Info("RepeatSegment", $"AUDIOFOCUS: result={result}");
    }

    private class PlayerFocusListener : Java.Lang.Object, Android.Media.AudioManager.IOnAudioFocusChangeListener
    {
        public void OnAudioFocusChange(Android.Media.AudioFocus focusChange)
        {
            string? cmd = focusChange switch
            {
                Android.Media.AudioFocus.Loss => "focus_loss",
                Android.Media.AudioFocus.LossTransient => "focus_loss_transient",
                Android.Media.AudioFocus.LossTransientCanDuck => "focus_duck",
                Android.Media.AudioFocus.Gain => "focus_gain",
                _ => null
            };
            if (cmd != null)
                MainThread.BeginInvokeOnMainThread(() => PlaybackBridge.Post(cmd));
        }
    }
}
