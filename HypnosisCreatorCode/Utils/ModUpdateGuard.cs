using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 実行中のmod差し替えやPCK不整合を検知し、安全な画面で再起動を案内する。
/// 戦闘中にはダイアログを出さない。
/// </summary>
public static class ModUpdateGuard
{
    private const int CheckEveryFrames = 120;
    private const string Message =
        "Modの更新またはアセット読み込み不整合を検出しました。ゲームを再起動してください。";

    private static SceneTree? _tree;
    private static string? _installedFingerprint;
    private static string? _markerPath;
    private static int _frameCounter;
    private static bool _pending;
    private static bool _started;
    private static CanvasLayer? _notificationLayer;

    public static void Start()
    {
        if (_started) return;
        _started = true;

        _tree = Engine.GetMainLoop() as SceneTree;
        if (_tree == null)
        {
            MainFile.Logger.Warn("ModUpdateGuard: SceneTree was unavailable.");
            return;
        }

        _installedFingerprint = BuildFingerprint();
        _markerPath = Path.Combine(OS.GetUserDataDir(), "hypnosis_creator_mod_fingerprint.txt");

        var previous = ReadMarker();
        _pending = !string.IsNullOrEmpty(previous)
            && !string.Equals(previous, _installedFingerprint, StringComparison.Ordinal);

        if (string.IsNullOrEmpty(previous))
            WriteMarker(_installedFingerprint);

        _tree.ProcessFrame += OnProcessFrame;
    }

    /// <summary>必須フォールバックアセットまで読めない場合に通知を予約する。</summary>
    public static void ReportAssetMismatch()
    {
        if (_started)
            _pending = true;
    }

    private static void OnProcessFrame()
    {
        if (_tree == null) return;

        _frameCounter++;
        if (_frameCounter < CheckEveryFrames) return;
        _frameCounter = 0;

        var current = BuildFingerprint();
        if (!string.Equals(current, _installedFingerprint, StringComparison.Ordinal))
            _pending = true;

        if (_pending)
            ShowOnce();
    }

    private static void ShowOnce()
    {
        _pending = false;
        WriteMarker(BuildFingerprint());
        ShowNotification();
        MainFile.Logger.Warn(Message);
    }

    private static void ShowNotification()
    {
        if (_tree?.Root == null) return;
        if (_notificationLayer != null && GodotObject.IsInstanceValid(_notificationLayer))
            return;

        _notificationLayer = new CanvasLayer { Layer = 100 };
        var button = new Button
        {
            Text = Message,
            TooltipText = "クリックで閉じる",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Alignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(720f, 72f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        var viewportSize = _tree.Root.Size;
        var width = Mathf.Max(240f, Mathf.Min(720f, viewportSize.X - 24f));
        button.Size = new Vector2(width, 72f);
        button.Position = new Vector2(
            Mathf.Max(12f, (viewportSize.X - width) * 0.5f),
            24f);
        button.Pressed += DismissNotification;
        _notificationLayer.AddChild(button);
        _tree.Root.AddChild(_notificationLayer);
    }

    private static void DismissNotification()
    {
        if (_notificationLayer != null && GodotObject.IsInstanceValid(_notificationLayer))
            _notificationLayer.QueueFree();
        _notificationLayer = null;
    }

    private static string? ReadMarker()
    {
        try
        {
            return _markerPath != null && File.Exists(_markerPath)
                ? File.ReadAllText(_markerPath)
                : null;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ModUpdateGuard: marker read failed: {ex.Message}");
            return null;
        }
    }

    private static void WriteMarker(string fingerprint)
    {
        try
        {
            if (_markerPath == null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(_markerPath)!);
            File.WriteAllText(_markerPath, fingerprint);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ModUpdateGuard: marker write failed: {ex.Message}");
        }
    }

    private static string BuildFingerprint()
    {
        var assemblyPath = typeof(MainFile).Assembly.Location;
        if (string.IsNullOrEmpty(assemblyPath))
            return "assembly-location-unavailable";

        var directory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
        var files = new[]
        {
            assemblyPath,
            Path.ChangeExtension(assemblyPath, ".pck"),
            Path.Combine(directory, $"{MainFile.ModId}.json")
        };

        return string.Join(
            "|",
            files.Select(path =>
            {
                try
                {
                    var info = new FileInfo(path);
                    return $"{path}:{info.Exists}:{info.Length}:{info.LastWriteTimeUtc.Ticks}";
                }
                catch
                {
                    return $"{path}:unreadable";
                }
            }));
    }
}
