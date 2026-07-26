using Godot;
using HypnosisCreator.HypnosisCreatorCode;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>ティンシャ攻撃用 <c>audio/tingshaws</c> を Godot で再生する。</summary>
public static class TingshaSfx
{
    private const string BaseName = "tingshaws";
    private static readonly string[] Extensions = [".ogg", ".wav", ".mp3"];
    private static AudioStream? _stream;
    private static bool _loadFailed;

    public static void Play() => PlayStream(EnsureStream(), 1f);

    private static void PlayStream(AudioStream? stream, float pitchScale)
    {
        if (stream == null) return;

        var root = (Engine.GetMainLoop() as SceneTree)?.Root;
        if (root == null) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            PitchScale = pitchScale,
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
            $"Tingsha sfx not found under {MainFile.ResPath}/audio/{BaseName} (ogg/wav/mp3)");
        return null;
    }
}
