#if ANDROID
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace RepeatSegment.Maui;

public static class NativeMagnifier
{
    static Android.Views.View? _labelView;
    static ImageView? _loupeImageView;

    public static void Attach(Microsoft.Maui.Controls.Label label)
    {
        if (label.Handler?.PlatformView is Android.Views.View v) _labelView = v;
        else label.HandlerChanged += (_, _) =>
        { if (label.Handler?.PlatformView is Android.Views.View v2) _labelView = v2; };
    }

    public static void AttachLoupeView(Microsoft.Maui.Controls.Image loupeImage)
    {
        if (loupeImage.Handler?.PlatformView is ImageView iv) _loupeImageView = iv;
        else loupeImage.HandlerChanged += (_, _) =>
        { if (loupeImage.Handler?.PlatformView is ImageView iv2) _loupeImageView = iv2; };
    }

    /// <summary>Capture a screenshot of the native TextView around (pxX,pxY) with diameter=size.</summary>
    public static byte[]? Capture(int pxX, int pxY, int size)
    {
        if (_labelView == null) return null;
        try
        {
            var bmp = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!);
            using var canvas = new Canvas(bmp);
            canvas.Translate(-pxX + size / 2f, -pxY + size / 2f);
            _labelView.Draw(canvas);
            using var ms = new System.IO.MemoryStream();
            bmp.Compress(Bitmap.CompressFormat.Png!, 80, ms);
            return ms.ToArray();
        }
        catch { return null; }
    }

    /// <summary>Capture directly to ImageView — native path, no PNG encode/decode.</summary>
    public static void CaptureToImageView(int pxX, int pxY, int size)
    {
        if (_labelView == null || _loupeImageView == null) return;
        try
        {
            var bmp = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888!);
            using var canvas = new Canvas(bmp);
            canvas.Translate(-pxX + size / 2f, -pxY + size / 2f);
            _labelView.Draw(canvas);
            _loupeImageView.SetImageBitmap(bmp);
        }
        catch { /* best-effort */ }
    }
}
#endif
