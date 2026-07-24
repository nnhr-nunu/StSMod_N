using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル選択用。キャラ選択画面アイコンから顔付近を切り出したポートレート。
/// </summary>
public static class CognitiveCharacterFaces
{
    private static readonly Dictionary<string, Texture2D> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static Texture2D? GetFacePortrait(CharacterModel character)
    {
        var key = character.Id.Entry;
        if (Cache.TryGetValue(key, out var cached) && GodotObject.IsInstanceValid(cached))
            return cached;

        try
        {
            var source = character.CharacterSelectIcon as Texture2D
                         ?? GD.Load<Texture2D>(CharacterSelectIconPath(character));
            if (source == null) return null;

            var face = CropFace(source);
            Cache[key] = face;
            return face;
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Cognitive character face load failed ({key}): {e.Message}");
            return null;
        }
    }

    /// <summary>パワーアイコン用。選んだキャラのセレクト画面アイコン。</summary>
    public static string CharacterSelectIconPath(CharacterModel character) =>
        $"res://images/packed/character_select/char_select_{character.Id.Entry.ToLowerInvariant()}.png";

    public static CharacterModel? CharacterForFormType(Type formCardType) => formCardType.Name switch
    {
        nameof(MegaCrit.Sts2.Core.Models.Cards.DemonForm) => ModelDb.Character<Ironclad>(),
        nameof(MegaCrit.Sts2.Core.Models.Cards.SerpentForm) => ModelDb.Character<Silent>(),
        nameof(MegaCrit.Sts2.Core.Models.Cards.VoidForm) => ModelDb.Character<Regent>(),
        nameof(MegaCrit.Sts2.Core.Models.Cards.ReaperForm) => ModelDb.Character<Necrobinder>(),
        nameof(MegaCrit.Sts2.Core.Models.Cards.EchoForm) => ModelDb.Character<Defect>(),
        _ => null
    };

    private static Texture2D CropFace(Texture2D source)
    {
        var size = source.GetSize();
        // 縦長セレクトアイコンの上部（顔）をカード枠向けに切り出す
        var cropW = size.X * 0.92f;
        var cropH = size.Y * 0.52f;
        var x = (size.X - cropW) * 0.5f;
        var y = size.Y * 0.06f;

        var atlas = new AtlasTexture
        {
            Atlas = source,
            Region = new Rect2(x, y, cropW, cropH)
        };
        return atlas;
    }
}
