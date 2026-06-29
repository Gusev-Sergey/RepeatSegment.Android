# Как запустить RepeatSegment на Android-эмуляторе

## 1. Запуск эмулятора (Android Studio)
1. Открой **Android Studio**
2. Нажми **More Actions** → **Virtual Device Manager** (или `...` → Virtual Device Manager)
3. В списке найди **Pixel_7** → нажми зелёную стрелку ▶️
4. Жди загрузки рабочего стола Android (1-2 минуты)

> ⚠️ Если эмулятор падает с ошибкой `libGLESv2.dll`:
> - Нажми значок **карандаша** (Edit) на Pixel_7
> - **Show Advanced Settings** → **Emulated Performance → Graphics** → выбери **Software - GLES 2.0**
> - **Finish** → запусти ▶️ заново

## 2. Запуск приложения (Visual Studio 2022)
1. Открой **Visual Studio 2022**
2. **Файл → Открыть → Проект/решение** → выбери:
   `C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\RepeatSegment.Maui.csproj`
3. В верхней панели выбери **Debug** и цель **Pixel 7 (Android)**
4. Жми зелёную стрелку ▶️

VS сама соберёт, установит и запустит приложение на эмуляторе.

## Что видно прямо сейчас
- ✅ APK собран: `com.astrorumarbor.repeatsegment-Signed.apk`
- ✅ Эмулятор Pixel_7 создан (API 35, Google APIs)
- ✅ Бизнес-логика подключена (8 .cs файлов из WPF-проекта)
- ✅ UI плеера готов (waveform, 5 кнопок, слайдер, статус)
