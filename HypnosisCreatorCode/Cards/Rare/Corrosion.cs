using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 侵食 — パワー。アタックカードをプレイするたび、ランダムな催眠カウントカードを手札に加える。
/// UGで、プレイしたアタックの性癖タグに一致するカウントカードを優先する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Corrosion() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<CorrosionPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

        if (IsUpgraded)
        {
            var power = Owner.Creature.GetPower<CorrosionPower>();
            if (power != null) power.PreferMatchingFetish = true;
        }
    }

    protected override void OnUpgrade() { }
}
