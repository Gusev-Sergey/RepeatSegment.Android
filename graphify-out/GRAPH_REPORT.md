# Graph Report - RepeatSegment.Android  (2026-06-30)

## Corpus Check
- 39 files · ~64,412 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 469 nodes · 642 edges · 53 communities (19 shown, 34 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 11 edges (avg confidence: 0.75)
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
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]

## God Nodes (most connected - your core abstractions)
1. `PlayerPage` - 85 edges
2. `AudioEngine` - 32 edges
3. `MenuPage` - 21 edges
4. `PlaybackService` - 18 edges
5. `Критические находки при портировании на Android` - 12 edges
6. `RepeatSegment Android — Итоговый контекст для нового чата` - 11 edges
7. `RepeatSegment — Resume for New Chat (June 29, 2026)` - 10 edges
8. `SettingsPage` - 8 edges
9. `RepeatSegment — План портирования WPF → MAUI Android` - 8 edges
10. `RepeatSegment Android — Project Context` - 8 edges

## Surprising Connections (you probably didn't know these)
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Solution`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_SOLUTION.md
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Attack Plan`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_ATTACK_PLAN.md
- `RepeatSegment Android — Project Context` --references--> `Waveform Smooth Rendering — Diagnosis`  [EXTRACTED]
  PROJECT_CONTEXT.md → WAVEFORM_DIAGNOSIS.md
- `Waveform Smooth Rendering — Attack Plan` --references--> `Waveform Smooth Rendering — Diagnosis`  [EXTRACTED]
  WAVEFORM_ATTACK_PLAN.md → WAVEFORM_DIAGNOSIS.md
- `Waveform Smooth Rendering — Solution` --references--> `Waveform Smooth Rendering — Diagnosis`  [EXTRACTED]
  WAVEFORM_SOLUTION.md → WAVEFORM_DIAGNOSIS.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Media Control Icons** — images_last, images_next_play, images_play, images_play_go, images_pre_play, images_repeat, images_stop_play [INFERRED 0.80]
- **App Icons (various densities)** — mipmap_hdpi_appicon, mipmap_mdpi_appicon, mipmap_xhdpi_appicon, mipmap_xxhdpi_appicon, mipmap_xxxhdpi_appicon [EXTRACTED 1.00]
- **Waveform Smooth Rendering Analysis Group** — waveform_attack_plan_doc, waveform_diagnosis_doc, waveform_solution_doc, playerpage_playerpage [INFERRED 0.80]
- **Android App Icons Set** — repeatsegment_droid_resources_mipmap_hdpi_appicon, repeatsegment_droid_resources_mipmap_hdpi_appicon_background, repeatsegment_droid_resources_mipmap_hdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_mdpi_appicon, repeatsegment_droid_resources_mipmap_mdpi_appicon_background, repeatsegment_droid_resources_mipmap_mdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xhdpi_appicon, repeatsegment_droid_resources_mipmap_xhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxhdpi_appicon_foreground, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_background, repeatsegment_droid_resources_mipmap_xxxhdpi_appicon_foreground, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher, repeatsegment_maui_platforms_android_resources_mipmap_xxxhdpi_ic_launcher_round, repeatsegment_maui_resources_appicon_appicon [INFERRED 0.85]

## Communities (53 total, 34 thin omitted)

### Community 0 - "PlayerPage Audio Controls"
Cohesion: 0.06
Nodes (21): AudioManager, double, EventArgs, Handler, IDispatcherTimer, l, PlayerPage, r (+13 more)

### Community 1 - "Audio Engine Decoding"
Cohesion: 0.13
Nodes (7): AudioTrack, DecodeResult, float, IDisposable, long, AudioEngine, short

### Community 2 - "Project Context & Dependencies"
Cohesion: 0.06
Nodes (31): Foreground Service + MediaSession: РАБОТАЕТ, RepeatSegment Android — Итоговый контекст для нового чата, Баг 1: Play с начала файла пропускает первый сегмент, Баг 2: Непрерывное проигрывание пропускает сегмент без звука, Важные конфигурации csproj, Документы, Инструменты, ИСПРАВЛЕНО: Краш Debug-сборки при adb install (+23 more)

### Community 4 - "MAUI App Entry Points"
Cohesion: 0.09
Nodes (10): MainApplication, AppDelegate, AppDelegate, MauiApp, MauiApplication, MauiUIApplicationDelegate, MauiWinUIApplication, MauiProgram (+2 more)

### Community 5 - "Audio Focus Management"
Cohesion: 0.07
Nodes (24): AudioFocus, Build Log (Release), Callback, Context, IOnAudioFocusChangeListener, MAUI Android Debug Notes, Microsoft.Data.Sqlite (9.0.*), Microsoft.Maui.Controls (9.0.*) (+16 more)

### Community 6 - "Playback Service & Notifications"
Cohesion: 0.13
Nodes (11): AudioFocusRequestClass, bool, IBinder, int, Intent, MediaSession, Notification, Service (+3 more)

### Community 7 - "Settings Page Configuration"
Cohesion: 0.10
Nodes (9): CheckedChangedEventArgs, ConfigManager, ContentPage, List, SettingsPage, Porting Plan WPF → MAUI Android, MainPage, RecentManager (+1 more)

