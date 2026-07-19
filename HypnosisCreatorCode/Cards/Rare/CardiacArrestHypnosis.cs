using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心停止催眠 — カウント。指定ターン後に対象を即死させる（ボスは2倍のターン数）。トランス1。
/// UG: 心臓停止時に追加のレリック報酬を獲得（説明は UpgradeDescriptionHooks）。
/// TODO: sts2 側に明確なボス判定APIが見つからないため、最大HP&gt;=100の簡易判定で近似している。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CardiacArrestHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const int BossMaxHpThreshold = 100;

    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Turns", 3M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<CardiacArrestPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var isBoss = play.Target.MaxHp >= BossMaxHpThreshold;
        var turns = DynamicVars["Turns"].IntValue * (isBoss ? 2 : 1);

        await PowerCmd.Apply<CardiacArrestPower>(choiceContext, play.Target, turns, Owner.Creature, this);
        var power = play.Target.GetPower<CardiacArrestPower>();
        if (power != null)
        {
            power.GrantBonusRelic = IsUpgraded;
            power.BonusRelicPlayer = Owner;
        }

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
