using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 練達 — パワー。有効な間にプレイしたカウントカードを、戦闘終了時に永続アップグレードする。
/// UGで、戦闘終了後に催眠系カウントカード報酬を追加する。コストは2のまま。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mastery() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var power = await PowerCmd.Apply<MasteryPower>(
            choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        if (power != null && IsUpgraded)
            power.AddHypnosisCountCardReward = true;
    }

    protected override void OnUpgrade() { }
}
