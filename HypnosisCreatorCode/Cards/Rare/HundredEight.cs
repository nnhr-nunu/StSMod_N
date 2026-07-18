using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 壱佰捌煩悩 — 全性癖タグ。敵全体の性癖を目覚めさせ、プレイするたびコストが+1される。
/// コストが3に達すると108ダメージを与えて廃棄する。性癖は種類ごとに個別で刺さる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HundredEight() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    private const int FinalCostThreshold = 3;
    private const decimal FinalDamage = 108M;

    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Sm, FetishType.DomSub, FetishType.Abnormal, FetishType.Trance];

    public override bool AlwaysHitsFetish => true;
    public override bool? FetishHitPerTypeOverride => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (CombatState != null)
            foreach (var enemy in CombatState.HittableEnemies.ToList())
                FetishCombat.AwakenAll(enemy, Owner);

        var resolvedCost = EnergyCost.GetResolved();
        var damage = resolvedCost >= FinalCostThreshold ? FinalDamage : DynamicVars.Damage.BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        await ResolveFetishOnAllEnemies(choiceContext, play);

        if (resolvedCost < FinalCostThreshold)
            EnergyCost.AddThisCombat(1);
    }

    /// <summary>
    /// コスト3に達した最終カットではプレイ後に廃棄する。
    /// コスト判定は OnPlay より前に確定させる必要があるため、こちらで上書きする。
    /// </summary>
    protected override CardLocation GetResultLocationForCardPlay() =>
        EnergyCost.GetResolved() >= FinalCostThreshold
            ? new CardLocation(Owner, PileType.Exhaust, CardPilePosition.Bottom)
            : base.GetResultLocationForCardPlay();
}
