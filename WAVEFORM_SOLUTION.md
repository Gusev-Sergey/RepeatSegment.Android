# Waveform Smooth Rendering — Правильное решение и анти-паттерны

## Проблема

Визуальная отрисовка аудиоволны (waveform) и курсора воспроизведения двигалась рывками. Рывки одинаково затрагивали как сложную waveform data, так и простую вертикальную линию (один `DrawLine`), что указывало на системную, а не вычислительную проблему.

После 15+ итераций найден правильный подход.

---

## ❌ Что НЕ работает (анти-паттерны)

### 1. `DateTime.Now` для позиции
```
Проблема: гранулярность ~10-16ms на Android Mono (gettimeofday)
Симптом: видимые скачки позиции при интервале таймера 33ms
```

### 2. `System.Threading.Timer` + `Dispatcher.Dispatch`
```
Проблема: добавляет 1-3 кадра задержки между вычислением позиции и отрисовкой
Симптом: курсор отстаёт от аудио, рывки
```

### 3. `IDispatcherTimer` с тяжёлым callback
```
Проблема: Tick → позиция → слайдер → лейбл → скролл → подсветка → InvalidateSurface
         — все на UI-потоке. Следующий Tick задерживается.
```

### 4. `new SKPaint` на каждом кадре
```
Проблема: 6-8 аллокаций/кадр × 30fps = 240 объектов/сек
Симптом: GC каждые 2-3 сек → stutter 80-200ms
```

### 5. Кеширование `SKBitmap` всей сцены
```
Проблема: segment lines + time ruler зафиксированы с visLeft=0
Симптом: линии не двигаются с окном просмотра → визуальный мусор
```

### 6. Троттлинг `InvalidateSurface`
```
Проблема: пропуск кадров делает движение ещё более рваным
Симптом: вместо 30fps с джиттером → 10fps с большими скачками
```

---

## ✅ Правильное решение (архитектура v15)

### Принцип: SKPicture для waveform data

Waveform data (кривая) рисуется **один раз** в `SKPicture` при загрузке файла.
Per-frame: `canvas.DrawPicture(_waveformPicture)` — GPU-блит, почти бесплатно.

### Код

```csharp
// ── Поле для кеша ──
private SKPicture? _waveformPicture;
private float _pictureW, _pictureH;

// ── Построение при загрузке ──
private void BuildWaveformPicture()
{
    _waveformPicture?.Dispose();
    if (_audio.SamplesSmall == null) return;

    float w = (float)WaveformCanvas.Width;
    float h = 120f - 22f; // waveAreaH
    if (w <= 0 || h <= 0) { w = 800; h = 98; }

    using var recorder = new SKPictureRecorder();
    using var recCanvas = recorder.BeginRecording(new SKRect(0, 0, w, h));
    DrawWaveformDataStatic(recCanvas, _audio.SamplesSmall, w, h);
    _waveformPicture = recorder.EndRecording();
    _pictureW = w;
    _pictureH = h;
}

// ── Статическая отрисовка (один раз) ──
private void DrawWaveformDataStatic(SKCanvas canvas, float[] small, float w, float h)
{
    int totalPixels = (int)w;
    if (totalPixels <= 1 || small.Length <= 1) return;

    using var paint = new SKPaint { Color = ..., StrokeWidth = 1.2f, ... };
    float midY = h / 2f; float scaleY = h / 2f * 0.85f;
    var path = new SKPath(); bool first = true;

    for (int px = 0; px < totalPixels; px++)
    {
        int si = (int)((double)px / totalPixels * small.Length);
        float y = Math.Clamp(midY - small[Math.Min(si, small.Length - 1)] * scaleY, 0, h);
        if (first) { path.MoveTo(px, y); first = false; }
        else path.LineTo(px, y);
    }
    canvas.DrawPath(path, paint);
    path.Dispose();
}

// ── Per-frame рендер ──
private void OnWaveformPaint(..., SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    // ... clear, get visLeft ...

    // GPU-блит waveform picture — clipping через src rect
    float visLeftPx = (float)(visLeft / _durationSeconds * _pictureW);
    float visWidthPx = (float)(visWidth / _durationSeconds * _pictureW);
    canvas.DrawPicture(_waveformPicture, new SKPoint(-visLeftPx, 0));

    // Per-frame overlays (лёгкие, с static paints)
    DrawSegmentLines(...);
    DrawCursor(...);
    DrawTimeRuler(...);
}
```

### Структура per-frame рендера (v15)
```
OnWaveformPaint:
  canvas.Clear()
  canvas.DrawPicture(_waveformPicture, offset)  ← GPU-блит, 0 аллокаций
  DrawSegmentLines()    ← static paint, 0 аллокаций
  DrawSegmentHighlight() ← static paint, 0 аллокаций
  DrawCursor()           ← static paint, 0 аллокаций
  DrawTimeRuler()        ← static paint, 0 аллокаций
```

### Итоговая архитектура
```
IDispatcherTimer (UI-поток, 33ms)
  ├─ Stopwatch.Elapsed → позиция (наносекунды, не DateTime.Now)
  ├─ Slider.Value = pos
  ├─ LblPosition.Text = format(pos)
  ├─ WaveformCanvas.InvalidateSurface()
  │    └─ OnWaveformPaint
  │         ├─ canvas.Clear()
  │         ├─ DrawPicture(_waveformPicture)  ← GPU-блит (1 вызов, 0 аллокаций)
  │         ├─ DrawSegmentLines()             ← static paint
  │         ├─ DrawCursor()                   ← static paint
  │         └─ DrawTimeRuler()                ← static paint
  └─ HighlightActiveWord()  ← отдельный таймер 100ms для подсветки
```

## Ключевые выводы

1. **SKPicture — правильный способ кеширования векторной графики на GPU**. В отличие от SKBitmap (растровый блит с потерей качества при масштабировании), SKPicture сохраняет векторные команды — GPU рендерит их на лету с идеальным качеством.
2. **Stopwatch, не DateTime.Now** — наносекундное разрешение против 10-16ms.
3. **IDispatcherTimer, не System.Threading.Timer** — меньше задержки на MAUI.
4. **static readonly paints — обязательно** — иначе GC убивает плавность.
5. **Никакого троттлинга InvalidateSurface** — каждый кадр важен для плавности.
6. **Подсветка + скролл на отдельном таймере** — не нагружают основной цикл отрисовки.
