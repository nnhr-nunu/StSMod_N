using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 自己暗示 — 筋力・スピードを得て、未所持かつ戦闘中敵以外の希少な心臓をランダム3つ入手（この戦闘のみ）。
/// 入手時に戦闘開始トリガー相当で即発動。非希少は抽選外。UGで筋力2・スピード2。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SelfSuggestion() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1M),
        new PowerVar<DexterityPower>(1M),
        new DynamicVar("Hearts", 3M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(choiceContext, self, DynamicVars["StrengthPower"].BaseValue, self, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, DynamicVars["DexterityPower"].BaseValue, self, this);

        await ObtainRandomHearts(choiceContext, DynamicVars["Hearts"].IntValue);
    }

    private async Task ObtainRandomHearts(PlayerChoiceContext choiceContext, int count)
    {
        var ownedTypes = Owner.Relics
            .OfType<EnemyHeartRelic>()
            .Select(r => r.GetType())
            .ToHashSet();

        var combatKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (CombatState != null)
        {
            foreach (var enemy in CombatState.HittableEnemies)
            {
                var id = enemy.Monster?.Id.Entry ?? enemy.ModelId.Entry;
                if (!string.IsNullOrEmpty(id))
                    combatKeys.Add(id);
            }
        }

        var pool = HeartRegistry.AllHeartTypes
            .Where(t => !ownedTypes.Contains(t))
            .Where(IsRareHeartType)
            .Where(t =>
            {
                var key = TryMonsterKey(t);
                if (key == null) return true;
                return !combatKeys.Any(ck => ck.Contains(key, StringComparison.OrdinalIgnoreCase));
            })
            .ToList();

        if (pool.Count == 0)
        {
            for (var i = 0; i < count; i++)
            {
                var obtained = await RelicCmd.Obtain<StolenHeart>(Owner);
                await MarkTemporaryAndMaybeActivate(choiceContext, Owner, obtained);
            }
            return;
        }

        var rng = Owner.RunState.Rng.CombatCardSelection;
        for (var i = 0; i < count && pool.Count > 0; i++)
        {
            var index = rng.NextInt(pool.Count);
            var type = pool[index];
            pool.RemoveAt(index);

            var canonical = ModelDb.AllRelics.FirstOrDefault(r => r.GetType() == type);
            RelicModel? obtained;
            if (canonical != null)
                obtained = await RelicCmd.Obtain(canonical.ToMutable(), Owner);
            else if (Activator.CreateInstance(type) is RelicModel created)
                obtained = await RelicCmd.Obtain(created, Owner);
            else
                continue;

            await MarkTemporaryAndMaybeActivate(choiceContext, Owner, obtained);
        }
    }

    /// <summary>
    /// 戦闘限定＋戦闘開始トリガー相当の即発動。
    /// （ターン1専用の FireCombatStartTrigger では、プレイ後に発動しないため）
    /// </summary>
    private static async Task MarkTemporaryAndMaybeActivate(
        PlayerChoiceContext choiceContext, Player player, RelicModel? obtained)
    {
        if (obtained is HypnosisCreatorRelic hc)
            hc.RemoveAtCombatEnd = true;

        if (obtained is EnemyHeartRelic heart && heart.IsRareHeart && !heart.IsUsedUp)
            await heart.ActivateAsync(choiceContext, player);
    }

    private static bool IsRareHeartType(Type heartType)
    {
        try
        {
            return Activator.CreateInstance(heartType) is EnemyHeartRelic { IsRareHeart: true };
        }
        catch
        {
            return false;
        }
    }

    private static string? TryMonsterKey(Type heartType)
    {
        try
        {
            if (Activator.CreateInstance(heartType) is EnemyHeartRelic sample)
                return sample.MonsterIdEntry;
        }
        catch
        {
            // ignore
        }

        return null;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1M);
        DynamicVars["DexterityPower"].UpgradeValueBy(1M);
    }
}
