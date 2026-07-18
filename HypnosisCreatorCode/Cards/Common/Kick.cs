using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>足蹴 — SM/DomSubアタック。10ダメージ。SMとDomSubを目覚めさせる。UGで山札に1枚追加。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Kick() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm, FetishType.DomSub];
    public override bool? FetishHitPerTypeOverride => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);

        FetishCombat.Awaken(play.Target, FetishType.Sm, Owner);
        FetishCombat.Awaken(play.Target, FetishType.DomSub, Owner);
        await ResolveFetishOnTarget(choiceContext, play);

        if (IsUpgraded)
        {
            var copy = ModelDb.Card<Kick>().ToMutable();
            await CardPileCmd.Add(copy, PileType.Draw, CardPilePosition.Random, this, false);
        }
    }
}
