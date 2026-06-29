namespace RepeatSegment.Maui.Services;

/// <summary>Persistent recent files list (survives page recreation).</summary>
public static class RecentManager
{
    private static readonly string RecentPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RepeatSegment", "recent.txt");

    private static List<string> _files = new();

    public static IReadOnlyList<string> Files => _files;

    public static event Action? Changed;

    static RecentManager()
    {
        Load();
    }

    public static void AddFile(string filePath)
    {
        _files.RemoveAll(f => f == filePath);
        _files.Insert(0, filePath);
        if (_files.Count > 5) _files = _files.Take(5).ToList();
        Save();
        Changed?.Invoke();
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(RecentPath))
                _files = File.ReadAllLines(RecentPath)
                    .Where(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f))
                    .Take(5).ToList();
            else _files.Clear();
        }
        catch { _files.Clear(); }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RecentPath)!);
            File.WriteAllLines(RecentPath, _files);
        }
        catch { }
    }
}
