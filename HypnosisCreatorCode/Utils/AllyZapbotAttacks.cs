using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 味方ザップマシン（Zapbot ペット）の攻撃。
/// AttackCommand + SourceType.Monster は味方攻撃時に PlayerCreatures を狙うため、直接ダメージを与える。
/// </summary>
internal static class AllyZapbotAttacks
{
    private static readonly MethodInfo? ZapDamageGetter =
        AccessTools.PropertyGetter(typeof(Zapbot), "ZapDamage");

    /// <summary>
    /// Zapbot 本体と同じダメージ（通常 14、Deadly Enemies アセンション時 15）。
    /// プロパティが取れない場合のフォールバックは通常値 14。
    /// </summary>
    public static decimal ResolveZapDamage(Creature zapbot)
    {
        if (zapbot.Monster is Zapbot zap && ZapDamageGetter != null)
            return Convert.ToDecimal(ZapDamageGetter.Invoke(zap, null));
        return 14m;
    }

    public static async Task Perform(PlayerChoiceContext choiceContext, Creature zapbot)
    {
        if (!zapbot.IsAlive) return;
        var combat = zapbot.CombatState;
        if (combat == null || combat.HittableEnemies.Count == 0) return;

        var owner = zapbot.PetOwner;
        if (owner == null) return;

        var enemies = combat.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var rng = owner.RunState.Rng.CombatCardSelection;
        var target = enemies[rng.NextInt(enemies.Count)];
        var damage = ResolveZapDamage(zapbot);

        await CreatureCmd.Damage(
            choiceContext, target, damage, ValueProp.Move, zapbot, null, null);
    }
}
