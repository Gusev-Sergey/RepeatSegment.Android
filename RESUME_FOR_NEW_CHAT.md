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

## Push to GitHub (secrets handling)

**API keys must stay LOCAL — NEVER pushed to GitHub.** GitHub Push Protection blocks commits containing secrets.

### Setup: git attribute filter
```bash
# Run once per clone:
git config filter.strip_secrets.clean "sed -e 's/=.*/= \"\";/g' -e 's/ = .*/ = \"\";/g'"
git config filter.strip_secrets.smudge cat
echo "PlayerPage.xaml.cs filter=strip_secrets" >> .git/info/attributes
echo "SettingsPage.xaml.cs filter=strip_secrets" >> .git/info/attributes
```

### Before push checklist
1. **Check**: `git diff --cached` — no API keys in staged changes
2. **Commit**: `git commit -m "message"`
3. **If blocked by push protection** — keys are in earlier commits. Squash history:
   ```bash
   git reset --soft <last_clean_commit>
   git add -A
   git commit -m "combined commit"
   git push origin master --force
   ```
4. **After push**: restore API keys locally from `config.ini` backup
