# Waveform Smooth Rendering — Перечень попыток, диагноз и план

## Проблема

Вертикальная линия курсора и аудиоволна движутся рывками на Android-устройстве. Скачки ~2-4 пикселя за кадр одинаково затрагивают простейшую `DrawLine` и сложный `DrawPath` waveform data. Это указывает на **системную** проблему, а не вычислительную.

## Список попыток и причин неудач

| # | Подход | Причина неудачи |
|---|--------|-----------------|
| 1 | `IDispatcherTimer` 33ms + `DateTime.Now` (оригинал) | `DateTime.Now` на Mono Android ~10-16ms гранулярность; таймер на UI-потоке — Tick задерживается рендером |
| 2 | `System.Threading.Timer` 16ms + `Stopwatch` + `Dispatcher.Dispatch` | Добавляет ~1-3 кадра задержки между вычислением позиции и `InvalidateSurface` |
| 3 | Троттлинг `InvalidateSurface` (каждый 3-й Tick) | Усугубляет: из 30fps с джиттером → 10fps с большими скачками |
| 4 | `SKPicture` кеш waveform data на GPU | Waveform data зафиксирована в координатах полного файла — при сдвиге окна `DrawPicture(offset)` работает некорректно для широких файлов |
| 5 | Раздельные таймеры: позиция 33ms + подсветка 100ms | Не влияет на рывки — проблема не в нагрузке коллбека, а в источнике сигнала |
| 6 | Самоподдерживающийся цикл: `InvalidateSurface` в конце `OnWaveformPaint` | `InvalidateSurface` не гарантирует немедленного вызова `PaintSurface` — очередь отрисовки добавляет лаг |
| 7 | `IsAntialias = true` на курсоре | Субпиксельное сглаживание не решает проблему джиттера источника позиции |
| 8 | `static readonly SKPaint` для всех красок | Убирает GC-паузы, но рывки остаются — источник не GC |
| 9 | Возврат к оригинальному коду | Рывки были изначально, просто не так заметны на первых версиях |

## Корневой диагноз

**Источник проблемы — `IDispatcherTimer` + `InvalidateSurface` не синхронизированы с VSYNC дисплея.**

В Android правильный путь для плавной 60fps анимации — `Choreographer.FrameCallback`. Он вызывается строго перед каждым VSYNC кадром. `IDispatcherTimer` работает на UI-цикле сообщений, но НЕ привязан к частоте обновления дисплея — между Tick'ом и фактическим `OnPaintSurface` проходит произвольное время (0-48ms), из-за чего позиция устаревает.

В нативном Android SkiaSharp `SKCanvasView.IgnorePixelScaling` + внутренний `Choreographer` обеспечивают плавность. Но в MAUI обёртка `SkiaSharp.Views.Maui.Controls.SKCanvasView` создаёт Android-виджет через handler, и не очевидно, синхронизирован ли он с `Choreographer`.

Альтернативно: MAUI имеет встроенный `IDispatcher.CreateTimer()` — он тоже не привязан к VSYNC.

## Перспективные подходы (не испробованы)

### Приоритет 1: Choreographer напрямую
```csharp
var choreographer = Android.Views.Choreographer.Instance;
choreographer.PostFrameCallback(new FrameCallback(_playStopwatch));
```
Плюс: гарантированная VSYNC-синхронизация.
Минус: требует `Android.Views` зависимости, callback вне MAUI-контекста → нужен `MainThread.BeginInvokeOnMainThread`.

### Приоритет 2: SKGLSurfaceView вместо SKCanvasView
`SKGLSurfaceView` использует OpenGL ES напрямую — гарантированно аппаратное ускорение и VSYNC.
Минус: другой API, не совместим с MAUI XAML напрямую, нужен custom handler.

### Приоритет 3: Custom MAUI handler с android.widget.TextureView
Обойти MAUI-обёртку SkiaSharp, использовать `TextureView` + `SKSurface` напрямую с `Choreographer`.
Плюс: полный контроль над pipeline.
Минус: много кода.

### Приоритет 4: Установить частоту обновления экрана в коде
```csharp
var activity = Platform.CurrentActivity;
activity.Window?.Attributes?.PreferredRefreshRate = 60;
```
И проверить, не работает ли дисплей на 120Hz с несовместимым таймером.

### Приоритет 5: Platform-specific renderer с native SKCanvasView
Использовать `#if ANDROID` для создания native `SkiaSharp.Views.Android.SKCanvasView` вместо MAUI-обёртки, в котором `InvalidateSurface()` синхронизирован с `Choreographer`.

## План действий

1. Изучить `SkiaSharp.Views.Android.SKCanvasView` на предмет `Choreographer` — проверить исходники
2. Проверить `PreferredRefreshRate` дисплея на устройстве
3. Если native SKCanvasView использует Choreographer — создать platform-specific renderer
4. Если нет — написать свой `FrameCallback` → `InvalidateSurface` мост
5. Задокументировать рабочее решение
