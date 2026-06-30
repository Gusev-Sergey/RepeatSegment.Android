# Исправленный план исправления трёх багов (с учётом замечаний)

## Баг 1 — Лупа: реалистичное изображение с ×1.5 увеличением

### Требование
Лупа должна показывать **реальное изображение экрана под пальцем** (как настоящая лупа), а не копию текста. В ней должны отражаться:
- Текст как есть (шрифт, цвет, переносы строк)
- Динамическая подсветка (золотая при проигрывании, синяя при выделении)
- Увеличение ×1.5 относительно основного текста

### Техническое решение
`NativeMagnifier.Capture(int pxX, int pxY, int size)` делает скриншот нативного Android `TextView` (который уже содержит все Span-ы с подсветкой) через `View.Draw(Canvas)`:
- Размер захвата: `180 / 1.5 = 120px`
- Canvas.Translate смещает область захвата на точку касания
- Bitmap → PNG byte[] → `ImageSource.FromStream()`
- Результат в `LoupeImage` (круглый `Image` внутри `Border` с `StrokeShape="Ellipse"`)

### Почему это работает
`RebuildTranscriptSpans()` обновляет `LblTranscription.FormattedText` → MAUI передаёт в нативный `TextView` → `TextView.Layout` перестраивается → при следующем `Capture()` скриншот содержит актуальную подсветку.

### Детали реализации в ShowLoupe
```csharp
private void ShowLoupe(int wordIdx, float pxX, float pxY)
{
    // Position loupe (above finger, not overlapping TranslationPanel)
    float density = DeviceDisplay.MainDisplayInfo.Density;
    float lw = 180f, lh = 180f;
    float lx = pxX / density + 8f;
    float ly = pxY / density - lh - 12f;
    // Clamp to screen / avoid TranslationPanel
    LoupeOverlay.TranslationX = lx;
    LoupeOverlay.TranslationY = ly;
    LoupeOverlay.IsVisible = true;
    
    // Capture 120px screenshot = 1.5× magnification when shown at 180px
    var bmp = NativeMagnifier.Capture((int)pxX, (int)pxY, 120);
    if (bmp != null)
        LoupeImage.Source = ImageSource.FromStream(() => new MemoryStream(bmp));
}
```

### XAML
```xml
<Border x:Name="LoupeOverlay" IsVisible="False" Grid.RowSpan="9"
        WidthRequest="180" HeightRequest="180"
        StrokeShape="Ellipse" Stroke="#00B4A6" StrokeThickness="3"
        BackgroundColor="#2D2D3F" InputTransparent="True" Padding="0">
    <Image x:Name="LoupeImage" Aspect="AspectFill" />
</Border>
```

---

## Баг 2 — Лупа перекрывается TranslationPanel

### Корень
`LoupeOverlay` с `Grid.Row="6"` привязан к строке транскрипции. Grid обрезает содержимое строки, поэтому `TranslationY` не может выйти за пределы Row 6.

### Фикс
Заменить `Grid.Row="6"` на `Grid.RowSpan="9"` — элемент будет позиционироваться абсолютно через `TranslationX/Y` поверх всей страницы, включая TranslationPanel.

---

## Баг 3 — Выделение влево/вверх не работает

### Корень
`MainThread.BeginInvokeOnMainThread` создаёт задержку в 1 кадр (16ms при 60fps). При вызове из нативного TouchListener (UI-поток) эта задержка не нужна и приводит к рассинхронизации с движением пальца.

### Фикс
В `RebuildTranscriptSpans()`:
```csharp
if (MainThread.IsMainThread)
    LblTranscription.FormattedText = fmt;  // direct — no delay
else
    MainThread.BeginInvokeOnMainThread(() => 
        LblTranscription.FormattedText = fmt);  // from HlWordBg bg timer
```

---

## Файлы для изменения

1. [PlayerPage.xaml](RepeatSegment.Maui/Pages/PlayerPage.xaml) — `LoupeOverlay`: заменить `Grid.Row="6"` на `Grid.RowSpan="9"`, вернуть `Image` + `Aspect="AspectFill"`
2. [PlayerPage.xaml.cs](RepeatSegment.Maui/Pages/PlayerPage.xaml.cs) — `ShowLoupe`: добавить захват скриншота и параметр `wordIdx`; `RebuildTranscriptSpans`: проверка `MainThread.IsMainThread`; `OnTransTouch`: передавать wordIdx в ShowLoupe
3. [NativeMagnifier.cs](RepeatSegment.Maui/Platforms/Android/NativeMagnifier.cs) — восстановить метод `Capture` (скриншот нативного View)

## Проверка отсутствия крашей

- `Capture()` использует try/catch и возвращает null при ошибке
- `DeviceDisplay.MainDisplayInfo.Density` кешируется в поле при первом вызове
- Все операции с UI — через MainThread
