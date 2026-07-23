using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 篝火の席番号・マルチ人数に応じて立ち絵の位置と縮尺を調整する。
/// ソロ（index 0）は .tscn の焚き火向きポーズ。マルチ index 1〜3 は頭側を内側へ向け、
/// 足が隣席に刺さらず肩身を寄せ合う見え方にする。
/// </summary>
public static class RestSiteSeatTuning
{
    private const float BaseRootScale = 0.76f;
    private const float MultiplayerScale = 0.88f;

    private static readonly FieldInfo CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")!;

    private readonly record struct SeatProfile(
        Vector2 VisualOffset,
        Vector2 RootOffset,
        float ScaleMul,
        bool HeadInwardPose);

    /// <summary>Character_1〜4（index 0〜3）。HeadInwardPose はマルチ時のみ有効。</summary>
    private static readonly SeatProfile[] Profiles =
    [
        new(Vector2.Zero, Vector2.Zero, 1f, false),
        new(new Vector2(-32, 10), new Vector2(-56, 12), 0.94f, true),
        new(new Vector2(24, 6), new Vector2(20, -22), 0.91f, true),
        new(new Vector2(-36, 8), new Vector2(-52, -18), 0.91f, true),
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

        if (playerCount > 1 && profile.HeadInwardPose)
            ApplyHeadInwardPose(controlRoot);

        var visuals = controlRoot.GetNodeOrNull<Sprite2D>("Visuals");
        ApplyOffset(visuals, profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%Hitbox"), profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleLeft"), profile.VisualOffset);
        ApplyControlOffset(controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleRight"), profile.VisualOffset);
    }

    /// <summary>
    /// 本家 FlipX の代わりに ControlRoot を反転し、頭側が内側・足が外側を向くポーズにする。
    /// index 2 は本家では反転されないため、ここで初めて向きを変える。
    /// </summary>
    private static void ApplyHeadInwardPose(Control controlRoot)
    {
        var scale = controlRoot.Scale;
        controlRoot.Scale = new Vector2(-Mathf.Abs(scale.X), scale.Y);
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
