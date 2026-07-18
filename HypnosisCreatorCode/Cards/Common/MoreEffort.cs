using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>もっとがんばる — このカードのコストをこの戦闘中 -1（繰り返しで0へ）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MoreEffort() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    // 自分対象のカウント: トランス解除なし（mechanics-lock）。コスト0まで待つ。
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new MegaCrit.Sts2.Core.Localization.DynamicVars.BlockVar(8M, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await MegaCrit.Sts2.Core.Commands.CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        // 次に備えて同名カードのコストを下げる（手札・捨て札・山札）
        foreach (var card in Owner.PlayerCombatState.AllCards.Where(c => c.GetType() == GetType()))
            card.EnergyCost.AddThisCombat(-1);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3M);
}
