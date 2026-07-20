using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>心臓レリック発動の共通ヘルパー（Composer 側と共有）。</summary>
internal static class HeartActivationHelpers
{
    public static async Task ActivateRareSelfPower<T>(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal amount)
        where T : PowerModel
    {
        heart.Flash();
        await PowerCmd.Apply<T>(ctx, player.Creature, amount, player.Creature, null!);
        heart.MarkUsed();
    }

    public static async Task ActivateRareSelfBlock(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal block)
    {
        heart.Flash();
        await CreatureCmd.GainBlock(player.Creature, block, ValueProp.Move, null);
        heart.MarkUsed();
    }

    public static async Task ActivateRareSelfHeal(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal heal)
    {
        heart.Flash();
        await CreatureCmd.Heal(player.Creature, heal);
        heart.MarkUsed();
    }

    public static async Task ActivateRareSelfMaxHp(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal maxHp)
    {
        heart.Flash();
        await CreatureCmd.GainMaxHp(player.Creature, maxHp);
        heart.MarkUsed();
    }

    public static async Task ActivateRareZeroCostSlimed(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player)
    {
        heart.Flash();
        var slime = ModelDb.Card<Slimed>().ToMutable();
        slime.Owner = player;
        slime.EnergyCost.SetThisCombat(0);
        await CardPileCmd.Add(slime, PileType.Hand, CardPilePosition.Bottom, heart);
        heart.MarkUsed();
    }

    public static async Task ActivateRareRandomEnemyPower<T>(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal amount)
        where T : PowerModel =>
        await ActivateRareRandomEnemyPowerTimes<T>(heart, ctx, player, amount, 1);

    /// <summary>同一ランダム敵へパワーを times 回付与（マイトの毒5×2 など）。</summary>
    public static async Task ActivateRareRandomEnemyPowerTimes<T>(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal amount, int times)
        where T : PowerModel
    {
        var enemy = PickRandomEnemy(player);
        if (enemy == null) return;

        heart.Flash();
        for (var i = 0; i < times; i++)
            await PowerCmd.Apply<T>(ctx, enemy, amount, player.Creature, null!);
        heart.MarkUsed();
    }

    public static async Task ActivateRareRandomEnemyDamage(
        EnemyHeartRelic heart, PlayerChoiceContext ctx, Player player, decimal damage, int hits = 1)
    {
        var enemy = PickRandomEnemy(player);
        if (enemy == null) return;

        heart.Flash();
        for (var i = 0; i < hits; i++)
        {
            await CreatureCmd.Damage(
                ctx, enemy, damage, ValueProp.Move, player.Creature, null, null);
        }
        heart.MarkUsed();
    }

    public static async Task ActivateRarePotion<T>(EnemyHeartRelic heart, Player player)
        where T : PotionModel
    {
        // 空きスロットがない場合は未発動のまま（CSV: シュリンカービートル等）
        var result = await PotionCmd.TryToProcure<T>(player);
        if (!result.success) return;

        heart.Flash();
        heart.MarkUsed();
    }

    public static Creature? PickRandomEnemy(Player player)
    {
        var combat = player.Creature.CombatState;
        if (combat == null) return null;

        var enemies = combat.HittableEnemies.ToList();
        if (enemies.Count == 0) return null;

        var rng = player.RunState.Rng.CombatCardSelection;
        return enemies[rng.NextInt(enemies.Count)];
    }

    public static async Task PassiveGold(EnemyHeartRelic heart, int gold)
    {
        if (heart.Owner == null) return;
        heart.Flash();
        await PlayerCmd.GainGold(gold, heart.Owner);
    }

    public static async Task PassiveMaxHp(EnemyHeartRelic heart, int maxHp)
    {
        if (heart.Owner == null) return;
        heart.Flash();
        await CreatureCmd.GainMaxHp(heart.Owner.Creature, maxHp);
    }

    public static async Task PassiveHeal(EnemyHeartRelic heart, decimal heal)
    {
        if (heart.Owner == null) return;
        heart.Flash();
        await CreatureCmd.Heal(heart.Owner.Creature, heal);
    }
}
