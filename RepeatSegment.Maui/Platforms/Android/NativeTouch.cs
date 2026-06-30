#if ANDROID
using Android.Views;

namespace RepeatSegment.Maui;

/// <summary>Native touch → pixel-relative coordinates for drag selection.</summary>
public static class NativeTouch
{
    public static void Attach(Microsoft.Maui.Controls.BoxView box, Action<float, float, int> cb)
    {
        void OnReady() { if (box.Handler?.PlatformView is Android.Views.View v) AttachNative(v, cb); }
        if (box.Handler != null) OnReady(); else box.HandlerChanged += (_, _) => OnReady();
    }
    static void AttachNative(Android.Views.View v, Action<float, float, int> cb)
    {
        v.Clickable = true; v.Focusable = true;
        v.SetOnTouchListener(new TL(cb, v));
    }
    class TL : Java.Lang.Object, Android.Views.View.IOnTouchListener
    {
        readonly Action<float, float, int> _cb;
        readonly Android.Views.View _v;
        public TL(Action<float, float, int> cb, Android.Views.View v) { _cb = cb; _v = v; }
        public bool OnTouch(Android.Views.View? v, MotionEvent? e)
        {
            if (e == null) return false;
            int a = e.ActionMasked switch { MotionEventActions.Down => 0, MotionEventActions.Move => 1, MotionEventActions.Up or MotionEventActions.Cancel => 2, _ => -1 };
            if (a < 0) return false;
            if (a == 0 && _v.Parent != null) _v.Parent.RequestDisallowInterceptTouchEvent(true);
            _cb(e.GetX(), e.GetY(), a);
            return true;
        }
    }
}
#endif
