# Waveform Smooth Rendering — Полный пересмотр и план атаки

## Текущее состояние

После 10+ попыток исправления рывков аудиоволны, ни одно решение не дало приемлемого результата. Проблема: на мощном Android-устройстве (iQOO Neo9S Pro+, Android 15) аудиоволна и курсор движутся рывками, хотя игры на этом же устройстве летают на 120fps.

## Что точно НЕ является проблемой

| Фактор | Почему исключён |
|--------|-----------------|
| Мощность GPU | Игры на 120fps работают идеально |
| Сложность отрисовки | Даже ОДНА линия (DrawLine) дёргается |
| GC-паузы | Static readonly SKPaint исключает аллокации, но рывки остаются |
| Количество объектов на экране | Waveform data — самый тяжёлый элемент, но без него линия всё равно дёргается |

## Корневая гипотеза

**`IDispatcherTimer` + `DateTime.Now` на Mono Android дают неравномерные интервалы между кадрами.**

- `DateTime.Now` на Mono Android использует `gettimeofday()` — разрешение ~10-16ms
- `IDispatcherTimer` на MAUI Android использует `Handler.PostDelayed` — ненадёжная доставка (+5-20ms джиттер)
- При скорости движения ~53 px/sec на 800px экране разница между 20ms и 50ms интервалами = 1.06px vs 2.65px — видимый скачок

## Что уже пробовали (10 попыток)

Подробно в [`WAVEFORM_DIAGNOSIS.md`](WAVEFORM_DIAGNOSIS.md). Кратко:
1. `IDispatcherTimer` + `DateTime.Now` — оригинал, рывки
2. `System.Threading.Timer` + `Stopwatch` + `Dispatcher.Dispatch` — задержка dispatch
3. Троттлинг `InvalidateSurface` — усугубило
4. `SKPicture` GPU-кеш — ломает offset при скролле
5. Раздельные таймеры — не влияет на источник
6. Самоподдерживающийся цикл через `InvalidateSurface` в конце `PaintSurface` — `InvalidateSurface` не мгновенный
7. `IsAntialias = true` — сглаживание не решает проблему джиттера источника
8. `static readonly` краски — убирает GC, не влияет на рывки
9. `Choreographer.FrameCallback` — стало хуже
10. `StretchNaive` вместо `StretchSola` — для теста, не влияет на рендер

## План атаки: 5 непроверенных подходов

### 1. Простая замена `DateTime.Now` → `Stopwatch` без других изменений

**Гипотеза**: источник неточности — `DateTime.Now` (10-16ms разрешение). `Stopwatch` использует `clock_gettime(CLOCK_MONOTONIC)` с наносекундной точностью.

**Что меняем**: ТОЛЬКО `DateTime.Now` → `Stopwatch.Elapsed`. Всё остальное как в оригинале.

**Риск**: низкий. Изменение минимально, легко откатить.

### 2. Нативный Android Handler с точным расписанием

**Гипотеза**: `IDispatcherTimer` на MAUI — обёртка над `Handler.PostDelayed`, которая добавляет джиттер. Прямой `Android.OS.Handler` с `PostAtTime` точнее.

```csharp
var handler = new Handler(Looper.MainLooper);
handler.PostAtTime(() => { /* render */ }, SystemClock.UptimeMillis() + 16);
```

**Риск**: средний. Требует ручного управления циклом.

### 3. SKGLSurfaceView вместо SKCanvasView

**Гипотеза**: `SKCanvasView` рендерит в software, без аппаратного ускорения. `SKGLSurfaceView` использует OpenGL ES напрямую → GPU-ускоренный рендер с VSYNC.

**Что меняем**: `SKCanvasView` → `SKGLSurfaceView`. API почти совместим.

**Риск**: средний. Может потребоваться custom MAUI handler.

### 4. MAUI GraphicsView + IDrawable (нативный MAUI рендеринг)

**Гипотеза**: `Microsoft.Maui.Graphics.GraphicsView` с `IDrawable` — нативный MAUI способ 2D-рендеринга, оптимизированный для платформы. Использует `Microsoft.Maui.Graphics.ICanvas` (не SkiaSharp).

**Что меняем**: полная замена SkiaSharp на MAUI Graphics.

**Риск**: высокий. Много кода для замены. Waveform data — path drawing.

### 5. Чисто нативный Android View + Canvas.OnDraw (без SkiaSharp и MAUI)

**Гипотеза**: проблемы на уровне MAUI → SkiaSharp → Android bridge. Нативный `Android.Views.View` с `OnDraw(Canvas)` — минимальный стек.

**Что меняем**: встраиваем `Android.Views.View` через custom handler, рисуем напрямую через `Android.Graphics.Canvas.DrawLine/DrawPath`.

**Риск**: очень высокий. Полный редизайн UI-компонента.

### Приоритетный порядок тестирования

1. **Stopwatch вместо DateTime.Now** (минимум изменений)
2. **SKGLSurfaceView** (максимальный эффект за средние усилия)
3. **Нативный Handler** (если п.1-2 не помогли)
4. **MAUI GraphicsView** (если SkiaSharp — корень проблемы)
5. **Нативный View + Canvas** (последний рубеж)

## Следующий шаг

Реализовать п.1 — простая замена `DateTime.Now` → `Stopwatch`.
Если не поможет — п.2 `SKGLSurfaceView`.
