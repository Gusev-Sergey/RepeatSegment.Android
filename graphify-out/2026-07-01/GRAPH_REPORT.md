# Graph Report - c:\ProjectsCSharp\RepeatSegment.Android  (2026-07-01)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 375 nodes · 555 edges · 57 communities (21 shown, 36 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 10 edges (avg confidence: 0.76)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1414a80c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_PlayerPage Audio Controls|PlayerPage Audio Controls]]
- [[_COMMUNITY_Audio Engine Decoding|Audio Engine Decoding]]
- [[_COMMUNITY_Project Context & Dependencies|Project Context & Dependencies]]
- [[_COMMUNITY_Menu Page Navigation|Menu Page Navigation]]
- [[_COMMUNITY_MAUI App Entry Points|MAUI App Entry Points]]
- [[_COMMUNITY_Audio Focus Management|Audio Focus Management]]
- [[_COMMUNITY_Playback Service & Notifications|Playback Service & Notifications]]
- [[_COMMUNITY_Settings Page Configuration|Settings Page Configuration]]
- [[_COMMUNITY_Waveform Drawing (SkiaSharp)|Waveform Drawing (SkiaSharp)]]
- [[_COMMUNITY_Logging and Recent Files|Logging and Recent Files]]
- [[_COMMUNITY_Android Activity Lifecycle|Android Activity Lifecycle]]
- [[_COMMUNITY_MAUI App Window|MAUI App Window]]
- [[_COMMUNITY_MAUI Asset Loading|MAUI Asset Loading]]
- [[_COMMUNITY_Asset Resources Configuration|Asset Resources Configuration]]
- [[_COMMUNITY_MAUI Android MainActivity|MAUI Android MainActivity]]
- [[_COMMUNITY_Program Entry Point|Program Entry Point]]
- [[_COMMUNITY_Program Entry Point|Program Entry Point]]
- [[_COMMUNITY_Android Project Configuration|Android Project Configuration]]
- [[_COMMUNITY_Native Project Configuration|Native Project Configuration]]
- [[_COMMUNITY_Android Resources Guide|Android Resources Guide]]
- [[_COMMUNITY_WPF Audio Engine|WPF Audio Engine]]
- [[_COMMUNITY_Previous Track Icon|Previous Track Icon]]
- [[_COMMUNITY_Next Track Icon|Next Track Icon]]
- [[_COMMUNITY_PlayGo Icon|Play/Go Icon]]
- [[_COMMUNITY_StopPause Icon|Stop/Pause Icon]]
- [[_COMMUNITY_App Icon HDPI|App Icon HDPI]]
- [[_COMMUNITY_App Icon MDPI|App Icon MDPI]]
- [[_COMMUNITY_App Icon XHDPI|App Icon XHDPI]]
- [[_COMMUNITY_Emulator Run Instructions|Emulator Run Instructions]]
- [[_COMMUNITY_App Icon HDPI|App Icon HDPI]]
- [[_COMMUNITY_App Icon Background HDPI|App Icon Background HDPI]]
- [[_COMMUNITY_App Icon Foreground HDPI|App Icon Foreground HDPI]]
- [[_COMMUNITY_App Icon MDPI|App Icon MDPI]]
- [[_COMMUNITY_App Icon XHDPI|App Icon XHDPI]]
- [[_COMMUNITY_App Icon XXHDPI|App Icon XXHDPI]]
- [[_COMMUNITY_App Icon XXXHDPI|App Icon XXXHDPI]]
- [[_COMMUNITY_Launcher Icon XXXHDPI|Launcher Icon XXXHDPI]]
- [[_COMMUNITY_Launcher Icon Round XXXHDPI|Launcher Icon Round XXXHDPI]]
- [[_COMMUNITY_App Icon Generic|App Icon Generic]]
- [[_COMMUNITY_First Image|First Image]]
- [[_COMMUNITY_Resume Chat Note|Resume Chat Note]]
- [[_COMMUNITY_WPF Silence Detector|WPF Silence Detector]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Community 53|Community 53]]
- [[_COMMUNITY_Community 54|Community 54]]
- [[_COMMUNITY_Community 56|Community 56]]

