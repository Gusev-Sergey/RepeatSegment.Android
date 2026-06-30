# RepeatSegment Android — Итоговый контекст для нового чата

## Состояние проекта (29.06.2026, 22:52 YEKT)

Проект портирован из WPF (.NET 8) на MAUI Android (.NET 9). Код компилируется с 0 ошибок. APK собирается и запускается на физическом устройстве (iQOO Neo9S Pro+, Android 15, SDK 35). Приложение запускается без краша (Release-сборка).

### Структура проекта
- **Основной проект**: [`RepeatSegment.Maui/`](RepeatSegment.Maui/) — MAUI Android
- **Бизнес-логика**: 8 shared .cs файлов через `<Link>` из WPF-проекта `c:\ProjectsCSharp\RepeatSegment\RepeatSegment.App\`
- **Сборка**: [`build_release.ps1`](RepeatSegment.Maui/build_release.ps1) — использует `C:\Program Files\dotnet\dotnet.exe` (.NET 9 SDK)

### Команда сборки и установки (актуальная!)
```powershell
# Сборка Release (полный путь к .NET 9 + Android SDK/JDK)
cmd /c "cd C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui && "C:\Program Files\dotnet\dotnet.exe" build RepeatSegment.Maui.csproj -c Release -f net9.0-android -p:AndroidSdkDirectory=%LOCALAPPDATA%\Android\Sdk -p:JavaSdkDirectory="C:\Program Files\Android\Android Studio\jbr""

# Установка на телефон (с -g для автоподтверждения разрешений)
adb install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"

# Запуск
adb shell monkey -p com.astrorumarbor.repeatsegment -c android.intent.category.LAUNCHER 1

# Логи ошибок
adb logcat -d -s AndroidRuntime:E *:F | findstr /i repeat
```

### Важные конфигурации csproj
```xml
<TargetFrameworks>net9.0-android</TargetFrameworks>
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
<AndroidUseAssemblyStore>true</AndroidUseAssemblyStore>
<AndroidStoreUncompressedFileExtensions>.so</AndroidStoreUncompressedFileExtensions>
<AndroidEnable16KPageAlignment>false</AndroidEnable16KPageAlignment>
<PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="4.148.0" />
```

---

## ИСПРАВЛЕНО: Критические баги Play/Stop

### Баг 1: Play с начала файла пропускает первый сегмент
**Причина**: `ButtonsPressed("play")` безусловно делал `_counter++`.
**Исправление**: проверка `atVeryStart` в [`PlayerPage.xaml.cs:65`](RepeatSegment.Maui/Pages/PlayerPage.xaml.cs:65).

### Баг 2: Непрерывное проигрывание пропускает сегмент без звука
**Причина 1**: `AudioTrack.Write()` в `Task.Run` — гонка Play/Write.
**Причина 2**: `GetPlaySamplesToEnd()` выделял гигабайты → OOM.
**Исправление**: Write синхронный ДО Play + всегда `GetPlaySamples(_t1, _t2)`.

---

## ИСПРАВЛЕНО: Краш Foreground Service (MissingForegroundServiceTypeException)

**Причина**: Android 14+ (SDK 34+) требует `foregroundServiceType` при вызове `startForeground()`. Без него — `MissingForegroundServiceTypeException`.

**Исправление в [`PlaybackService.cs`](RepeatSegment.Maui/Services/PlaybackService.cs)**:
1. Атрибут: `[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaPlayback)]`
2. В `OnCreate()`: `StartForeground(NOTIFY_ID, notification, (ForegroundService)1)` для SDK 34+
3. Добавлен `using Android.Content.PM;`

---

## ИСПРАВЛЕНО: Краш Debug-сборки при adb install

**Причина**: Debug сборки используют Fast Deployment (каталог `.__override__/arm64-v8a`), которого нет на устройстве при обычной установке.
**Исправление**: ВСЕГДА собирать и устанавливать **Release** для тестирования через `adb install`.

---

## РЕШЕНО: Плавность waveform (29.06.2026, 17 попыток)

### Симптомы
Курсор и аудиоволна двигались рывками на мощном устройстве (iQOO Neo9S Pro+, 120Hz). Без транскрипции — терпимо. С транскрипцией — сильно заметно.

### Корневая причина
Модификация `FormattedString.Spans[idx].BackgroundColor` (подсветка произносимого слова) заставляла MAUI перестраивать лейаут Label'а с сотнями span'ов, блокируя главный Looper на 50-150ms и задерживая рендер-коллбек.

### Правильное решение (v6)
**Строить полностью новый `FormattedString` на фоновом потоке (`System.Threading.Timer`), на главный — ОДНО присваивание `LblTranscription.FormattedText = ...`.**

Полная архитектура:
```
Handler.PostDelayed 16ms (главный Looper)
  └─ WaveformCanvas.InvalidateSurface() → OnWaveformPaint
       ├─ fresh NanoTime() → позиция (в момент отрисовки)
       ├─ static readonly краски × 9 (0 аллокаций)
       ├─ static readonly SKPathEffect (0 аллокаций)
       └─ ScheduleRender() — самоподдерживающийся цикл

