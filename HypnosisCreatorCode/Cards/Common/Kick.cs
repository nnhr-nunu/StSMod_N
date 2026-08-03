using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>足蹴 — SM/DomSubアタック。10ダメージ。UGでプレイ後山札へ（説明は UpgradeDescriptionHooks）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Kick() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm, FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: VanillaAttackSfx.HeavyHitFile)
            .Execute(choiceContext);

        // 先に既存性癖への刺さりを解決してから目覚めさせる
        // （目覚め直後に刺さるとアブノーマル敵にも必ず破滅が乗る）
        await ResolveFetishOnTarget(choiceContext, play);
        await FetishCombat.AwakenAsync(choiceContext, play.Target, FetishType.Sm, Owner);
        await FetishCombat.AwakenAsync(choiceContext, play.Target, FetishType.DomSub, Owner);

        EnergyCost.AddThisCombat(-1);
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        IsUpgraded
            ? new CardLocation(Owner, PileType.Draw, CardPilePosition.Random)
            : base.GetResultLocationForCardPlay();
}
