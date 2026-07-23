using System.Reflection;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 篝火の席番号・マルチ人数に応じて立ち絵の位置と縮尺を調整する。
/// Mod設定の席別オフセットは RestSiteSeatStore 経由で上乗せする。
/// </summary>
public static class RestSiteSeatTuning
{
    private const float BaseRootScale = 0.76f;
    private const float MultiplayerScale = 0.88f;
    private const string BaseCapturedMeta = "hc_rest_base_captured";

    private static readonly FieldInfo CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")!;

    private readonly record struct SeatProfile(
        Vector2 VisualOffset,
        Vector2 RootOffset,
        float ScaleMul,
        bool HeadInwardPose);

    private static readonly SeatProfile[] Profiles =
    [
        new(Vector2.Zero, Vector2.Zero, 1f, false),
        new(new Vector2(-32, 10), new Vector2(-56, 12), 0.94f, true),
        new(new Vector2(24, 6), new Vector2(20, -22), 0.91f, true),
        new(new Vector2(-36, 8), new Vector2(-52, -18), 0.91f, true),
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

        if (useMpLayout && profile.HeadInwardPose)
            ApplyHeadInwardPose(controlRoot);

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
