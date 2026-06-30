# Graph Report - c:\ProjectsCSharp\RepeatSegment.Android  (2026-06-30)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 313 nodes · 480 edges · 46 communities (12 shown, 34 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 23 edges (avg confidence: 0.75)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `3542f1da`
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
- [[_COMMUNITY_Play Icon|Play Icon]]
- [[_COMMUNITY_PlayGo Icon|Play/Go Icon]]
- [[_COMMUNITY_Previous Play Icon|Previous Play Icon]]
- [[_COMMUNITY_Repeat Icon|Repeat Icon]]
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
- [[_COMMUNITY_Dotnet Bot Image|Dotnet Bot Image]]
- [[_COMMUNITY_First Image|First Image]]
- [[_COMMUNITY_Resume Chat Note|Resume Chat Note]]
- [[_COMMUNITY_WPF Silence Detector|WPF Silence Detector]]

## God Nodes (most connected - your core abstractions)
1. `PlayerPage` - 78 edges
2. `AudioEngine` - 32 edges
3. `MenuPage` - 21 edges
4. `PlaybackService` - 18 edges
5. `Porting Plan WPF → MAUI Android` - 15 edges
6. `SettingsPage` - 8 edges
7. `MAUI Android Debug Notes` - 8 edges
8. `RepeatSegment Android — Project Context` - 8 edges
9. `Log` - 7 edges
10. `SessionCb` - 7 edges

## Surprising Connections (you probably didn't know these)
- `Porting Plan WPF → MAUI Android` --references--> `VolumeWidget.cs (WPF)`  [INFERRED]
  PLAN_PORTING.md → c:/ProjectsCSharp/RepeatSegment/RepeatSegment.App/VolumeWidget.cs
- `Porting Plan WPF → MAUI Android` --references--> `WaveformGraph.cs (WPF)`  [INFERRED]
  PLAN_PORTING.md → c:/ProjectsCSharp/RepeatSegment/RepeatSegment.App/WaveformGraph.cs
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Solution`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_SOLUTION.md
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Attack Plan`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_ATTACK_PLAN.md
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Diagnosis`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_DIAGNOSIS.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Media Control Icons** — images_last, images_next_play, images_play, images_play_go, images_pre_play, images_repeat, images_stop_play [INFERRED 0.80]
- **App Icons (various densities)** — mipmap_hdpi_appicon, mipmap_mdpi_appicon, mipmap_xhdpi_appicon, mipmap_xxhdpi_appicon, mipmap_xxxhdpi_appicon [EXTRACTED 1.00]
- **Waveform Smooth Rendering Analysis Group** — waveform_attack_plan_doc, waveform_diagnosis_doc, waveform_solution_doc, playerpage_playerpage [INFERRED 0.80]
- **Android App Icons Set** — repeatsegment_droid_resources_mipmap_hdpi_appicon, repeatsegment_droid_resources_mipmap_hdpi_appicon_background, repeatsegment_droid_resources_mipmap_hdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_mdpi_appicon, repeatsegment_droid_resources_mipmap_mdpi_appicon_background, repeatsegment_droid_resources_mipmap_mdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xhdpi_appicon, repeatsegment_droid_resources_mipmap_xhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_foreground, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher_round, repeatsegment_maui_resources_appicon_appicon [INFERRED 0.85]

## Communities (46 total, 34 thin omitted)

### Community 0 - "PlayerPage Audio Controls"
Cohesion: 0.06
Nodes (17): AudioManager, double, Handler, IDispatcherTimer, l, PlayerPage, r, SilenceDetector (+9 more)

### Community 1 - "Audio Engine Decoding"
Cohesion: 0.13
Nodes (7): AudioTrack, DecodeResult, float, IDisposable, long, AudioEngine, short

### Community 2 - "Project Context & Dependencies"
Cohesion: 0.09
Nodes (16): Build Log (Release), MAUI Android Debug Notes, Microsoft.Data.Sqlite (9.0.*), Microsoft.Maui.Controls (9.0.*), Microsoft.Maui.Controls.Compatibility (9.0.*), SkiaSharp.Views.Maui.Controls (4.148.0), System.Net.Http.Json (9.0.*), Porting Plan WPF → MAUI Android (+8 more)

### Community 3 - "Menu Page Navigation"
Cohesion: 0.15
Nodes (5): Action, EventArgs, MenuPage, AppShell, Shell

### Community 4 - "MAUI App Entry Points"
Cohesion: 0.09
Nodes (10): MainApplication, AppDelegate, AppDelegate, MauiApp, MauiApplication, MauiUIApplicationDelegate, MauiWinUIApplication, MauiProgram (+2 more)

### Community 5 - "Audio Focus Management"
Cohesion: 0.13
Nodes (10): AudioFocus, Callback, Context, IOnAudioFocusChangeListener, Object, PlayerFocusListener, FocusListener, PlaybackBridge (+2 more)

### Community 6 - "Playback Service & Notifications"
Cohesion: 0.13
Nodes (11): AudioFocusRequestClass, bool, IBinder, int, Intent, MediaSession, Notification, Service (+3 more)

### Community 7 - "Settings Page Configuration"
Cohesion: 0.17
Nodes (5): CheckedChangedEventArgs, ConfigManager, ContentPage, SettingsPage, MainPage

### Community 8 - "Waveform Drawing (SkiaSharp)"
Cohesion: 0.30
Nodes (3): SKCanvas, SKColor, SKPaintSurfaceEventArgs

### Community 9 - "Logging and Recent Files"
Cohesion: 0.24
Nodes (4): List, Log, RecentManager, string

### Community 10 - "Android Activity Lifecycle"
Cohesion: 0.29
Nodes (4): Activity, Bundle, MainActivity, MainActivity

### Community 11 - "MAUI App Window"
Cohesion: 0.33
Nodes (4): Application, IActivationState, App, Window

## Knowledge Gaps
- **42 isolated node(s):** `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android`, `Microsoft.Maui.Controls (9.0.*)`, `Microsoft.Maui.Controls.Compatibility (9.0.*)` (+37 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **34 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerPage` connect `PlayerPage Audio Controls` to `Audio Engine Decoding`, `Project Context & Dependencies`, `Menu Page Navigation`, `Audio Focus Management`, `Playback Service & Notifications`, `Settings Page Configuration`, `Waveform Drawing (SkiaSharp)`, `Logging and Recent Files`?**
  _High betweenness centrality (0.379) - this node is a cross-community bridge._
- **Why does `AudioEngine` connect `Audio Engine Decoding` to `PlayerPage Audio Controls`, `Project Context & Dependencies`, `Playback Service & Notifications`?**
  _High betweenness centrality (0.153) - this node is a cross-community bridge._
- **Why does `MAUI Android Debug Notes` connect `Project Context & Dependencies` to `MAUI App Entry Points`, `Audio Focus Management`?**
  _High betweenness centrality (0.149) - this node is a cross-community bridge._
- **What connects `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android` to the rest of the system?**
  _42 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PlayerPage Audio Controls` be split into smaller, more focused modules?**
  _Cohesion score 0.06448412698412699 - nodes in this community are weakly interconnected._
- **Should `Audio Engine Decoding` be split into smaller, more focused modules?**
  _Cohesion score 0.12688172043010754 - nodes in this community are weakly interconnected._
- **Should `Project Context & Dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.08505747126436781 - nodes in this community are weakly interconnected._