# Recovery Log — 2026-07-01/02 (MSK)
## Включает финальные находки от Gemini

---

## Часть 1: Восстановление потерянного кода (2026-07-01)

### Причина потери

При сбое в сессии произошёл `git reset --hard HEAD~2`, откативший коммиты:
- `3ebab89` — **v1.0-stable-loupe-pinch-highlight-translation-cache** (47 файлов, 17 899 строк)
- `462e0a0` — remove Yandex API keys before push

Попытка восстановления (`1414a80`) была неполной.

### Метод восстановления

Коммит найден через `git reflog`. Восстановление: `git checkout 3ebab89 -- <10 файлов>`.

---

## Часть 2: Битва с кнопкой «Open decks folder» (2026-07-02)

### Задача

Открыть системный файловый менеджер Android в папке `Download/RepeatSegment/`, чтобы пользователь видел созданные `.apkg` колоды.

### Неудачные попытки (7 итераций)

| # | Подход | Результат |
|---|--------|-----------|
| 1 | `Intent.ActionView` + `Uri.FromFile()` | `FileUriExposedException` (Android 7+, запрет file:// URI) |
| 2 | FileProvider на приватную папку `files/.config/RepeatSegment/decks/` | `IllegalArgumentException: Failed to find configured root` — файл в приватной папке, FileProvider ожидает внешнее хранилище |
| 3 | Копирование в `Downloads/` + FileProvider `<external-path path="Download/RepeatSegment/">` | `IllegalArgumentException` — файл лежал в `Download/` без подпапки, а FileProvider требовал `Download/RepeatSegment/` |
| 4 | Копирование в `Downloads/RepeatSegment/` + FileProvider + `CreateChooser(ACTION_VIEW, application/octet-stream)` | `IllegalArgumentException` — `external-path` не соответствует реальному пути на Android 15 |
| 5 | `ACTION_OPEN_DOCUMENT_TREE` с `INITIAL_URI` на Download | Ничего не происходило — SAF-пикер не активировался без должного контекста |
| 6 | FileProvider `<external-path path="."/>` (вся внешняя память) + `GetUriForFile(папка)` | Краш — FileProvider не работает с папками, только с файлами |
| 7 | FileProvider `<external-path path="."/>` + `GetUriForFile(файл)` + `ACTION_VIEW` + `*/*` | Где-то краш, где-то ничего не происходило — зависит от оболочки |

### Причины провалов

1. **FileUriExposedException** — Android 7+ запрещает передачу `file://` URI между приложениями. Нужен `content://` URI через FileProvider.
2. **FileProvider не работает с папками** — `GetUriForFile()` только для файлов, не для директорий.
3. **Несовпадение путей** — `<external-path path="Download/RepeatSegment/">` требует точного совпадения реального пути `/storage/emulated/0/Download/RepeatSegment/`. Если файл лежит в `Download/` без подпапки — ошибка.
4. **Кастомные оболочки (iQOO OriginOS)** — штатный файловый менеджер не регистрирует `vnd.android.document/directory` MIME-тип, поэтому `ACTION_VIEW` не находит обработчика.

### ✅ Финальное рабочее решение (Gemini)

**Двухэтапный подход** (скопирован 1:1 из рекомендации Gemini):

```csharp
// Этап 1: Открыть папку RepeatSegment через System DocumentsUI
string folderName = "RepeatSegment";
string uriString = $"content://com.android.externalstorage.documents/document/primary%3ADownload%2F{folderName}";
var uri = Android.Net.Uri.Parse(uriString);
var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
intent.SetDataAndType(uri, "vnd.android.document/directory");
intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission |
                 Android.Content.ActivityFlags.NewTask);
Android.App.Application.Context.StartActivity(intent);
```

```csharp
// Fallback: DownloadManager для кастомных оболочек (iQOO, Xiaomi)
var fbIntent = new Android.Content.Intent(Android.App.DownloadManager.ActionViewDownloads);
fbIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
Android.App.Application.Context.StartActivity(fbIntent);
```

**Почему работает:**
- Чистый Android (Pixel, Motorola, Samsung) — этап 1 открывает папку RepeatSegment напрямую
- Кастомные оболочки (iQOO OriginOS, MIUI) — этап 1 падает в `ActivityNotFoundException`, этап 2 открывает папку `Download/` целиком, пользователь видит подпапку RepeatSegment
- Крайний fallback — диалог с путём для копирования в буфер

**Файлы, задействованные в решении:**
- [`AnkiCardPage.xaml.cs:227-255`](RepeatSegment.Maui/Pages/AnkiCardPage.xaml.cs:227) — двухэтапный Intent
- [`PublicDecksDir()`](RepeatSegment.Maui/Pages/AnkiCardPage.xaml.cs:206) — копирование .apkg в Download/RepeatSegment/
- [`OnCreateClicked()`](RepeatSegment.Maui/Pages/AnkiCardPage.xaml.cs:530) — копирование .apkg при создании карточки

### Бонус: CleanWord() + FindSentenceBounds

В ходе сессии также исправлены:
- **Перенос знаков препинания** — `CleanWord()` общий метод очистки, используется в `TranslateSelection()` и `OnAnkiClicked()`
- **Границы предложения** — `FindSentenceBounds()` использует `_wordStart` (время первого выделенного слова), а не `_positionSeconds`; `_sentenceEnd + 0.12s` padding для захвата конца слова
