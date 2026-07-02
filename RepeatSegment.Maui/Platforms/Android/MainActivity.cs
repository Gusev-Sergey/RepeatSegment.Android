using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace RepeatSegment.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int REQUEST_PICK_AUDIO = 1001;
    public static event Action<string?>? FilePicked;

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == REQUEST_PICK_AUDIO && resultCode == Result.Ok && data?.Data != null)
        {
            string? path = ResolveContentPath(data.Data);
            FilePicked?.Invoke(path);
        }
        else
        {
            FilePicked?.Invoke(null);
        }
    }

    private string? ResolveContentPath(Android.Net.Uri uri)
    {
        // Try DocumentsProvider
        string? docId = Android.Provider.DocumentsContract.GetDocumentId(uri);
        if (docId != null)
        {
            int colon = docId.IndexOf(':');
            string docIdPart = colon >= 0 ? docId.Substring(colon + 1) : docId;

            string tryPath = System.IO.Path.Combine("/storage/emulated/0", docIdPart);
            if (System.IO.File.Exists(tryPath)) return tryPath;

            tryPath = System.IO.Path.Combine("/sdcard", docIdPart);
            if (System.IO.File.Exists(tryPath)) return tryPath;
        }

        // Fallback via cursor
        try
        {
            string[] proj = { "_data" };
            using var cursor = ContentResolver?.Query(uri, proj, null, null, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                int col = cursor.GetColumnIndex("_data");
                if (col >= 0)
                {
                    string p = cursor.GetString(col)!;
                    if (!string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
                        return p;
                }
            }
        }
        catch { }

        return null;
    }

    public static void PickAudioFile(Activity activity)
    {
        var intent = new Intent(Intent.ActionOpenDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("audio/*");
        activity.StartActivityForResult(intent, REQUEST_PICK_AUDIO);
    }
}
