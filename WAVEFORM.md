# Waveform Smooth Rendering — Диагноз, атака, решение

> Объединение трёх файлов: `WAVEFORM_DIAGNOSIS.md`, `WAVEFORM_ATTACK_PLAN.md`, `WAVEFORM_SOLUTION.md`

## Проблема

Визуальная отрисовка аудиоволны и курсора двигалась рывками на мощном Android-устройстве (iQOO Neo9S Pro+, Android 15). Скачки ~2-4 пикселя за кадр одинаково затрагивали сложный `DrawPath` и простой `DrawLine` — **системная проблема, не вычислительная**.

## Что точно НЕ является проблемой

| Фактор | Почему исключён |
|--------|-----------------|
| Мощность GPU | Игры на 120fps работают идеально |
| Сложность отрисовки | Даже ОДНА линия дёргается |
| GC-паузы | Static readonly SKPaint исключает аллокации |
| Количество объектов | Без waveform data линия всё равно дёргается |

## Перечень попыток и причин неудач (9 итераций)

| # | Подход | Причина неудачи |
|---|--------|-----------------|
| 1 | `IDispatcherTimer` 33ms + `DateTime.Now` (оригинал) | `DateTime.Now` ~10-16ms гранулярность; таймер на UI-потоке задерживается рендером |
| 2 | `System.Threading.Timer` 16ms + `Stopwatch` + `Dispatcher.Dispatch` | +1-3 кадра задержки между позицией и `InvalidateSurface` |
| 3 | Троттлинг `InvalidateSurface` (каждый 3-й Tick) | Усугубляет: 30fps → 10fps с большими скачками |
| 4 | `SKPicture` кеш waveform data на GPU | Координаты зафиксированы — `DrawPicture(offset)` некорректно для широких файлов |
| 5 | Раздельные таймеры: позиция 33ms + подсветка 100ms | Не влияет — проблема в источнике сигнала, не в нагрузке |
| 6 | Самоподдерживающийся цикл: `InvalidateSurface` в конце | `InvalidateSurface` не мгновенный |
| 7 | `IsAntialias = true` на курсоре | Субпиксельное сглаживание не решает джиттер источника |
| 8 | `static readonly SKPaint` для всех красок | Убирает GC, но рывки остаются — источник не GC |
| 9 | Возврат к оригинальному коду | Рывки были изначально |

## Корневой диагноз

**`IDispatcherTimer` + `InvalidateSurface` не синхронизированы с VSYNC дисплея.** Правильный путь — `Choreographer.FrameCallback`.

## Непроверенные подходы (план атаки)

| # | Подход | Риск |
|---|--------|------|
| 1 | `Stopwatch` вместо `DateTime.Now` | Низкий |
| 2 | `SKGLSurfaceView` вместо `SKCanvasView` | Средний |
| 3 | Нативный `Android.OS.Handler` с `PostAtTime` | Средний |
| 4 | `MAUI GraphicsView` + `IDrawable` | Высокий |
| 5 | Нативный `Android.Views.View` + `Canvas.OnDraw` | Очень высокий |

---

## ✅ Правильное решение (v15)

### Принцип: SKPicture для waveform data

Waveform data рисуется **один раз** в `SKPicture` при загрузке. Per-frame: `canvas.DrawPicture()` — GPU-блит.

### Ключевые выводы

1. **SKPicture** — правильный способ кеширования векторной графики на GPU (в отличие от `SKBitmap`)
2. **Stopwatch**, не `DateTime.Now` — наносекундное разрешение
3. **IDispatcherTimer**, не `System.Threading.Timer` — меньше задержки на MAUI
4. **static readonly paints — обязательно** — иначе GC убивает плавность
5. **Никакого троттлинга InvalidateSurface** — каждый кадр важен
6. **Подсветка + скролл на отдельном таймере** — не нагружают основной цикл
