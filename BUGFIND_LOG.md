# RepeatSegment — Log of bugs found during selection & loupe development

## 2026-06-30: Selection, Loupe, Translation

### Bug 1: TapGestureRecognizer forgets to set translation text
- **Symptom**: selection was drawn (blue highlight) but TranslationPanel stayed invisible
- **Root cause**: `TranslateSelection` was never actually called after the refactor
- **Fix**: ensure `_ = TranslateSelection()` in both `OnTransTouch action=2` and `TranslateSelection` itself calls `TranslationPanel.IsVisible = true`

### Bug 2: Character-level selection indices wrong at TranslateSelection
- **Symptom**: `_selStartChar/_selEndChar` once set to word indexes instead of char indexes
- **Root cause**: older code used `_selStartIdx` (word index), search&replace changed it to `_selStartChar` but the assignment in `OnTransTouch` still used `wordIdx` instead of `charIdx`
- **Fix**: `_selStartChar = charIdx; _selEndChar = charIdx;` (already fixed in final code)

### Bug 3: Loupe too far from touch point
- **Symptom**: magnifier appeared far above the finger
- **Root cause**: `pxY` was relative to `TransOverlay` (Row 6) while `LoupeOverlay` was positioned from page top via `TranslationY`. The missing offset `TransOverlay.Y` pushed the loupe up.
- **Fix**: `float sy = pxY / density + (float)TransOverlay.Y;`

### Bug 4: Loupe magnification incorrect
- **Symptom**: huge zoom instead of ×1.3
- **Root cause**: `Capture(138, 138)` — 138 **pixels** captured, then stretched to 180 **dips** container. On a 3.5× density device the actual magnification was 180×3.5 / 138 = ×4.6!
- **Fix**: `capturePx = (int)(180f * density / 1.3f)` — capture size calculated in pixels matching the target dips × density.

### Bug 5: Selection broken for reverse direction (left/up)
- **Symptom**: dragging right/down selected fine, dragging left/up selected only the last character.
- **Root cause**: every `Move` recalculated `s = _selStartChar, e = charIdx; if (s > e) swap`. The swap set `_selStartChar` to the *current* touch, losing the *original* anchor. Next Move used this wrong `_selStartChar` again — anchor kept wandering.
- **Fix**: introduced `_selAnchorChar` — set once on Down, never changed. Move/Up do `_selStartChar = Min(_selAnchorChar, charIdx)`, `_selEndChar = Max(_selAnchorChar, charIdx)`.

### Bug 6: Crash during playback (CalledFromWrongThreadException)
- **Symptom**: app crashed seconds after starting playback
- **Root cause**: `HlWordBg()` runs on `System.Threading.Timer` (background thread) and called `RebuildTranscriptSpans()` which previously called `LblTranscription.FormattedText = fmt` (UI operation).
- **Fix**: wrapped `RebuildTranscriptSpans()` in `MainThread.BeginInvokeOnMainThread` for the timer path. Alternatively, moved the threading check into `RebuildTranscriptSpans`.

### Bug 7: Loupe image stuttering / blank
- **Symptom**: magnifier showed blank or jerky updates
- **Root cause**: `NativeMagnifier.Capture()` performed Bitmap creation + compress + PNG encode on **every** Move event (60+/sec), saturating the render thread.
- **Fix**: cached `_lastLoupeBmp` with a 20px dead zone. Capture only when finger moved >20px since last capture, skip otherwise.

### Bug 8: FormattedString visibility — Bold causes text reflow
- **Symptom**: selected words changed font weight, moving surrounding text to new lines
- **Root cause**: `FontAttributes.Bold` in span changes character width → line rewrap → layout shift.
- **Fix**: removed all `FontAttributes = FontAttributes.Bold` from selection spans.

## Key Lessons

1. **Android TouchListener always runs on UI thread** — no `BeginInvokeOnMainThread` needed for direct UI updates from touch callbacks.
2. **MAUI `TranslationY` uses dips, not pixels** — must divide raw pixel coords by `DeviceDisplay.MainDisplayInfo.Density` before positioning.
3. **Capture size in pixels vs display in dips** — the actual magnification factor is `(display_dips × density) / capture_pixels`, not `display / capture`.
4. **Anchor-based selection** is the standard pattern for text selection — anchor stays fixed at touch-down, selection range extends from anchor to current position.
5. **Grid.RowSpan for overlay positioning** — elements positioned via `TranslationX/Y` relative to their parent container; for full-page overlays, use `Grid.RowSpan` to span all rows.
