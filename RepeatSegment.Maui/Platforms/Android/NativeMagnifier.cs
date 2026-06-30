#if ANDROID
using Android.Graphics;

namespace RepeatSegment.Maui;

public static class NativeMagnifier
{
    static Android.Views.View? _labelView;

    public static void Attach(Microsoft.Maui.Controls.Label label)
    {
        if (label.Handler?.PlatformView is Android.Views.View v) _labelView = v;
        else label.HandlerChanged += (_, _) =>
        { if (label.Handler?.PlatformView is Android.Views.View v2) _labelView = v2; };
    }

    /// <summary>Capture a screenshot of the native TextView around (pxX,pxY) with diameter=size.</summary>
    public static byte[]? Capture(int pxX, int pxY, int size)
    {
        if (_labelView == null) return null;
        try
        {
            var bmp = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!);
            using var canvas = new Android.Graphics.Canvas(bmp);
            canvas.Translate(-pxX + size / 2f, -pxY + size / 2f);
            _labelView.Draw(canvas);
            using var ms = new System.IO.MemoryStream();
            bmp.Compress(Bitmap.CompressFormat.Png!, 80, ms);
            return ms.ToArray();
        }
        catch { return null; }
    }
}
#endif
