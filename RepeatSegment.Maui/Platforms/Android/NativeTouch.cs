#if ANDROID
using Android.Views;

namespace RepeatSegment.Maui;

/// <summary>Native touch → pixel-relative coordinates for drag selection + pinch detection.</summary>
public static class NativeTouch
{
    public static void Attach(Microsoft.Maui.Controls.BoxView box, Action<float, float, int> touchCb, Action<float>? pinchCb = null, Action? pinchStartCb = null, Action? pinchEndCb = null)
    {
        void OnReady() { if (box.Handler?.PlatformView is Android.Views.View v) AttachNative(v, touchCb, pinchCb, pinchStartCb, pinchEndCb); }
        if (box.Handler != null) OnReady(); else box.HandlerChanged += (_, _) => OnReady();
    }
    static void AttachNative(Android.Views.View v, Action<float, float, int> touchCb, Action<float>? pinchCb, Action? pinchStartCb, Action? pinchEndCb)
    {
        v.Clickable = true; v.Focusable = true;
        v.SetOnTouchListener(new TL(touchCb, pinchCb, pinchStartCb, pinchEndCb, v));
    }
    class TL : Java.Lang.Object, Android.Views.View.IOnTouchListener
    {
        readonly Action<float, float, int> _touchCb;
        readonly Action<float>? _pinchCb;
        readonly Action? _pinchStartCb, _pinchEndCb;
        readonly Android.Views.View _v;
        private float _pinchBaseDist;
        private bool _pinching;

        public TL(Action<float, float, int> touchCb, Action<float>? pinchCb, Action? pinchStartCb, Action? pinchEndCb, Android.Views.View v)
        { _touchCb = touchCb; _pinchCb = pinchCb; _pinchStartCb = pinchStartCb; _pinchEndCb = pinchEndCb; _v = v; }

        public bool OnTouch(Android.Views.View? v, MotionEvent? e)
        {
            if (e == null) return false;
            int pc = e.PointerCount;

            if (pc >= 2 && _pinchCb != null)
            {
                // Multi-touch: suppress single-finger selection, compute pinch scale
                if (!_pinching)
                {
                    _pinching = true;
                    _pinchStartCb?.Invoke(); // notify — hide loupe, cancel selection
                }
                if (_v.Parent != null) _v.Parent.RequestDisallowInterceptTouchEvent(true);

                float dx = e.GetX(0) - e.GetX(1);
                float dy = e.GetY(0) - e.GetY(1);
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                var a = e.ActionMasked;
                if (a == MotionEventActions.PointerDown || a == MotionEventActions.Down)
                {
                    _pinchBaseDist = dist;
                }
                else if (a == MotionEventActions.Move && _pinchBaseDist > 0)
                {
                    // Cumulative scale from initial distance — NOT resetting base for smooth slow movements
                    float scale = dist / _pinchBaseDist;
                    _pinchCb(scale);
                }
                return true;
            }

            // Single finger: normal selection if not in pinch mode
            int sa = e.ActionMasked switch { MotionEventActions.Down => 0, MotionEventActions.Move => 1, MotionEventActions.Up or MotionEventActions.Cancel => 2, _ => -1 };
            if (sa < 0) return false;
            if (_pinching && sa == 2) { _pinching = false; _pinchEndCb?.Invoke(); return true; }
            if (_pinching) return true; // suppress single-finger events during pinch
            if (sa == 0 && _v.Parent != null) _v.Parent.RequestDisallowInterceptTouchEvent(true);
            _touchCb(e.GetX(), e.GetY(), sa);
            return true;
        }
    }
}
#endif
