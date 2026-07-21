using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 味方ザップマシン（Zapbot ペット）の攻撃。
/// FromMonster は全敵攻撃固定のため、Attacker 設定＋ランダム単体狙いを組み立てる。
/// </summary>
internal static class AllyZapbotAttacks
{
    private static readonly FieldInfo? AttackerBackingField =
        AccessTools.Field(typeof(AttackCommand), "<Attacker>k__BackingField");

    private static readonly FieldInfo? AttackerAnimNameField =
        AccessTools.Field(typeof(AttackCommand), "_attackerAnimName");

    private static readonly FieldInfo? SourceTypeField =
        AccessTools.Field(typeof(AttackCommand), "_sourceType");

    private static readonly MethodInfo? ZapDamageGetter =
        AccessTools.PropertyGetter(typeof(Zapbot), "ZapDamage");

    /// <summary>Zapbot 本体と同じ基礎ダメージ（取れなければ 9）。</summary>
    public static decimal ResolveZapDamage(Creature zapbot)
    {
        if (zapbot.Monster is Zapbot zap && ZapDamageGetter != null)
            return Convert.ToDecimal(ZapDamageGetter.Invoke(zap, null));
        return 9m;
    }

    public static async Task Perform(PlayerChoiceContext choiceContext, Creature zapbot)
    {
        if (!zapbot.IsAlive) return;
        var combat = zapbot.CombatState;
        if (combat == null) return;
        if (combat.HittableEnemies.Count == 0) return;
        if (AttackerBackingField == null || AttackerAnimNameField == null || SourceTypeField == null)
            return;

        var damage = ResolveZapDamage(zapbot);
        var cmd = DamageCmd.Attack(damage);
        AttackerBackingField.SetValue(cmd, zapbot);
        // FromMonster 相当のソース／アニメ名だけ先に入れ、狙いだけランダム単体にする
        AttackerAnimNameField.SetValue(cmd, "Attack");
        SourceTypeField.SetValue(cmd, Enum.ToObject(SourceTypeField.FieldType, 2));

        await cmd
            .TargetingRandomOpponents(combat, allowDuplicates: false)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
    }
}
