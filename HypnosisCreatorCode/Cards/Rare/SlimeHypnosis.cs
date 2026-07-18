using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// スライム催眠 — CSV: 専用ビジュアル演出（対象をスライムに包み込む見た目）を想定。
/// 見た目差し替えは task.md 記載の別タスク。効果は暫定で沼＋弱体の複合デバフ。
/// TODO: 専用VFX/ポートレート差し替えが確定したら追加する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SlimeHypnosis() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Bog", 2M),
        new PowerVar<WeakPower>(2M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<BogPower>(
            choiceContext, play.Target, DynamicVars["Bog"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["Bog"].UpgradeValueBy(2M);
}