## God Nodes (most connected - your core abstractions)
1. `PlayerPage` - 90 edges
2. `AudioEngine` - 37 edges
3. `MenuPage` - 21 edges
4. `PlaybackService` - 19 edges
5. `SettingsPage` - 11 edges
6. `ShowLoupe()` - 9 edges
7. `NativeMagnifier` - 8 edges
8. `TL` - 8 edges
9. `Log` - 7 edges
10. `SessionCb` - 7 edges

## Surprising Connections (you probably didn't know these)
- `Bug1: TapGestureRecognizer forgets translation` --conceptually_related_to--> `NativeTouch`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Platforms/Android/NativeTouch.cs
- `Bug3: Loupe too far from touch` --conceptually_related_to--> `ShowLoupe()`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Pages/PlayerPage.xaml.cs
- `Bug4: Loupe magnification incorrect` --conceptually_related_to--> `ShowLoupe()`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Pages/PlayerPage.xaml.cs
- `Bug7: Loupe image stuttering` --conceptually_related_to--> `ShowLoupe()`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Pages/PlayerPage.xaml.cs
- `Bug6: Crash during playback (thread)` --conceptually_related_to--> `RebuildTranscriptSpans()`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Pages/PlayerPage.xaml.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Android App Icons Set** — repeatsegment_droid_resources_mipmap_hdpi_appicon, repeatsegment_droid_resources_mipmap_hdpi_appicon_background, repeatsegment_droid_resources_mipmap_hdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_mdpi_appicon, repeatsegment_droid_resources_mipmap_mdpi_appicon_background, repeatsegment_droid_resources_mipmap_mdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xhdpi_appicon, repeatsegment_droid_resources_mipmap_xhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_foreground, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher_round, repeatsegment_maui_resources_appicon_appicon [INFERRED 0.85]
- **Waveform Smooth Rendering Analysis Group** — waveform_attack_plan_doc, waveform_diagnosis_doc, waveform_solution_doc, playerpage_playerpage [INFERRED 0.80]
- **Text Selection & Loupe Flow** — playerpage_xaml_ontranstouch, playerpage_xaml_showloupe, nativemagnifier_capture, playerpage_loupeoverlay, playerpage_loupeimage, playerpage_xaml_selanchorchar, playerpage_xaml_selstartchar, playerpage_xaml_selendchar, playerpage_xaml_rebuiltranscriptspans, playerpage_lbltranscription [EXTRACTED 0.85]

## Communities (57 total, 36 thin omitted)

### Community 0 - "PlayerPage Audio Controls"
Cohesion: 0.06
Nodes (18): AudioManager, byte, double, Handler, IDispatcherTimer, List, PlayerPage, PinchGestureUpdatedEventArgs (+10 more)

### Community 1 - "Audio Engine Decoding"
Cohesion: 0.12
Nodes (6): AudioTrack, DecodeResult, IDisposable, long, AudioEngine, short

### Community 2 - "Project Context & Dependencies"
Cohesion: 0.10
Nodes (12): AudioFocus, Callback, Context, IOnAudioFocusChangeListener, Object, PlayerFocusListener, Log, FocusListener (+4 more)

### Community 3 - "Menu Page Navigation"
Cohesion: 0.10
Nodes (14): MainActivity, AudioFocusRequestClass, IBinder, int, Intent, MauiAppCompatActivity, MediaSession, Notification (+6 more)

### Community 4 - "MAUI App Entry Points"
Cohesion: 0.14
Nodes (4): EventArgs, MenuPage, AppShell, Shell

