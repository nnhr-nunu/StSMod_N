using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>腹部への殴打 — 16ダメージ＋弱体2＋脆弱2。UGで弱体3・脆弱3・無慈悲（本家 Cruelty 同値）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbdominalStrike() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    /// <summary>本家アイアンクラッド「Cruelty」と同じ付与量。</summary>
    private const decimal CrueltyAmount = 25M;

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16M, ValueProp.Move),
        new PowerVar<VulnerablePower>(2M),
        new PowerVar<FrailPower>(2M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<VulnerablePower>();
            yield return HoverTipFactory.FromPower<FrailPower>();
            if (IsUpgraded)
                yield return HoverTipFactory.FromPower<CrueltyPower>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: VanillaAttackSfx.HeavyHitFile)
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, play.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(
            choiceContext, play.Target, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this);
        if (IsUpgraded)
            await PowerCmd.Apply<CrueltyPower>(
                choiceContext, Owner.Creature, CrueltyAmount, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1M);
        DynamicVars["FrailPower"].UpgradeValueBy(1M);
    }
}
