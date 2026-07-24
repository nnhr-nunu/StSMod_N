using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 心臓レリック説明の {Damage:diff()} / {Block:diff()} に筋力・敏捷を反映する。
/// レリックはカードの UpdateCardPreview を通らないため、ホバー直前に PreviewValue を更新する。
/// </summary>
public static class HeartRelicPreview
{
    public static void Refresh(RelicModel relic)
    {
        switch (relic)
        {
            case StolenHeart stolen:
                RefreshStolen(stolen);
                break;
            case EnemyHeartRelic heart:
                RefreshEnemyHeart(heart);
                break;
        }
    }

    private static void RefreshStolen(StolenHeart stolen)
    {
        if (stolen.Owner?.Creature == null) return;
        if (!stolen.DynamicVars.TryGetValue("Block", out var blockVar)) return;

        var stacks = stolen.Owner.Relics.Count(r => r is StolenHeart);
        var baseBlock = Math.Max(stacks, 1) * 2m;
        var preview = HeartActivationHelpers.BlockAmountWithDexterity(stolen.Owner.Creature, baseBlock);
        blockVar.EnchantedValue = baseBlock;
        blockVar.PreviewValue = preview;

        if (stolen.DynamicVars.TryGetValue("Hearts", out var heartsVar))
        {
            heartsVar.EnchantedValue = stacks;
            heartsVar.PreviewValue = stacks;
        }
    }

    private static void RefreshEnemyHeart(EnemyHeartRelic heart)
    {
        var creature = heart.Owner?.Creature;
        var vars = heart.DynamicVars;

        if (vars.TryGetValue("Damage", out var damageVar) && creature != null)
        {
            var baseDmg = damageVar.BaseValue;
            var preview = PreviewRandomEnemyDamage(heart, creature, baseDmg);
            damageVar.EnchantedValue = baseDmg;
            damageVar.PreviewValue = preview;
        }

        if (vars.TryGetValue("Block", out var blockVar))
        {
            // Enchanted＝敏捷前（オビコは心臓数込み）、Preview＝敏捷込み
            var beforeDex = blockVar.BaseValue;
            if (heart is OvicopterHeart && heart.Owner != null)
            {
                var hearts = HeartInventory.CountHearts(heart.Owner);
                beforeDex = Math.Max(hearts, 1) * blockVar.BaseValue;
                if (vars.TryGetValue("Hearts", out var heartsVar))
                {
                    heartsVar.EnchantedValue = hearts;
                    heartsVar.PreviewValue = hearts;
                }
            }

            blockVar.EnchantedValue = beforeDex;
            blockVar.PreviewValue = creature == null
                ? beforeDex
                : HeartActivationHelpers.BlockAmountWithDexterity(creature, beforeDex);
        }
    }

    /// <summary>
    /// ランダム敵ダメージ心臓用。全ヒット可能敵のうち最小ダメージ（弱体差など）を表示する。
    /// target に自分を渡すと霊体など被ダメ修正が誤適用されるため、必ず敵を target にする。
    /// </summary>
    private static decimal PreviewRandomEnemyDamage(EnemyHeartRelic heart, Creature dealer, decimal baseDmg)
    {
        var combat = dealer.CombatState;
        if (combat == null)
            return baseDmg + dealer.GetPowerAmount<StrengthPower>();

        var enemies = combat.HittableEnemies.ToList();
        if (enemies.Count == 0)
            return baseDmg + dealer.GetPowerAmount<StrengthPower>();

        var min = decimal.MaxValue;
        foreach (var enemy in enemies)
        {
            var dmg = Hook.ModifyDamage(
                heart.Owner!.RunState,
                combat,
                target: enemy,
                dealer: dealer,
                damage: baseDmg,
                props: ValueProp.Move,
                cardSource: null!,
                cardPlay: null!,
                ModifyDamageHookType.All,
                CardPreviewMode.Normal,
                out _);
            if (dmg < min)
                min = dmg;
        }

        return min;
    }
}
