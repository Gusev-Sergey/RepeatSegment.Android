using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using AndroidX.Core.App;

namespace RepeatSegment.Maui.Services;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaPlayback)]
public class PlaybackService : Service
{
    private const int NOTIFY_ID = 1001;
    private const string CHANNEL_ID = "playback_channel";
    private MediaSession? _session;

    public override void OnCreate()
    {
        base.OnCreate();
        CreateChannel();
        _session = new MediaSession(this, "RepeatSegment");
        _session.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
        _session.SetCallback(new SessionCb());
        _session.Active = true;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
            StartForeground(NOTIFY_ID, BuildNotification(true, "RepeatSegment"), Android.Content.PM.ForegroundService.TypeMediaPlayback);
        else
            StartForeground(NOTIFY_ID, BuildNotification(true, "RepeatSegment"));
    }

    private void CreateChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var ch = new NotificationChannel(CHANNEL_ID, "Playback", NotificationImportance.Low)
            { Description = "RepeatSegment playback controls" };
            var nm = GetSystemService(NotificationService) as NotificationManager;
            nm?.CreateNotificationChannel(ch);
        }
    }

    public void Update(bool playing, string? title = null)
    {
        var nm = GetSystemService(NotificationService) as NotificationManager;
        nm?.Notify(NOTIFY_ID, BuildNotification(playing, title ?? "RepeatSegment"));
    }

    private Notification BuildNotification(bool playing, string title)
    {
        var b = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle(title)
            .SetContentText(playing ? "Playing" : "Paused")
            .SetSmallIcon(Android.Resource.Drawable.IcMediaPlay)
            .SetPriority(NotificationCompat.PriorityLow)
            .SetOngoing(playing);
        return b.Build();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        => StartCommandResult.Sticky;

    public override IBinder? OnBind(Intent? i) => null;
    public override void OnDestroy() { _session?.Release(); _session = null; base.OnDestroy(); }

    private class SessionCb : MediaSession.Callback
    {
        public override void OnPlay() => PlaybackBridge.Post("play");
        public override void OnPause() => PlaybackBridge.Post("pause");
        public override void OnSkipToNext() => PlaybackBridge.Post("next");
        public override void OnSkipToPrevious() => PlaybackBridge.Post("prev");
        public override void OnStop() => PlaybackBridge.Post("stop");
    }
}

public static class PlaybackBridge
{
    public static event Action<string>? Cmd;
    public static void Post(string c) => Cmd?.Invoke(c);
}

public static class PlaybackServiceManager
{
    private static Context? _ctx;
    public static void Init(Context c) => _ctx = c;

    public static void Start()
    {
        if (_ctx == null) return;
        var i = new Intent(_ctx, typeof(PlaybackService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O) _ctx.StartForegroundService(i);
        else _ctx.StartService(i);
    }

    public static void Stop()
    {
        if (_ctx == null) return;
        _ctx.StopService(new Intent(_ctx, typeof(PlaybackService)));
    }
}
