#if ANDROID
using Android.Views;

namespace RepeatSegment.Maui;

/// <summary>Native touch → pixel-relative coordinates for drag selection + pinch-zoom.</summary>
public static class NativeTouch
{
    public static event Action<float, float>? PinchStart;
    public static event Action<float, float>? PinchEnd;

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
        private bool _pinching;
        private float _cumulativeScale = 1f;

        public TL(Action<float, float, int> cb, Android.Views.View v) { _cb = cb; _v = v; }
        public bool OnTouch(Android.Views.View? v, MotionEvent? e)
        {
            if (e == null) return false;

            if (e.PointerCount >= 2)
            {
                // Pinch-zoom — calculate span ratio
                if (!_pinching)
                {
                    _pinching = true;
                    _cumulativeScale = 1f;
                    float cx = (e.GetX(0) + e.GetX(1)) / 2f;
                    float cy = (e.GetY(0) + e.GetY(1)) / 2f;
                    PinchStart?.Invoke(cx, cy);
                }
                float dx = e.GetX(0) - e.GetX(1);
                float dy = e.GetY(0) - e.GetY(1);
                float span = (float)System.Math.Sqrt(dx * dx + dy * dy);
                // Simplified: use span relative to 300px baseline
                _cumulativeScale = System.Math.Clamp(span / 300f, 0.5f, 3f);
                _cb(_cumulativeScale, 0f, 3); // action=3 → pinch scale
                return true;
            }
            if (_pinching)
            {
                _pinching = false;
                float cx = e.GetX(), cy = e.GetY();
                PinchEnd?.Invoke(cx, cy);
            }

            int a = e.ActionMasked switch { MotionEventActions.Down => 0, MotionEventActions.Move => 1, MotionEventActions.Up or MotionEventActions.Cancel => 2, _ => -1 };
            if (a < 0) return false;
            if (a == 0 && _v.Parent != null) _v.Parent.RequestDisallowInterceptTouchEvent(true);
            _cb(e.GetX(), e.GetY(), a);
            return true;
        }
    }
}
#endif
