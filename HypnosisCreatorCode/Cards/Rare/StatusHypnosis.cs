using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 状態異常催眠 — パワー。トランス状態の相手に状態異常・呪いをプレイ可能にする。
/// （戦闘中は対応カードをプレイ可能版へ置き換える。UGですべての相手にトランス2。）
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class StatusHypnosis() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    private const int UpgradedTrance = 2;

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<StatusHypnosisPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

        if (!IsUpgraded || CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
            await TranceCombat.ApplyTrance(choiceContext, enemy, UpgradedTrance, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}
