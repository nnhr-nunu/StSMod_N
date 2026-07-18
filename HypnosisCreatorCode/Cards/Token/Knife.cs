using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>ナイフ — ナイフと拳銃 が手札に加える小さな刃物。アブノーマル性癖。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Knife() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Token, TargetType.AnyEnemy,
    showInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
