using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>ナイフと拳銃 — アブノーマル。ダメージ＋ナイフ生成＋アブノーマル目覚め。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class KnifeAndGun() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(11M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);

        if (CombatState != null)
        {
            var generated = CombatState.CreateCard<Knife>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, Owner);
        }

        FetishCombat.Awaken(play.Target, FetishType.Abnormal, Owner);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}
