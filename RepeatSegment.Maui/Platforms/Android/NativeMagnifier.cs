#if ANDROID
using Android.Graphics;
using Android.Widget;

namespace RepeatSegment.Maui;

public static class NativeMagnifier
{
    static Android.Views.View? _labelView;
    static Android.Views.View? _overlayView;
    static Android.Views.View? _loupeView;    // native view of LoupeOverlay (Border)
    static ImageView? _loupeImageView;          // native ImageView inside LoupeOverlay
    static Bitmap? _cachedBmp;
    static int _cachedSize;

    public static void Attach(Microsoft.Maui.Controls.Label label)
    {
        if (label.Handler?.PlatformView is Android.Views.View v) _labelView = v;
        else label.HandlerChanged += (_, _) =>
        { if (label.Handler?.PlatformView is Android.Views.View v2) _labelView = v2; };
    }

    public static void AttachOverlay(Microsoft.Maui.Controls.BoxView overlay)
    {
        if (overlay.Handler?.PlatformView is Android.Views.View v) _overlayView = v;
        else overlay.HandlerChanged += (_, _) =>
        { if (overlay.Handler?.PlatformView is Android.Views.View v2) _overlayView = v2; };
    }

    public static void AttachLoupe(Microsoft.Maui.Controls.Border loupe)
    {
        if (loupe.Handler?.PlatformView is Android.Views.View v) _loupeView = v;
        else loupe.HandlerChanged += (_, _) =>
        { if (loupe.Handler?.PlatformView is Android.Views.View v2) _loupeView = v2; };
        // Find ImageView inside
        if (loupe.Content is Microsoft.Maui.Controls.Image img)
        {
            if (img.Handler?.PlatformView is ImageView iv) _loupeImageView = iv;
            else img.HandlerChanged += (_, _) =>
            { if (img.Handler?.PlatformView is ImageView iv2) _loupeImageView = iv2; };
        }
    }

    /// <summary>Fast capture: natively hide loupe, draw root, set native ImageView bitmap directly. No MAUI ImageSource overhead.</summary>
    public static void CaptureToImageView(int pxX, int pxY, int size)
    {
        if (_labelView == null || _overlayView == null || _loupeImageView == null) return;
        try
        {
            int[] loc = new int[2];
            _overlayView.GetLocationOnScreen(loc);
            int absX = loc[0] + pxX;
            int absY = loc[1] + pxY;

            var root = _labelView.RootView;
            if (root == null) return;

            // Reuse bitmap
            if (_cachedBmp == null || _cachedSize != size)
            {
                _cachedBmp?.Recycle();
                _cachedBmp = Bitmap.CreateBitmap(size, size, Bitmap.Config.Rgb565!);
                _cachedSize = size;
            }

            // NATIVE hide loupe (synchronous on UI thread)
            var oldVis = _loupeView?.Visibility;
            if (_loupeView != null) _loupeView.Visibility = Android.Views.ViewStates.Invisible;

            using var canvas = new Android.Graphics.Canvas(_cachedBmp);
            _cachedBmp.EraseColor(Android.Graphics.Color.Transparent);
            canvas.Translate(-absX + size / 2f, -absY + size / 2f);
            root.Draw(canvas);

            // Restore loupe visibility
            if (_loupeView != null && oldVis != null) _loupeView.Visibility = oldVis.Value;

            // Set bitmap directly on native ImageView — no MAUI round-trip, no flicker
            _loupeImageView.SetImageBitmap(_cachedBmp);
        }
        catch { }
    }
}
#endif
