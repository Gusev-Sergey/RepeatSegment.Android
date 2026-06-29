# RepeatSegment — План портирования WPF → MAUI Android

Сопоставление плана из [`c:\ProjectsCSharp\RepeatSegment\plans\android_port_plan.md`](c:\ProjectsCSharp\RepeatSegment\plans\android_port_plan.md) с текущим состоянием MAUI-проекта.

## Файловая миграция: план vs факт

| WPF-файл | План | Статус в MAUI |
|----------|------|---------------|
| `AudioEngine.cs` | **Rewrite** (MediaExtractor + AudioTrack) | ✅ Готово |
| `SilenceDetector.cs` | **Reuse** | ✅ Shared via Link |
| `TranscriptionProvider.cs` | **Reuse** | ✅ Shared via Link |
| `TranslationProvider.cs` | **Reuse** | ✅ Shared via Link |
| `TtsProvider.cs` | **Reuse** | ✅ Shared via Link |
| `AnkiBuilder.cs` | **Reuse** | ✅ Shared via Link |
| `AnkiExportManager.cs` | **Reuse** | ✅ Shared via Link |
| `ConfigManager.cs` | **Adapt** | ✅ Shared via Link |
| `Strings.cs` | **Reuse** | ✅ Shared via Link |
| `MainWindow.xaml` | **Rewrite** → PlayerPage | ✅ PlayerPage.xaml |
| `AnkiCardWindow.xaml` | **Rewrite** → AnkiCardPage | ❌ **Не сделано** |
| `SettingsWindow.xaml` | **Rewrite** → SettingsPage | ✅ SettingsPage.xaml |
| `ManualWindow.xaml` | **Adapt** | ⚠️ Упрощено |
| `WaveformGraph.cs` (SkiaSharp) | **Rewrite** | ✅ Готово |
| `VolumeWidget.cs` | **Drop** | ✅ Убран |
| `AboutWindow.xaml` | **Adapt** | ⚠️ Alert, не окно |
| `GeneralSettingsWindow.xaml` | **Adapt** | ✅ Часть SettingsPage |
| `TranslationSettingsWindow.xaml` | **Adapt** | ✅ Часть SettingsPage |

## Текущее состояние Phase 1 (MVP) из WPF-плана

| # | Задача из android_port_plan.md | Статус |
|---|-------------------------------|--------|
| 1 | MAUI project setup + shared core | ✅ |
| 2 | AudioEngine rewrite | ✅ |
| 3 | Player: waveform, slider, 5 кнопок | ✅ (7 кнопок + speed/volume) |
| 4 | Transcription: текст | ✅ (inline Label) |
| 5 | Translation: выделение → перевод | ❌ |
| 6 | Settings: API keys screen | ✅ |
| 7 | AnkiCard: basic card creation | ❌ |

## Phase 2 из WPF-плана

| # | Задача | Статус |
|---|--------|--------|
| 8 | Anki card: sentence audio + picture search | ❌ |
| 9 | Library: file browser | ❌ |
| 10 | Background playback + notification | ❌ |

## Phase 3 из WPF-плана

| # | Задача | Статус |
|---|--------|--------|
| 11 | Material Design theming | ⚠️ Частично |
| 12 | Google Play Store listing | ❌ |
| 13 | Crash reporting | ❌ |

---

## Что DROP (из WPF в Android)

| Функция | Причина |
|---------|---------|
| `VolumeWidget` | Кнопки громкости устройства |
| `FirstRunWindow` | Системная локаль Android |
| `ManualWindow` (сложный) | Простой About-экран |
| Запись с микрофона | Редко используется |
| OxyPlot | Заменён на SkiaSharp |
| Тёмная/светлая тема (ручная) | System-following |
| WiX Installer | Google Play / APK |

---

## План дальнейшего портирования (4 фазы)

### 🔴 Фаза 1: Android-специфичные фичи (основа для Play Store)

| # | Задача | Детали |
|---|--------|--------|
| 1 | **Foreground Service** | Вынести AudioEngine в `PlaybackService : Service`. Аудио не должно убиваться при сворачивании. |
| 2 | **MediaSession** | `MediaSessionCompat` для управления с lock screen, Bluetooth-гарнитуры, Android Auto |
| 3 | **Уведомление** | Компактное уведомление с кнопками play/pause/next/close. Канал `playback_channel`. |
| 4 | **Audio Focus** | `AudioManager.OnAudioFocusChangeListener`: duck/pause при звонках, уведомлениях |
| 5 | **Permissions** | `POST_NOTIFICATIONS`, `FOREGROUND_SERVICE_MEDIA_PLAYBACK`, `READ_MEDIA_AUDIO` (Android 13+) |
| 6 | **Headset controls** | `MediaSession.Callback.OnMediaButtonEvent` |

### 🟡 Фаза 2: Бизнес-логика из WPF

