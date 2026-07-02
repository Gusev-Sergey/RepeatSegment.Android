# Graph Report - RepeatSegment.Android  (2026-07-02)

## Corpus Check
- 41 files · ~68,418 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 480 nodes · 691 edges · 67 communities (20 shown, 47 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 4 edges (avg confidence: 0.75)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c4724bd7`
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
- [[_COMMUNITY_Community 55|Community 55]]
- [[_COMMUNITY_Community 56|Community 56]]
- [[_COMMUNITY_Community 57|Community 57]]
- [[_COMMUNITY_Community 58|Community 58]]
- [[_COMMUNITY_Community 59|Community 59]]
- [[_COMMUNITY_Community 60|Community 60]]
- [[_COMMUNITY_Community 61|Community 61]]
- [[_COMMUNITY_Community 62|Community 62]]
- [[_COMMUNITY_Community 63|Community 63]]
- [[_COMMUNITY_Community 64|Community 64]]
- [[_COMMUNITY_Community 65|Community 65]]
- [[_COMMUNITY_Community 66|Community 66]]
- [[_COMMUNITY_Community 67|Community 67]]
- [[_COMMUNITY_Community 68|Community 68]]
- [[_COMMUNITY_Community 69|Community 69]]

## God Nodes (most connected - your core abstractions)
1. `PlayerPage` - 91 edges
2. `AudioEngine` - 40 edges
3. `AnkiCardPage` - 31 edges
4. `MenuPage` - 22 edges
5. `PlaybackService` - 18 edges
6. `ImageSearchPage` - 13 edges
7. `AnkiCardPage v0.1 — План реализации (Android MAUI)` - 12 edges
8. `SettingsPage` - 11 edges
9. `NativeMagnifier` - 9 edges
10. `RepeatSegment — План портирования WPF → MAUI Android` - 8 edges

## Surprising Connections (you probably didn't know these)
- `Bug1: TapGestureRecognizer forgets translation` --conceptually_related_to--> `NativeTouch`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Platforms/Android/NativeTouch.cs
- `Bug3: Loupe too far from touch` --conceptually_related_to--> `TransOverlay`  [EXTRACTED]
  BUGFIND_LOG.md → RepeatSegment.Maui/Pages/PlayerPage.xaml
- `PlayerPage` --references--> `AudioEngine`  [EXTRACTED]
  RepeatSegment.Maui/Pages/PlayerPage.xaml.cs → RepeatSegment.Maui/Services/AudioEngine.cs
- `AnkiCardPage` --references--> `AudioEngine`  [EXTRACTED]
  RepeatSegment.Maui/Pages/AnkiCardPage.xaml.cs → RepeatSegment.Maui/Services/AudioEngine.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Android App Icons Set** — repeatsegment_droid_resources_mipmap_hdpi_appicon, repeatsegment_droid_resources_mipmap_hdpi_appicon_background, repeatsegment_droid_resources_mipmap_hdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_mdpi_appicon, repeatsegment_droid_resources_mipmap_mdpi_appicon_background, repeatsegment_droid_resources_mipmap_mdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xhdpi_appicon, repeatsegment_droid_resources_mipmap_xhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_foreground, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher_round, repeatsegment_maui_resources_appicon_appicon [INFERRED 0.85]
- **Waveform Smooth Rendering Analysis Group** — waveform_attack_plan_doc, waveform_diagnosis_doc, waveform_solution_doc, playerpage_playerpage [INFERRED 0.80]
- **Text Selection & Loupe Flow** — playerpage_xaml_ontranstouch, playerpage_xaml_showloupe, nativemagnifier_capture, playerpage_loupeoverlay, playerpage_loupeimage, playerpage_xaml_selanchorchar, playerpage_xaml_selstartchar, playerpage_xaml_selendchar, playerpage_xaml_rebuiltranscriptspans, playerpage_lbltranscription [EXTRACTED 0.85]

## Communities (67 total, 47 thin omitted)

### Community 0 - "PlayerPage Audio Controls"
Cohesion: 0.06
Nodes (19): AudioEngine, AudioManager, byte, Handler, IDispatcherTimer, l, PlayerPage, PinchGestureUpdatedEventArgs (+11 more)

### Community 1 - "Audio Engine Decoding"
Cohesion: 0.10
Nodes (8): AudioTrack, DecodeResult, IDisposable, long, AudioEngine, Log, short, string

### Community 2 - "Project Context & Dependencies"
Cohesion: 0.12
Nodes (10): AudioFocus, Callback, Context, IOnAudioFocusChangeListener, Object, PlayerFocusListener, FocusListener, PlaybackBridge (+2 more)

### Community 3 - "Menu Page Navigation"
Cohesion: 0.10
Nodes (14): MainActivity, AudioFocusRequestClass, IBinder, int, Intent, MauiAppCompatActivity, MediaSession, Notification (+6 more)

### Community 4 - "MAUI App Entry Points"
Cohesion: 0.09
Nodes (4): EventArgs, MenuPage, AppShell, Shell

### Community 5 - "Audio Focus Management"
Cohesion: 0.09
Nodes (10): MainApplication, AppDelegate, AppDelegate, MauiApp, MauiApplication, MauiUIApplicationDelegate, MauiWinUIApplication, MauiProgram (+2 more)

### Community 6 - "Playback Service & Notifications"
Cohesion: 0.40
Nodes (5): Translation, Google Translate, TranslationPanel, TranslationProvider.TranslateEnRu(), Yandex Translate

### Community 9 - "Logging and Recent Files"
Cohesion: 0.11
Nodes (15): Action, NativeMagnifier, NativeTouch, TL, Bitmap, bool, Border, BoxView (+7 more)

### Community 10 - "Android Activity Lifecycle"
Cohesion: 0.30
Nodes (3): SKCanvas, SKColor, SKPaintSurfaceEventArgs

### Community 11 - "MAUI App Window"
Cohesion: 0.25
Nodes (4): Activity, Bundle, MainActivity, MainActivity

### Community 12 - "MAUI Asset Loading"
Cohesion: 0.25
Nodes (7): net9.0-android, Microsoft.Data.Sqlite (9.0.*), Microsoft.Maui.Controls (9.0.*), Microsoft.Maui.Controls.Compatibility (9.0.*), SkiaSharp.Views.Maui.Controls (4.148.0), System.Net.Http.Json (9.0.*), Microsoft.NET.Sdk

### Community 13 - "Asset Resources Configuration"
Cohesion: 0.33
Nodes (4): Application, IActivationState, App, Window

### Community 15 - "Program Entry Point"
Cohesion: 0.06
Nodes (31): 10. Что НЕ делаем в v0.1, 1. Что уже есть (shared из WPF), 2. Файлы для создания / изменения, 3. Решённые проблемы из WPF (AI_NOTES.md), 4. Детальный UI: AnkiCardPage, 5. Детальный UI: ImageSearchPage (модальный WebView), 6. Передача данных между страницами, 7. Порядок реализации (5 шагов) (+23 more)

### Community 40 - "Launcher Icon Round XXXHDPI"
Cohesion: 0.07
Nodes (11): CheckedChangedEventArgs, ConfigManager, ContentPage, double, List, MediaRecorder, AnkiCardPage, SettingsPage (+3 more)

### Community 41 - "App Icon Generic"
Cohesion: 0.07
Nodes (27): Recovery Log — 2026-07-01/02 (MSK), Бонус: CleanWord() + FindSentenceBounds, Включает финальные находки от Gemini, Задача, Метод восстановления, Неудачные попытки (7 итераций), Причина потери, Причины провалов (+19 more)

### Community 55 - "Community 55"
Cohesion: 0.13
Nodes (14): Phase 2 из WPF-плана, Phase 3 из WPF-плана, RepeatSegment — План портирования WPF → MAUI Android, Баг 1: Play с начала файла пропускает первый сегмент, Баг 2: Непрерывное проигрывание пропускает сегмент без звука, Баги, исправленные 29.06.2026, План дальнейшего портирования (4 фазы), Текущее состояние Phase 1 (MVP) из WPF-плана (+6 more)

### Community 57 - "Community 57"
Cohesion: 0.21
Nodes (3): ImageSearchPage, TaskCompletionSource, WebNavigatedEventArgs

## Knowledge Gaps
- **107 isolated node(s):** `What's New (July 1-2 session)`, `Quick Start`, `Key Files`, `Shared files (from WPF)`, `Known Issues` (+102 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **47 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerPage` connect `PlayerPage Audio Controls` to `Audio Engine Decoding`, `Project Context & Dependencies`, `Menu Page Navigation`, `MAUI App Entry Points`, `Launcher Icon Round XXXHDPI`, `Logging and Recent Files`, `Android Activity Lifecycle`?**
  _High betweenness centrality (0.207) - this node is a cross-community bridge._
- **Why does `AudioEngine` connect `Audio Engine Decoding` to `Launcher Icon Round XXXHDPI`, `PlayerPage Audio Controls`, `Menu Page Navigation`, `Logging and Recent Files`?**
  _High betweenness centrality (0.093) - this node is a cross-community bridge._
- **Why does `PlaybackService` connect `Menu Page Navigation` to `PlayerPage Audio Controls`, `Logging and Recent Files`, `Project Context & Dependencies`, `Audio Engine Decoding`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **What connects `What's New (July 1-2 session)`, `Quick Start`, `Key Files` to the rest of the system?**
  _108 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PlayerPage Audio Controls` be split into smaller, more focused modules?**
  _Cohesion score 0.06009615384615385 - nodes in this community are weakly interconnected._
- **Should `Audio Engine Decoding` be split into smaller, more focused modules?**
  _Cohesion score 0.09745293466223699 - nodes in this community are weakly interconnected._
- **Should `Project Context & Dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.1225296442687747 - nodes in this community are weakly interconnected._