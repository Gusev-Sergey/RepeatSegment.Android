# RepeatSegment Android — Итоговый контекст (30.06.2026, 23:50 YEKT)

## Состояние проекта
Проект портирован из WPF (.NET 8) на MAUI Android (.NET 9). Полностью работоспособен.

### Ключевой новый функционал (реализован 30.06.2026)
- **Выделение текста per-character** через нативный TouchListener с `_selAnchorChar` (якорная модель)
- **Лупа** — скриншот нативного TextView через `NativeMagnifier.Capture()` с ×1.3 увеличением
- **Круглый Border** с бирюзовым контуром, позиционируется над пальцем
- **Подсветка**: синий (`#2A5A9B`) для выделения, золотой (`#FFD700`) для проигрывания
- **Сосуществование подсветок** через посимвольный state-machine в `RebuildTranscriptSpans`

### Архитектура лупы
- `LoupeOverlay` с `Grid.RowSpan="9"` — позиционируется поверх всей страницы
- `ShowLoupe()` переводит px координаты в dips, добавляет `TransOverlay.Y` офсет
- `NativeMagnifier.Capture(pxX, pxY, capturePx)` делает скриншот через `View.Draw(Canvas)`
- Кеширование: плотность (`_density`), размер захвата (`_loupeCapturePx`), последний bitmap с dead-zone 20px

### Баги, исправленные 30.06 (подробнее в BUGFIND_LOG.md)
1. TapGestureRecognizer → нативный TouchListener + pointer coords
2. Неправильное позиционирование лупы (отсутствие TransOverlay.Y офсета)
3. Некорректное увеличение (пиксели захвата vs dips отображения)
4. Выделение влево/вверх (перезапись _selStartChar в Move)
5. Краш во время проигрывания (UI с фонового потока)
6. Лупа дергалась (избыточный Capture на каждом Move)
7. Bold сдвигал строки при выделении

## Структура
```
RepeatSegment.Android/
├── RepeatSegment.Maui/          # MAUI Android приложение
│   ├── Pages/
│   │   ├── PlayerPage.xaml      # Весь UI
│   │   └── PlayerPage.xaml.cs   # Вся логика
│   ├── Platforms/Android/
│   │   ├── NativeTouch.cs       # Нативный TouchListener
│   │   └── NativeMagnifier.cs   # Скриншот для лупы
│   ├── Services/
│   │   ├── AudioEngine.cs       # Android аудио
│   │   ├── PlaybackService.cs   # Foreground service
│   │   └── RecentManager.cs
│   └── build_release.ps1        # Скрипт сборки
├── plans/
│   └── selection_bugs_root_cause.md
├── BUGFIND_LOG.md               # Лог багов и root cause analysis
├── RESUME_FOR_NEW_CHAT.md       # Контекст для следующего чата
└── PROJECT_CONTEXT.md           # Этот файл
```

## Команды сборки
```powershell
# Release APK
powershell -ExecutionPolicy Bypass -File "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\build_release.ps1"

# Установка
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe install -g -r "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\bin\Release\net9.0-android\com.astrorumarbor.repeatsegment-Signed.apk"

# Запуск
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe shell monkey -p com.astrorumarbor.repeatsegment -c android.intent.category.LAUNCHER 1
```
