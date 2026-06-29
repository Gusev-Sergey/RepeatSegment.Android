# RepeatSegment — Resume for New Chat (June 29, 2026)

## Project Overview

RepeatSegment — приложение для изучения языков через аудиокниги. WPF-версия (Windows) портируется на Android через .NET MAUI.

**Текущий статус:** MAUI Android приложение работает (Release сборка), базовый функционал портирован.

## Repository Structure

```
c:/ProjectsCSharp/
├── RepeatSegment/                    # WPF проект (.NET 8) + MAUI shared
│   ├── RepeatSegment.App/           # WPF UI + все бизнес-логика
│   └── RepeatSegment.Tests/
├── RepeatSegment.Android/           # Android проект
│   └── RepeatSegment.Maui/          # MAUI Android приложение (.NET 9)
│       ├── Pages/                   # UI страницы (Player, Menu, Settings)
│       ├── Services/                # AudioEngine, RecentManager
│       ├── Platforms/Android/       # Android-specific
│       └── Resources/               # Иконки, шрифты, стили
```

## Architecture: Shared Files

MAUI проект компилирует выбранные файлы из WPF проекта напрямую:
```xml
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\SilenceDetector.cs" Link="Shared\SilenceDetector.cs" />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\TranscriptionProvider.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\TranslationProvider.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\TtsProvider.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\ConfigManager.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\Strings.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\AnkiBuilder.cs" ... />
<Compile Include="..\..\RepeatSegment\RepeatSegment.App\AnkiExportManager.cs" ... />
```

## Key Components Status

| Компонент | Файл | Статус |
|-----------|------|--------|
| Audio Engine (Android) | `Services/AudioEngine.cs` | ✅ Работает (MediaExtractor + AudioTrack) |
| Silence Detector | `Shared/SilenceDetector.cs` | ✅ Переиспользован из WPF |
| Waveform (SkiaSharp) | `Pages/PlayerPage.xaml + .cs` | ✅ SKCanvasView рендерит |
| Buttons (Play/PlayGo/Repeat) | `Pages/PlayerPage.xaml.cs:ButtonsPressed` | ✅ Портирована WPF логика |
| Transcription Provider | `Shared/TranscriptionProvider.cs` | ✅ Deepgram API |
| Translation Provider | `Shared/TranslationProvider.cs` | ✅ Google/Yandex |
| Config Manager | `Shared/ConfigManager.cs` | ✅ INI-файлы |
| Menu Page | `Pages/MenuPage.xaml + .cs` | ✅ С галочками |
| Settings Page | `Pages/SettingsPage.xaml + .cs` | ✅ API ключи, язык, битрейт |
| Recent Files | `Services/RecentManager.cs` | ✅ Статический, persistent |

## NOT YET IMPLEMENTED
- Translation UI (выделение текста → перевод → панель)
- TTS + Anki Export
- i18n (Strings.cs загрузка JSON языков)
- Background playback

## Critical Technical Notes

### Сборка — ТОЛЬКО Release для тестирования!
```
dotnet build -c Release -f net9.0-android \
  -p:AndroidSdkDirectory="%LOCALAPPDATA%\Android\Sdk" \
  -p:JavaSdkDirectory="C:\Program Files\Android\Android Studio\jbr"
```
Debug сборка ожидает Fast Deployment (директория `.__override__`) и крашится при `adb install -r`.

### csproj конфигурация
```xml
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
<AndroidUseAssemblyStore>true</AndroidUseAssemblyStore>
<AndroidStoreUncompressedFileExtensions>.so</AndroidStoreUncompressedFileExtensions>
<AndroidEnable16KPageAlignment>false</AndroidEnable16KPageAlignment>
```

### NuGet пакеты
```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="9.0.*" />
<PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="4.148.0" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.*" />
```

### Телефон
- **Модель:** iQOO Neo9S Pro+
- **Android:** 15 (SDK 35)
- **ADB:** `C:\Users\gsa40\AppData\Local\Microsoft\WinGet\Packages\Genymobile.scrcpy_...\scrcpy-win64-v4.0\adb.exe`
- **SCRCPY:** для управления с ПК

### Полезные команды
```bash
# Установка и запуск
adb install -r bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk
adb shell monkey -p com.astrorumarbor.repeatsegment -c android.intent.category.LAUNCHER 1

# Просмотр крашей
adb logcat -d *:F | findstr repeatsegment

# Полная очистка
adb uninstall com.astrorumarbor.repeatsegment
```

## Known Warnings (2, не критичны)
- CS0618: `SKPath.MoveTo/LineTo` deprecated → использовать `SKPathBuilder` (PlayerPage.xaml.cs:601-602)

## Next Steps (Priority Order)
1. Исправить 2 deprecated варнинга (SKPath → SKPathBuilder)
2. Translation UI: тап по слову → перевод → панель + кнопка Anki (WPF модель)
3. TTS + Anki Export
4. i18n (Strings.cs + переключение языка)
5. build_maui_debug.ps1 → переименовать/обновить для Release

## Git
- WPF проект в `c:/ProjectsCSharp/RepeatSegment/.git`
- Android проект также в этом репозитории
- Рабочая версия закоммичена