### Community 5 - "Audio Focus Management"
Cohesion: 0.09
Nodes (10): MainApplication, AppDelegate, AppDelegate, MauiApp, MauiApplication, MauiUIApplicationDelegate, MauiWinUIApplication, MauiProgram (+2 more)

### Community 6 - "Playback Service & Notifications"
Cohesion: 0.13
Nodes (18): Bug1: TapGestureRecognizer forgets translation, Bug2: Character-level selection indices wrong, Bug5: Selection broken for reverse direction, Bug6: Crash during playback (thread), Bug8: Bold causes text reflow, Text Selection, Translation, Google Translate (+10 more)

### Community 7 - "Settings Page Configuration"
Cohesion: 0.14
Nodes (13): NativeMagnifier, Bug3: Loupe too far from touch, Bug4: Loupe magnification incorrect, Bug7: Loupe image stuttering, Loupe (magnifier), Image, ImageView, Label (+5 more)

### Community 8 - "Waveform Drawing (SkiaSharp)"
Cohesion: 0.14
Nodes (5): CheckedChangedEventArgs, ConfigManager, ContentPage, SettingsPage, MainPage

### Community 9 - "Logging and Recent Files"
Cohesion: 0.20
Nodes (10): Action, NativeTouch, TL, bool, BoxView, float, IOnTouchListener, MotionEvent (+2 more)

### Community 10 - "Android Activity Lifecycle"
Cohesion: 0.30
Nodes (3): SKCanvas, SKColor, SKPaintSurfaceEventArgs

### Community 11 - "MAUI App Window"
Cohesion: 0.29
Nodes (4): Activity, Bundle, MainActivity, MainActivity

### Community 12 - "MAUI Asset Loading"
Cohesion: 0.25
Nodes (7): net9.0-android, Microsoft.Data.Sqlite (9.0.*), Microsoft.Maui.Controls (9.0.*), Microsoft.Maui.Controls.Compatibility (9.0.*), SkiaSharp.Views.Maui.Controls (4.148.0), System.Net.Http.Json (9.0.*), Microsoft.NET.Sdk

### Community 13 - "Asset Resources Configuration"
Cohesion: 0.33
Nodes (4): Application, IActivationState, App, Window

### Community 15 - "Program Entry Point"
Cohesion: 0.50
Nodes (3): l, r, w

### Community 21 - "Previous Track Icon"
Cohesion: 0.67
Nodes (3): Waveform Smooth Rendering — Attack Plan, Waveform Smooth Rendering — Diagnosis, Waveform Smooth Rendering — Solution

## Knowledge Gaps
- **47 isolated node(s):** `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android` (+42 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **36 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerPage` connect `PlayerPage Audio Controls` to `Audio Engine Decoding`, `Project Context & Dependencies`, `Menu Page Navigation`, `MAUI App Entry Points`, `Waveform Drawing (SkiaSharp)`, `Logging and Recent Files`, `Android Activity Lifecycle`, `Program Entry Point`?**
  _High betweenness centrality (0.315) - this node is a cross-community bridge._
- **Why does `PlaybackService` connect `Menu Page Navigation` to `PlayerPage Audio Controls`, `Logging and Recent Files`, `Project Context & Dependencies`?**
  _High betweenness centrality (0.132) - this node is a cross-community bridge._
- **Why does `TL` connect `Logging and Recent Files` to `Project Context & Dependencies`?**
  _High betweenness centrality (0.127) - this node is a cross-community bridge._
- **What connects `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android` to the rest of the system?**
  _48 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PlayerPage Audio Controls` be split into smaller, more focused modules?**
  _Cohesion score 0.05593607305936073 - nodes in this community are weakly interconnected._
- **Should `Audio Engine Decoding` be split into smaller, more focused modules?**
  _Cohesion score 0.11596638655462185 - nodes in this community are weakly interconnected._
- **Should `Project Context & Dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.0960591133004926 - nodes in this community are weakly interconnected._