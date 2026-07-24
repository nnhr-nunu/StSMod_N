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
/// 心臓効果は右クリック発動（得た瞬間には発動しない）。UGで筋力2・スピード2。
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

        await ObtainRandomHearts(DynamicVars["Hearts"].IntValue);
    }

    private async Task ObtainRandomHearts(int count)
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
                var id = HeartRegistry.GetMonsterId(enemy);
                if (!string.IsNullOrEmpty(id))
                    combatKeys.Add(id);
            }
        }

        var pool = HeartRegistry.AllHeartTypes
            .Where(t => !ownedTypes.Contains(t))
            .Where(IsRareHeartType)
            .Where(t =>
            {
                var ids = HeartRegistry.GetMonsterIds(t);
                if (ids.Count == 0) return true;
                return !ids.Any(id => combatKeys.Contains(id));
            })
            .ToList();

        if (pool.Count == 0)
        {
            for (var i = 0; i < count; i++)
            {
                var obtained = await RelicCmd.Obtain<StolenHeart>(Owner);
                MarkTemporaryCombatRelic(obtained);
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

            MarkTemporaryCombatRelic(obtained);
        }
    }

    /// <summary>戦闘終了で失う一時所持。効果はレリック枠の右クリックで発動する。</summary>
    private static void MarkTemporaryCombatRelic(RelicModel? obtained)
    {
        if (obtained is HypnosisCreatorRelic hc)
            hc.RemoveAtCombatEnd = true;
    }

    private static bool IsRareHeartType(Type heartType)
    {
        // CustomRelic は Activator.CreateInstance 不可。全 EnemyHeart は希少に統一済み。
        return typeof(EnemyHeartRelic).IsAssignableFrom(heartType) && !heartType.IsAbstract;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1M);
        DynamicVars["DexterityPower"].UpgradeValueBy(1M);
    }
}
