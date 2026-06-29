using Android.App;
using Android.OS;
using Android.Widget;

namespace RepeatSegmentNative;

[Activity(Label = "RepeatSegment", MainLauncher = true, Theme = "@android:style/Theme.Material.NoActionBar")]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent,
                LinearLayout.LayoutParams.MatchParent)
        };
        layout.SetBackgroundColor(global::Android.Graphics.Color.ParseColor("#1E1E1E"));
        layout.SetGravity(global::Android.Views.GravityFlags.Center);

        var title = new TextView(this)
        {
            Text = "RepeatSegment",
            TextSize = 30,
        };
        title.SetTextColor(global::Android.Graphics.Color.White);

        var subtitle = new TextView(this)
        {
            Text = "Android Native — работает!",
            TextSize = 16,
        };
        subtitle.SetTextColor(global::Android.Graphics.Color.ParseColor("#888888"));

        layout.AddView(title);
        layout.AddView(subtitle);

        SetContentView(layout);
    }
}
