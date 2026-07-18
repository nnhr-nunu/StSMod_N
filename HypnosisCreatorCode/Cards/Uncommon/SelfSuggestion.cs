using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 自己暗示 — 筋力・スピードを得て、未所持かつ戦闘中敵以外の心臓をランダム3つ入手。
/// FireCombatStartTrigger=true。UGで筋力2・スピード2。
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

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(choiceContext, self, DynamicVars["StrengthPower"].BaseValue, self, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, DynamicVars["DexterityPower"].BaseValue, self, this);

        // HeartRegistry 抽選（所持済み・戦闘中敵の心臓を除外）
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
                var id = enemy.Monster?.Id.Entry ?? enemy.ModelId.Entry;
                if (!string.IsNullOrEmpty(id))
                    combatKeys.Add(id);
            }
        }

        var pool = HeartRegistry.AllHeartTypes
            .Where(t => !ownedTypes.Contains(t))
            .Where(t =>
            {
                var resolved = HeartRegistry.ResolveHeartType(
                    // 型から MonsterIdEntry を得て戦闘中キーと照合
                    TryMonsterKey(t) ?? "");
                // 戦闘中敵に対応する心臓は除外
                var key = TryMonsterKey(t);
                if (key == null) return true;
                return !combatKeys.Any(ck => ck.Contains(key, StringComparison.OrdinalIgnoreCase));
            })
            .ToList();

        if (pool.Count == 0)
        {
            // レジストリ未整備時のフォールバック
            for (var i = 0; i < count; i++)
                await RelicCmd.Obtain<StolenHeart>(Owner);
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

            if (obtained is EnemyHeartRelic heart)
                heart.FireCombatStartTrigger = true;
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