IDispatcherTimer UiTick 100ms
  └─ Слайдер + лейбл + границы сегмента

System.Threading.Timer HlWordBg 500ms (фоновый поток)
  ├─ Бинарный поиск индекса слова
  ├─ Построение полного FormattedString (3 span'а)
  └─ MainThread.BeginInvokeOnMainThread(() => LblTranscription.FormattedText = fmt)

IDispatcherTimer ScrollTimer 700ms
  └─ ScWord() — скролл транскрипции
```

### Что НЕ сработало (16 попыток)
1. `IDispatcherTimer` + `DateTime.Now` — гранулярность ~10-16ms
2. `System.Threading.Timer` + `Stopwatch` + `Dispatch` — задержка dispatch
3. Троттлинг InvalidateSurface — усугубило
4. `SKPicture` кеш — ломает offset
5. Choreographer — десинхронизация
6. `GraphicsView + IDrawable` — Translate не работает
7. `SKGLSurfaceView` (OpenGL) — рывки остались
8. Пред-рендеренный `SKBitmap` + блит — рывки остались
9. `static readonly` краски — GC не причина
10. `IDispatcherTimer` для подсветки (главный поток) — блокирует рендер
11. `System.Threading.Timer` + 6 модификаций Span свойств через MainThread — всё равно re-layout
12. `Handler.PostDelayed` 16ms — помог, но не решил до конца
13. Fresh `NanoTime()` в `OnWaveformPaint` — помог, но не решил
14. Троттлинг слайдера/лейбла — помог, но не решил
15. `static readonly SKPathEffect` — помог, но не решил
16. `ScWord` на отдельном таймере — помог, но не решил

### Ключевое наблюдение (решающее)
**Без загруженной транскрипции анимация плавная. С транскрипцией — рывки.** Это указало, что проблема в подсветке слов, а не в рендере waveform.

### Документы
- [`WAVEFORM_DIAGNOSIS.md`](WAVEFORM_DIAGNOSIS.md) — история попыток
- [`WAVEFORM_SOLUTION.md`](WAVEFORM_SOLUTION.md) — правильные и неправильные подходы
- [`WAVEFORM_ATTACK_PLAN.md`](WAVEFORM_ATTACK_PLAN.md) — план атаки

---

## РЕШЕНО: Скролл транскрипции с центрированием подсветки (30.06.2026)

### Симптомы
Подсвеченное слово уходило вверх за пределы экрана.

### Причина
Ручной расчёт ширины символов (`cw`) и переносов строк не совпадал с реальным MAUI-рендером.

### Правильное решение
**Пропорциональная позиция**: `(charIndex / totalChars) * LblTranscription.Height`. Использует точную высоту Label, уже вычисленную MAUI.

- `_prevScrollOffset` с гистерезисом 3px
- Плавный скролл `animated: true`
- Не требует `cw`, `textW`, `_wordLineNumbers`

### Попытки (не сработали)
1. `cw=9.8`, `textW=dispW-48`, `_lineHeight=22` — недоскролл
2. `cw=10.5`, `textW=dispW-56`, `_lineHeight=24` — недоскролл
3. `Android.Text.StaticLayout` — неправильные строки
4. `_scrollCalibrator` каждые 5s — неэффективен
5. `_prevScrollLine` guard — позиции строк неверны

---

## Foreground Service + MediaSession: РАБОТАЕТ

Исправлены краши `MissingForegroundServiceTypeException` и `foregroundServiceType 0x01 != 0x02`. Сервис запускается, уведомление показывается, кнопки MediaSession (play/pause/next/prev) связаны через `PlaybackBridge`.

Итоговое исправление в [`PlaybackService.cs`](RepeatSegment.Maui/Services/PlaybackService.cs):
1. Атрибут: `[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaPlayback)]`
2. В `OnCreate()`: `StartForeground(NOTIFY_ID, notification, Android.Content.PM.ForegroundService.TypeMediaPlayback)` (SDK 34+)
3. `using Android.Content.PM;`

---

## НЕ СДЕЛАНЫ: Задачи из PLAN_PORTING.md

- Audio Focus
- TranslationProvider интеграция
- AnkiCardPage
- TtsProvider кнопка
- AnkiExportManager
- Локализация UI

---

## Инструменты
- .NET 9: `C:\Program Files\dotnet\dotnet.exe` (SDK 9.0.304)
- .NET 8: `C:\Program Files (x86)\dotnet\sdk\8.0.421`
- Android SDK: `%LOCALAPPDATA%\Android\Sdk`
- JDK: `C:\Program Files\Android\Android Studio\jbr`
