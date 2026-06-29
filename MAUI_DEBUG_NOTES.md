# MAUI Android Debug Notes — RepeatSegment

## Критические находки при портировании на Android

### 1. SkiaSharp: несовместимость версий с MAUI
- **SkiaSharp 3.x** (3.116.0) собран для **MAUI 8** (`net8.0-android34.0`)
- **SkiaSharp 4.x** (4.148.0) собран для **MAUI 9** (`net9.0-android`)
- Наш проект: MAUI 9 (`net9.0-android`) → нужна **SkiaSharp 4.x**
- Симптом: SIGABRT в `libmonosgen-2.0.so` при использовании 3.x в Release сборке

### 2. 16KB Page Alignment (Android 15)
- **iQOO Neo9S Pro+** (Android 15/SDK 35) требует 16KB page alignment для `.so` файлов
- `libSkiaSharp.so` не выровнен → SIGABRT
- Решение: `<AndroidEnable16KPageAlignment>false</AndroidEnable16KPageAlignment>` в csproj
- Дополнительно: `<AndroidStoreUncompressedFileExtensions>.so</AndroidStoreUncompressedFileExtensions>` чтобы .so не сжимались

### 3. Debug vs Release сборки
- **Debug** сборка ожидает Fast Deployment (каталог `.__override__/arm64-v8a`)
- При `adb install -r` Debug-сборка крашится: "No assemblies found in `.__override__`"
- **Release** сборка НЕ использует Fast Deployment → работает через `adb install`
- **ВСЕГДА тестировать через `adb install` ТОЛЬКО Release-сборки**

### 4. MauiProgram.cs: UseSkiaSharp()
- `.UseSkiaSharp()` **ОБЯЗАТЕЛЕН** для SKCanvasView
- Без него: `HandlerNotFoundException: Unable to find IElementHandler for SKCanvasView`
- Требует: `using SkiaSharp.Views.Maui.Controls.Hosting`

### 5. AudioTrack.Builder fluent chains → варнинги
- Android-биндинги возвращают `Builder?` из `.SetAudioAttributes()`, `.SetAudioFormat()`
- Решение: разорвать цепочки на отдельные операторы с промежуточными переменными

### 6. Shared-файлы: зависимость от System.AppDomain
- `Strings.cs` использовал `AppDomain.CurrentDomain.BaseDirectory` → stack overflow на Android
- Решение: заменить на `AppContext.BaseDirectory` с try/catch

### 7. TranscriptionProvider.cs: nullable warnings
- `result?.Words?.Count > 0` → нужен `!` на `MergeChunk(result!, ...)`
- `msg.GetString()` → нужен `?? "unknown"`

### 8. Конфигурация csproj для Release APK
```xml
<EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
<AndroidUseAssemblyStore>true</AndroidUseAssemblyStore>
<AndroidStoreUncompressedFileExtensions>.so</AndroidStoreUncompressedFileExtensions>
<AndroidEnable16KPageAlignment>false</AndroidEnable16KPageAlignment>
```

### 9. Foreground Service: MissingForegroundServiceTypeException (Android 14+)
- **Симптом**: `java.lang.RuntimeException: Unable to create service ... PlaybackService: android.app.MissingForegroundServiceTypeException: Starting FGS without a type callerApp=... targetSDK=35`
- **Причина**: Android 14 (SDK 34+) требует явного указания `foregroundServiceType` при вызове `startForeground()`
- **Исправление в [`PlaybackService.cs`](RepeatSegment.Maui/Services/PlaybackService.cs)**:
  1. Атрибут: `[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaPlayback)]`
  2. Вызов: `StartForeground(NOTIFY_ID, notification, (ForegroundService)1)` — каст к int т.к. .NET биндинг может не иметь перегрузки с enum
  3. Требует: `using Android.Content.PM;`

### 10. .NET 9 SDK — не на PATH по умолчанию
- В системе установлены два SDK: 8.0.421 (`C:\Program Files (x86)\dotnet\`) и 9.0.304 (`C:\Program Files\dotnet\`)
- По умолчанию используется 8.0 → ошибка `NETSDK1139: не удалось распознать идентификатор целевой платформы android`
- **Решение**: всегда использовать полный путь `"C:\Program Files\dotnet\dotnet.exe"`

### 11. Android SDK и JDK — ручное указание при сборке из командной строки
- Без параметров сборка падает с `XA5300: Не удалось найти каталог пакета SDK для Android`
- **Решение**: явно передавать `-p:AndroidSdkDirectory=%LOCALAPPDATA%\Android\Sdk -p:JavaSdkDirectory="C:\Program Files\Android\Android Studio\jbr"`

---

## Команды сборки и установки (актуальные!)

```powershell
# Release сборка — полная команда со всеми параметрами
cmd /c "cd C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui && "C:\Program Files\dotnet\dotnet.exe" build RepeatSegment.Maui.csproj -c Release -f net9.0-android -p:AndroidSdkDirectory=%LOCALAPPDATA%\Android\Sdk -p:JavaSdkDirectory="C:\Program Files\Android\Android Studio\jbr""

# Установка на телефон (с автоподтверждением разрешений)
adb install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"

# Запуск
adb shell monkey -p com.astrorumarbor.repeatsegment -c android.intent.category.LAUNCHER 1

# Или одной командой: сборка + установка
powershell -ExecutionPolicy Bypass -File "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\build_release.ps1"
adb install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"
```

---

## Полезные команды диагностики

```bash
# Полная деинсталляция
adb uninstall com.astrorumarbor.repeatsegment

# Просмотр крашей (FATAL + AndroidRuntime errors)
adb logcat -d -s AndroidRuntime:E *:F | findstr /i repeat

# Просмотр всех логов приложения
adb logcat -d | findstr repeatsegment

# Версия на телефоне
adb shell dumpsys package com.astrorumarbor.repeatsegment | findstr versionName
```
