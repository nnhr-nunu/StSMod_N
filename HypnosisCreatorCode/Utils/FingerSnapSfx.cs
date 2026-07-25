using Godot;
using HypnosisCreator.HypnosisCreatorCode;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// mod 同梱の <c>audio/FINGER_SNAP</c> を Godot で再生する。
/// FMOD の vanilla ヒット音とは別系統。
/// </summary>
public static class FingerSnapSfx
{
    private const string BaseName = "FINGER_SNAP";
    private static readonly string[] Extensions = [".ogg", ".wav", ".mp3"];
    private static AudioStream? _stream;
    private static bool _loadFailed;

    public static void PlayNormal() => Play(1f);

    /// <summary>多段ヒット用。段数が多いほど再生速度を上げる。</summary>
    public static void PlayForHit(int totalHits, int hitIndex)
    {
        var pitch = totalHits switch
        {
            <= 1 => 1f,
            <= 5 => 1.2f,
            <= 20 => 1.45f,
            <= 50 => 1.65f,
            _ => 1.85f
        };

        // 連打感のためヒットごとにわずかにピッチをずらす
        pitch += Math.Min(hitIndex - 1, 8) * 0.02f;
        Play(pitch);
    }

    private static void Play(float pitchScale)
    {
        var stream = EnsureStream();
        if (stream == null) return;

        var tree = Engine.GetMainLoop() as SceneTree;
        var root = tree?.Root;
        if (root == null) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            PitchScale = Math.Clamp(pitchScale, 0.5f, 2.5f),
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

    private static AudioStream? EnsureStream()
    {
        if (_stream != null) return _stream;
        if (_loadFailed) return null;

        foreach (var ext in Extensions)
        {
            var path = $"{MainFile.ResPath}/audio/{BaseName}{ext}";
            if (!ResourceLoader.Exists(path)) continue;

            _stream = GD.Load<AudioStream>(path);
            if (_stream != null) return _stream;
        }

        _loadFailed = true;
        MainFile.Logger.Warn(
            $"Finger snap sfx not found under {MainFile.ResPath}/audio/{BaseName} (ogg/wav/mp3)");
        return null;
    }
}
