# RepeatSegment — Resume for New Chat (June 30, 2026, 23:50 YEKT)

## Project Status
MAUI Android приложение работает. Ключевой функционал выделения текста и лупы реализован и стабилен.

## Architecture Overview
- **Проект**: [`RepeatSegment.Maui/`](RepeatSegment.Maui/) — MAUI Android (.NET 9)
- **Бизнес-логика**: 8 shared .cs файлов из WPF проекта
- **UI**: PlayerPage со всем функционалом в одном файле

## Key Files (Current State)

| File | Purpose |
|------|---------|
| [`PlayerPage.xaml`](RepeatSegment.Maui/Pages/PlayerPage.xaml) | UI: waveform, кнопки, транскрипция, лупа, панель перевода |
| [`PlayerPage.xaml.cs`](RepeatSegment.Maui/Pages/PlayerPage.xaml.cs) | Вся логика: playback, транскрипция, выделение, лупа, перевод |
| [`NativeTouch.cs`](RepeatSegment.Maui/Platforms/Android/NativeTouch.cs) | Нативный Android TouchListener для drag-выделения |
| [`NativeMagnifier.cs`](RepeatSegment.Maui/Platforms/Android/NativeMagnifier.cs) | Скриншот TextView для лупы с ×1.3 увеличением |
| [`AudioEngine.cs`](RepeatSegment.Maui/Services/AudioEngine.cs) | MediaExtractor + AudioTrack |

## Selection System (Current)
- **Character-level**: `_selStartChar`, `_selEndChar` — индексы символов
- **Anchor-based**: `_selAnchorChar` фиксируется при Down, диапазон расширяется от якоря
- **Highlights**: синий (`#2A5A9B`) для выделения, золотой (`#FFD700`) для проигрывания
- **Loupe**: 180×180 pixel круглый Border + NativeMagnifier.Capture с ×1.3 zoom
- **Position**: лупа над пальцем, не перекрывает TranslationPanel

## Translation
- `TranslationProvider.TranslateEnRu(text)` — Google Translate (бесплатный) + Yandex fallback
- API ключи в ConfigManager (`YandexTranslateApiKey`, `TranslationProviderPreference`)

## Build Commands
```powershell
# Сборка Release
powershell -ExecutionPolicy Bypass -File "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\build_release.ps1"

# Установка
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"

# Запуск
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe shell monkey -p com.astrorumarbor.repeatsegment -c android.intent.category.LAUNCHER 1
```

## TODO (Next Priorities)
1. Переключение режима выделения: по буквам / по словам
2. Pinch-to-zoom для изменения шрифта транскрипции
3. AnkiCardPage
4. Локализация UI (из WPF lang/*.json)

## Bug Log
See [`BUGFIND_LOG.md`](BUGFIND_LOG.md) for detailed bug history and root causes.
