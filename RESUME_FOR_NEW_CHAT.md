# RepeatSegment Android — Resume for New Chat (July 2, 2026)

## What's New (July 1-2 session)
- **AnkiCardPage v0.1** — full card UI + image search + TTS + mic recording + .apkg export
- **Open Decks button** solved: two-step Intent (DocumentsUI → DownloadManager fallback)
- See [`RECOVERY_LOG.md`](RECOVERY_LOG.md) for full history

## Quick Start
```powershell
# Build
powershell -ExecutionPolicy Bypass -File "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\build_release.ps1"
# Install
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"
```

## Key Files
| File | Purpose |
|------|---------|
| [`PlayerPage.xaml.cs`](RepeatSegment.Maui/Pages/PlayerPage.xaml.cs) | Playback, selection, loupe, translation, CleanWord(), OnAnkiClicked |
| [`AnkiCardPage.xaml.cs`](RepeatSegment.Maui/Pages/AnkiCardPage.xaml.cs) | Card UI, IPA, TTS, mic record, AnkiExportManager, Open decks |
| [`ImageSearchPage.xaml.cs`](RepeatSegment.Maui/Pages/ImageSearchPage.xaml.cs) | Full-screen WebView (Google/Yandex) + JS injection |
| [`AudioEngine.cs`](RepeatSegment.Maui/Services/AudioEngine.cs) | MediaExtractor + AudioTrack, .samples/.waveform cache, SOLA stretch |
| [`NativeTouch.cs`](RepeatSegment.Maui/Platforms/Android/NativeTouch.cs) | TouchListener + pinch-zoom |
| [`NativeMagnifier.cs`](RepeatSegment.Maui/Platforms/Android/NativeMagnifier.cs) | Loupe via View.Draw, native ImageView |
| [`PlaybackService.cs`](RepeatSegment.Maui/Services/PlaybackService.cs) | Foreground service + notification |
| [`AppShell.xaml.cs`](RepeatSegment.Maui/AppShell.xaml.cs) | Routes: player, menu, settings, ankiCard |

## Shared files (from WPF)
8 files linked via csproj: SilenceDetector, TranscriptionProvider, TranslationProvider, TtsProvider, ConfigManager, Strings, AnkiBuilder, AnkiExportManager

## Known Issues
1. Deepgram transcription not working on Android (works on WPF)
2. Mic recording saves MP4 (AAC), Anki needs MP3
3. Google Images WebView may show captcha on mobile

## Documentation
- [`BUGFIND_LOG.md`](BUGFIND_LOG.md) — all bugs with root cause analysis
- [`RECOVERY_LOG.md`](RECOVERY_LOG.md) — git recovery + Open button battle (7 iterations + solution)
- [`MAUI_DEBUG_NOTES.md`](MAUI_DEBUG_NOTES.md) — Android porting critical findings (SkiaSharp, 16KB pages, etc.)
- [`PLAN_PORTING.md`](PLAN_PORTING.md) — WPF→MAUI migration matrix
- [`WAVEFORM.md`](WAVEFORM.md) — waveform smooth rendering: diagnosis, attack plan, solution
- [`plans/anki_card_page_v01.md`](plans/anki_card_page_v01.md) — AnkiCardPage implementation plan
