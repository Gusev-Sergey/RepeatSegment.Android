using Android.App;
using Android.OS;
using Android.Widget;
using Graphics = Android.Graphics;
using Views = Android.Views;

namespace RepeatSegment.Droid;

[Activity(Label = "RepeatSegment", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new LinearLayout.LayoutParams(-1, -1)
        };
        layout.SetBackgroundColor(Graphics.Color.ParseColor("#1E1E1E"));
        layout.SetGravity(Views.GravityFlags.Center);

        var title = new TextView(this)
        {
            Text = "RepeatSegment",
            TextSize = 30
        };
        title.SetTextColor(Graphics.Color.White);

        var subtitle = new TextView(this)
        {
            Text = "Droid Native",
            TextSize = 16
        };
        subtitle.SetTextColor(Graphics.Color.ParseColor("#888888"));

        layout.AddView(title);
        layout.AddView(subtitle);
        SetContentView(layout);
    }
}
