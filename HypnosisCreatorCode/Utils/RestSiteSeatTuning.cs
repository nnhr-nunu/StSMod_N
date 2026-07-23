using System.Reflection;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 篝火の席番号・マルチ人数に応じて立ち絵の位置と縮尺を調整する。
/// 本家 Character_1〜4（index 0〜3）の実座標に合わせ、焚き火方向を向くよう左右反転する。
/// </summary>
public static class RestSiteSeatTuning
{
    private const float BaseRootScale = 0.76f;
    private const float MultiplayerScale = 0.88f;
    /// <summary>右側席（1,3）の立ち絵基準X。本家コンテナ位置に加え、シーン内ローカルで右へ寄せる。</summary>
    private const float RightSeatBaseVisualX = 1400f;
    private const string BaseCapturedMeta = "hc_rest_base_captured";

    private static readonly FieldInfo CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")!;

  /// <summary>
    /// VisualOffset / RootOffset はコード側の構造調整。細かい位置は RestSiteSeatStore（Mod設定）。
    /// MirrorForFire: 左側の席は true（素材は左向きのため反転して火を向く）、右側の席は false。
    /// </summary>
    private readonly record struct SeatProfile(
        Vector2 VisualOffset,
        Vector2 RootOffset,
        float ScaleMul,
        bool MirrorForFire);

    /// <summary>右側の席（1,3）は大きく +X してから Mod設定で微調整。左側（2）は反転。</summary>
    private static readonly SeatProfile[] Profiles =
    [
        new(Vector2.Zero, Vector2.Zero, 1f, false),
        new(new Vector2(RightSeatBaseVisualX, 0), new Vector2(0, 14), 0.94f, false),
        new(Vector2.Zero, new Vector2(0, -18), 0.91f, true),
        new(new Vector2(RightSeatBaseVisualX, 0), new Vector2(0, -16), 0.91f, false),
    ];

    public static void ReapplyAll()
    {
        var count = 0;
        foreach (var character in FindRestSiteCharacters())
        {
            Apply(character);
            count++;
        }

        if (count == 0)
            MainFile.Logger.Info("VisualTuner: rest site character not found (open rest site to adjust).");
    }

    public static void Apply(NRestSiteCharacter character)
    {
        if (character.Player?.Character is not HcCharacter)
            return;

        var actualIndex = (int)CharacterIndexField.GetValue(character)!;
        if (actualIndex < 0 || actualIndex >= Profiles.Length)
            actualIndex = 0;

        var controlRoot = character.GetNodeOrNull<Control>("ControlRoot");
        var visuals = controlRoot?.GetNodeOrNull<Sprite2D>("Visuals");
        if (controlRoot == null || visuals == null)
            return;

        var hitbox = controlRoot.GetNodeOrNull<Control>("%Hitbox");
        var thoughtLeft = controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleLeft");
        var thoughtRight = controlRoot.GetNodeOrNull<Control>("%ThoughtBubbleRight");
        ResetToBase(character, controlRoot, visuals, hitbox, thoughtLeft, thoughtRight);

        var effectiveIndex = ResolveEffectiveSeatIndex(actualIndex);
        var profile = Profiles[effectiveIndex];
        var playerCount = character.Player.RunState.Players.Count;
        var useMpLayout = playerCount > 1 || (UsePreviewLayout() && effectiveIndex >= 1);
        var mpScale = useMpLayout ? MultiplayerScale : 1f;
        var scale = BaseRootScale * mpScale * profile.ScaleMul;
        character.Scale = new Vector2(scale, scale);
        character.Position += profile.RootOffset;

        var mirror = effectiveIndex == 0 ? false : profile.MirrorForFire;
        ApplyMirror(controlRoot, mirror);

        var configOffset = RestSiteSeatStore.Get(effectiveIndex).ToVector2();
        var totalVisualOffset = profile.VisualOffset + configOffset;
        ApplyOffset(visuals, totalVisualOffset);
        ApplyControlOffset(hitbox, totalVisualOffset);
        ApplyControlOffset(thoughtLeft, totalVisualOffset);
        ApplyControlOffset(thoughtRight, totalVisualOffset);
    }

    private static int ResolveEffectiveSeatIndex(int actualIndex)
    {
        if (!UsePreviewLayout())
            return actualIndex;

        return RestSiteSeatStore.ClampSeat((int)HypnosisCreatorConfig.RestSitePreviewSeat);
    }

    private static bool UsePreviewLayout() => HypnosisCreatorConfig.RestSiteUsePreviewLayout > 0.5;

    private static void ResetToBase(
        NRestSiteCharacter character,
        Control controlRoot,
        Sprite2D visuals,
        Control? hitbox,
        Control? thoughtLeft,
        Control? thoughtRight)
    {
        if (!character.HasMeta(BaseCapturedMeta))
        {
            character.SetMeta(BaseCapturedMeta, true);
            character.SetMeta("hc_rest_root_pos", character.Position);
            character.SetMeta("hc_rest_root_scale", character.Scale);
            character.SetMeta("hc_rest_cr_scale", controlRoot.Scale);
            character.SetMeta("hc_rest_visual_pos", visuals.Position);
            if (hitbox != null) character.SetMeta("hc_rest_hitbox_pos", hitbox.Position);
            if (thoughtLeft != null) character.SetMeta("hc_rest_thought_l_pos", thoughtLeft.Position);
            if (thoughtRight != null) character.SetMeta("hc_rest_thought_r_pos", thoughtRight.Position);
            return;
        }

        character.Position = (Vector2)character.GetMeta("hc_rest_root_pos");
        character.Scale = (Vector2)character.GetMeta("hc_rest_root_scale");
        controlRoot.Scale = (Vector2)character.GetMeta("hc_rest_cr_scale");
        visuals.Position = (Vector2)character.GetMeta("hc_rest_visual_pos");
        if (hitbox != null && character.HasMeta("hc_rest_hitbox_pos"))
            hitbox.Position = (Vector2)character.GetMeta("hc_rest_hitbox_pos");
        if (thoughtLeft != null && character.HasMeta("hc_rest_thought_l_pos"))
            thoughtLeft.Position = (Vector2)character.GetMeta("hc_rest_thought_l_pos");
        if (thoughtRight != null && character.HasMeta("hc_rest_thought_r_pos"))
            thoughtRight.Position = (Vector2)character.GetMeta("hc_rest_thought_r_pos");
    }

    /// <summary>本家 FlipX を打ち消し、席ごとの向きを明示する。</summary>
    private static void ApplyMirror(Control controlRoot, bool mirrored)
    {
        var scale = controlRoot.Scale;
        controlRoot.Scale = new Vector2(mirrored ? -Mathf.Abs(scale.X) : Mathf.Abs(scale.X), scale.Y);
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

    private static IEnumerable<NRestSiteCharacter> FindRestSiteCharacters()
    {
        var root = (Engine.GetMainLoop() as SceneTree)?.Root;
        if (root == null)
            yield break;

        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node is NRestSiteCharacter character)
                yield return character;

            foreach (var child in node.GetChildren())
                stack.Push(child);
        }
    }
}
