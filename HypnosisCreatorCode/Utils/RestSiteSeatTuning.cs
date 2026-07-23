using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 篝火の席番号・マルチ人数に応じて立ち絵の位置と縮尺を調整する。
/// ソロ用の .tscn 調整値を基準に、席ごとのオフセットとマルチ時縮小を足す。
/// </summary>
public static class RestSiteSeatTuning
{
    private const float BaseRootScale = 0.76f;
    private const float MultiplayerScale = 0.88f;

    private static readonly FieldInfo CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")!;

    private readonly record struct SeatProfile(Vector2 VisualOffset, Vector2 RootOffset, float ScaleMul);

    /// <summary>Character_1〜4（index 0〜3）。本家コンテナ位置差を踏まえた仮調整。</summary>
    private static readonly SeatProfile[] Profiles =
    [
        new(Vector2.Zero, Vector2.Zero, 1f),
        new(new Vector2(36, 8), new Vector2(-52, 10), 0.94f),
        new(new Vector2(-24, 12), new Vector2(28, -28), 0.91f),
        new(new Vector2(32, 12), new Vector2(-40, -24), 0.91f),
    ];

    public static void Apply(NRestSiteCharacter character)
    {
        if (character.Player?.Character is not HcCharacter)
            return;

        var index = (int)CharacterIndexField.GetValue(character)!;
        if (index < 0 || index >= Profiles.Length)
            index = 0;

        var profile = Profiles[index];
        var playerCount = character.Player.RunState.Players.Count;
        var mpScale = playerCount > 1 ? MultiplayerScale : 1f;
        var scale = BaseRootScale * mpScale * profile.ScaleMul;
        character.Scale = new Vector2(scale, scale);
        character.Position += profile.RootOffset;

        var controlRoot = character.GetNodeOrNull<Control>("ControlRoot");
        if (controlRoot == null)
            return;

        ApplyOffset(controlRoot.GetNodeOrNull<Sprite2D>("Visuals"), profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%Hitbox"), profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleLeft"), profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleRight"), profile.VisualOffset);
    }

    private static void ApplyOffset(Node2D? node, Vector2 offset)
    {
        if (node == null || offset == Vector2.Zero)
            return;

        node.Position += offset;
    }

    private static void ApplyControlOffset(Control? control, Vector2 offset)
    {
        if (control == null || offset == Vector2.Zero)
            return;

        control.Position += offset;
    }
}
