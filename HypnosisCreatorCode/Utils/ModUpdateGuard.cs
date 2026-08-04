using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Nodes;

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

        // NRun が存在する間は戦闘・ラン画面の可能性があるため通知しない。
        if (_pending && NRun.Instance == null)
            ShowOnce();
    }

    private static void ShowOnce()
    {
        _pending = false;
        WriteMarker(BuildFingerprint());
        OS.Alert(Message, "Hypno Creator");
        MainFile.Logger.Warn(Message);
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
