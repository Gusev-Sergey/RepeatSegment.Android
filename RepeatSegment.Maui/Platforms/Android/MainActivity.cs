using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace RepeatSegment.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    // SAF (Storage Access Framework) file pick result handler
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == 1001 && resultCode == Result.Ok && data?.Data != null)
        {
            var uri = data.Data;
            // Copy URI content to a local temp file
            try
            {
                using var input = ContentResolver?.OpenInputStream(uri);
                if (input == null) return;
                string tmpPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"saf_import_{System.Guid.NewGuid()}.mp3");
                using var output = System.IO.File.Create(tmpPath);
                input.CopyTo(output);
                // Post to menu page to load the file
                Services.RecentManager.AddFile(tmpPath);
            }
            catch { /* best-effort */ }
        }
    }
}