### Community 8 - "Waveform Drawing (SkiaSharp)"
Cohesion: 0.30
Nodes (3): SKCanvas, SKColor, SKPaintSurfaceEventArgs

### Community 9 - "Logging and Recent Files"
Cohesion: 0.08
Nodes (24): 1. Простая замена `DateTime.Now` → `Stopwatch` без других изменений, 2. Нативный Android Handler с точным расписанием, 3. SKGLSurfaceView вместо SKCanvasView, 4. MAUI GraphicsView + IDrawable (нативный MAUI рендеринг), 5. Чисто нативный Android View + Canvas.OnDraw (без SkiaSharp и MAUI), Waveform Smooth Rendering — Полный пересмотр и план атаки, Корневая гипотеза, План атаки: 5 непроверенных подходов (+16 more)

### Community 10 - "Android Activity Lifecycle"
Cohesion: 0.29
Nodes (4): Activity, Bundle, MainActivity, MainActivity

### Community 11 - "MAUI App Window"
Cohesion: 0.33
Nodes (4): Application, IActivationState, App, Window

### Community 20 - "WPF Audio Engine"
Cohesion: 0.17
Nodes (9): Action, NativeMagnifier, NativeTouch, TL, BoxView, IOnTouchListener, Label, MotionEvent (+1 more)

### Community 45 - "WPF Silence Detector"
Cohesion: 0.12
Nodes (14): 10. .NET 9 SDK — не на PATH по умолчанию, 11. Android SDK и JDK — ручное указание при сборке из командной строки, 1. SkiaSharp: несовместимость версий с MAUI, 2. 16KB Page Alignment (Android 15), 3. Debug vs Release сборки, 5. AudioTrack.Builder fluent chains → варнинги, 6. Shared-файлы: зависимость от System.AppDomain, 7. TranscriptionProvider.cs: nullable warnings (+6 more)

### Community 46 - "Community 46"
Cohesion: 0.12
Nodes (15): XAML, Баг 1 — Лупа: реалистичное изображение с ×1.5 увеличением, Баг 2 — Лупа перекрывается TranslationPanel, Баг 3 — Выделение влево/вверх не работает, Детали реализации в ShowLoupe, Исправленный план исправления трёх багов (с учётом замечаний), Корень, Корень (+7 more)

### Community 47 - "Community 47"
Cohesion: 0.12
Nodes (15): Architecture: Shared Files, Critical Technical Notes, csproj конфигурация, Git, Key Components Status, Known Warnings (2, не критичны), Next Steps (Priority Order), NOT YET IMPLEMENTED (+7 more)

### Community 48 - "Community 48"
Cohesion: 0.12
Nodes (15): 1. `DateTime.Now` для позиции, 2. `System.Threading.Timer` + `Dispatcher.Dispatch`, 3. `IDispatcherTimer` с тяжёлым callback, 4. `new SKPaint` на каждом кадре, 5. Кеширование `SKBitmap` всей сцены, 6. Троттлинг `InvalidateSurface`, Waveform Smooth Rendering — Правильное решение и анти-паттерны, Итоговая архитектура (+7 more)

### Community 49 - "Community 49"
Cohesion: 0.13
Nodes (14): Phase 2 из WPF-плана, Phase 3 из WPF-плана, RepeatSegment — План портирования WPF → MAUI Android, Баг 1: Play с начала файла пропускает первый сегмент, Баг 2: Непрерывное проигрывание пропускает сегмент без звука, Баги, исправленные 29.06.2026, План дальнейшего портирования (4 фазы), Текущее состояние Phase 1 (MVP) из WPF-плана (+6 more)

### Community 50 - "Community 50"
Cohesion: 0.40
Nodes (4): 1. Запуск эмулятора (Android Studio), 2. Запуск приложения (Visual Studio 2022), Как запустить RepeatSegment на Android-эмуляторе, Что видно прямо сейчас

## Knowledge Gaps
- **146 isolated node(s):** `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android`, `Microsoft.Maui.Controls (9.0.*)`, `Microsoft.Maui.Controls.Compatibility (9.0.*)` (+141 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **34 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerPage` connect `PlayerPage Audio Controls` to `Audio Engine Decoding`, `Menu Page Navigation`, `Audio Focus Management`, `Playback Service & Notifications`, `Settings Page Configuration`, `Waveform Drawing (SkiaSharp)`?**
  _High betweenness centrality (0.194) - this node is a cross-community bridge._
- **Why does `MAUI Android Debug Notes` connect `Audio Focus Management` to `MAUI App Entry Points`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Why does `AudioEngine` connect `Audio Engine Decoding` to `PlayerPage Audio Controls`, `Audio Focus Management`, `Playback Service & Notifications`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **What connects `net9.0-android`, `Microsoft.NET.Sdk`, `net9.0-android` to the rest of the system?**
  _146 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PlayerPage Audio Controls` be split into smaller, more focused modules?**
  _Cohesion score 0.05719298245614035 - nodes in this community are weakly interconnected._
- **Should `Audio Engine Decoding` be split into smaller, more focused modules?**
  _Cohesion score 0.12688172043010754 - nodes in this community are weakly interconnected._
- **Should `Project Context & Dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.06451612903225806 - nodes in this community are weakly interconnected._