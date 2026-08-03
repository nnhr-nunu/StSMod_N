using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カイザークラブ左右爪（Crusher / Rocket）は背景一体型。
/// 行動差し替え時に腕アニメの待機が残ると進行不能になりうるため、CTS と内部状態をリセットする。
/// </summary>
public static class KaiserClubBackgroundSafety
{
    private static readonly FieldInfo? CtsField = typeof(Creature).Assembly
        .GetType("MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds.NKaiserCrabBossBackground")?
        .GetField("_cts", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RightArmStateField = typeof(Creature).Assembly
        .GetType("MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds.NKaiserCrabBossBackground")?
        .GetField("_rightArmState", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly Type? RightArmStateType = typeof(Creature).Assembly
        .GetType("MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds.NKaiserCrabBossBackground+RightArmState");

    public static void StabilizeArm(Creature? creature)
    {
        if (!IntentOverwriteUnsafeMonsters.IsKaiserClubClaw(creature)) return;
        var monster = creature?.Monster;
        if (monster == null) return;

        try
        {
            var bgField = monster.GetType().GetField(
                "_background", BindingFlags.Instance | BindingFlags.NonPublic);
            var bg = bgField?.GetValue(monster);
            if (bg == null) return;

            if (CtsField?.GetValue(bg) is CancellationTokenSource cts)
            {
                try { cts.Cancel(); } catch { /* ignore */ }
            }

            if (RightArmStateField != null && RightArmStateType != null)
            {
                var defaultState = Enum.ToObject(RightArmStateType, 0);
                RightArmStateField.SetValue(bg, defaultState);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"KaiserClubBackgroundSafety failed: {e.Message}");
        }
    }
}
