using Godot;
using HypnosisCreator.HypnosisCreatorCode;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// mod 同梱の <c>audio/FINGER_SNAP</c> / <c>FINGER_SNAP_short</c> を Godot で再生する。
/// FMOD の vanilla ヒット音とは別系統。
/// </summary>
public static class FingerSnapSfx
{
    private const string NormalBaseName = "FINGER_SNAP";
    private const string ShortBaseName = "FINGER_SNAP_short";
    private static readonly string[] Extensions = [".ogg", ".wav", ".mp3"];

    private static AudioStream? _normalStream;
    private static AudioStream? _shortStream;
    private static bool _normalLoadFailed;
    private static bool _shortLoadFailed;

    public static void PlayNormal() => Play(EnsureNormalStream(), 1f);

    /// <summary>多段ヒット用。短い版を等速付近で連続再生する。</summary>
    public static void PlayForHit(int totalHits, int hitIndex)
    {
        if (totalHits <= 1)
        {
            PlayNormal();
            return;
        }

        // 短い版を基本等速。連打感はごくわずかなピッチ差のみ（高音化しすぎない）
        var pitch = 1f + Math.Min(hitIndex - 1, 6) * 0.01f;
        Play(EnsureShortStream(), pitch);
    }

    private static void Play(AudioStream? stream, float pitchScale)
    {
        if (stream == null) return;

        var tree = Engine.GetMainLoop() as SceneTree;
        var root = tree?.Root;
        if (root == null) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            PitchScale = Math.Clamp(pitchScale, 0.9f, 1.15f),
            VolumeDb = -2f
        };

        root.AddChild(player);
        player.Play();
        player.Finished += () =>
        {
            if (GodotObject.IsInstanceValid(player))
                player.QueueFree();
        };
    }

    private static AudioStream? EnsureNormalStream() =>
        LoadStream(NormalBaseName, ref _normalStream, ref _normalLoadFailed);

    private static AudioStream? EnsureShortStream()
    {
        var stream = LoadStream(ShortBaseName, ref _shortStream, ref _shortLoadFailed);
        if (stream != null) return stream;

        MainFile.Logger.Warn(
            $"Short finger snap sfx not found under {MainFile.ResPath}/audio/{ShortBaseName}; falling back to normal clip.");
        return EnsureNormalStream();
    }

    private static AudioStream? LoadStream(
        string baseName, ref AudioStream? cache, ref bool loadFailed)
    {
        if (cache != null) return cache;
        if (loadFailed) return null;

        foreach (var ext in Extensions)
        {
            var path = $"{MainFile.ResPath}/audio/{baseName}{ext}";
            if (!ResourceLoader.Exists(path)) continue;

            cache = GD.Load<AudioStream>(path);
            if (cache != null) return cache;
        }

        loadFailed = true;
        if (baseName == NormalBaseName)
        {
            MainFile.Logger.Warn(
                $"Finger snap sfx not found under {MainFile.ResPath}/audio/{baseName} (ogg/wav/mp3)");
        }

        return null;
    }
}