| # | Задача | Детали |
|---|--------|--------|
| 7 | **TranslationProvider — интеграция** | Выделение текста (слово / фраза / несколько предложений) в транскрипции → автоматический перевод. Панель перевода появляется под текстом, содержит: оригинальный выделенный текст, перевод, информацию о провайдере (Google/Yandex). Кнопка **[+Anki]** забирает выделение с переводом и открывает AnkiCardPage. |
| 8 | **AnkiCardPage** | Полностраничный диалог: поля EN word (авто из выделения), transcription (международный формат — авто-поиск), translation (авто из перевода), context (предложение-источник из аудиокниги), sentence audio (извлечение из аудио), TTS audio (Google/Deepgram), image search (Google/Yandex → WebView), выбор колоды (Deck dropdown). Кнопка **[Create Cards]**. Экспорт .apkg с Intent share в AnkiDroid. |
| 9 | **TtsProvider — кнопка** | Озвучка выделенного слова/фразы через Android TTS или Deepgram/Google TTS API |
| 10 | **AnkiExportManager — кнопка** | Создание .apkg, `Intent.ACTION_SEND` + content URI для отправки в AnkiDroid |

### 🟢 Фаза 3: UX-полировка

| # | Задача | Детали |
|---|--------|--------|
| 11 | **Bottom sheet** для транскрипции | Draggable: peek 80dp → expanded 60% экрана. Вместо текущего inline ScrollView. |
| 12 | **Speed badge cycling** | Тап по ⚡1× cycling: 0.4×→0.5×→...→1.5×→1.0×. Вместо Picker. |
| 13 | **Long press** ⏪/⏩ | Быстрая перемотка при удержании |
| 14 | **Pinch-zoom** waveform | Горизонтальный zoom waveform через pinch-жест |
| 15 | **Локализация UI** | Загрузка из WPF `lang/*.json`: RU, EN, DE, FR, ES. `Strings.cs` уже shared. |
| 16 | **Splash Screen** | Нативный Android splash (MAUI 9) |
| 17 | **Material Design 3** | Primary: `#1E3A5F`, Accent: `#00B4A6`. System dark/light mode. |

### 🔵 Фаза 4: Production-ready

| # | Задача | Детали |
|---|--------|--------|
| 18 | **Стриминг-декодирование** | Сейчас весь файл в RAM (600 MB для 2ч аудиокниги). Декодировать чанками. |
| 19 | **Library screen** | Список аудиокниг: недавние + выбор из файловой системы |
| 20 | **Google Play Store** | Иконки (48×48, 72×72, 96×96, 144×144, 192×192, 512×512), Feature Graphic (1024×500), скриншоты, Privacy Policy URL, listing EN/RU |
| 21 | **Crash reporting** | Sentry или AppCenter |
| 22 | **ProGuard/R8** | Правила обфускации для Release |
| 23 | **Unit-тесты** | xUnit: SilenceDetector, WSOLA stretch, AnkiBuilder |

---

## Баги, исправленные 29.06.2026

### Баг 1: Play с начала файла пропускает первый сегмент

**Симптом**: если находимся в начале первого сегмента (левее аудио нет), нажатие ▶ (play) переключает на второй сегмент вместо проигрывания текущего.

**Причина**: в `ButtonsPressed("play")` строка `_counter++` выполнялась безусловно.

**Исправление**: [`PlayerPage.xaml.cs:136-143`](RepeatSegment.Maui/Pages/PlayerPage.xaml.cs:136) — добавлена проверка `atVeryStart`:
```csharp
bool atVeryStart = (_counter == 0 && Math.Abs(_positionSeconds - _t1) < 0.05);
if (!atVeryStart) { /* _counter++ */ }
```

### Баг 2: Непрерывное проигрывание пропускает сегмент без звука

**Симптом**: при `_pte` (play-to-end) режиме следующий сегмент запускается, waveform движется, но звука нет — только через сегмент появляется звук.

**Причина**: `AudioTrack.Play()` вызывался ДО `Write(pcmBytes)`. `Task.Run(() => Write(...))` создавал гонку: Play стартует, запрашивает данные из буфера, а буфер ещё пуст → тишина. Особенно проявлялось на коротких сегментах (2–5 сек).

**Исправление**: [`AudioEngine.cs:422-424`](RepeatSegment.Maui/Services/AudioEngine.cs:422), [`AudioEngine.cs:463-465`](RepeatSegment.Maui/Services/AudioEngine.cs:463), [`AudioEngine.cs:531-533`](RepeatSegment.Maui/Services/AudioEngine.cs:531) — во всех методах (`Play`, `PlaySegment`, `Seek`) запись теперь синхронная ДО `Play()`:
```csharp
_audioTrack.Write(pcmBytes, 0, pcmBytes.Length);  // сначала данные
_audioTrack.Play();  // потом старт
```
