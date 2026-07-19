using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心停止催眠 — カウント。指定ターン後に対象を即死させる（ボスは2倍のターン数）。トランス1。
/// UG: 心臓停止時に追加のレリック報酬を獲得。
/// TODO: sts2 側に明確なボス判定APIが見つからないため、最大HP&gt;=100の簡易判定で近似している。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CardiacArrestHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const int BossMaxHpThreshold = 100;

    // 説明全体を差し替えない（[gold]カウント・性癖プレフィックスを消さない）
    private const string JpnUpgradeFrom = "相手の心臓が止まる。";
    private const string JpnUpgradeTo = "相手の心臓が止まり[green]追加のレリック報酬を獲得する[/green]。";
    private const string EngUpgradeFrom = "their heart stops.";
    private const string EngUpgradeTo = "their heart stops and [green]you gain an extra Relic reward[/green].";

    static CardiacArrestHypnosis()
    {
        DescriptionOverrides.CustomizeDescriptionPost += ApplyUpgradeWording;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Turns", 3M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<CardiacArrestPower>()];

    private static void ApplyUpgradeWording(CardModel card, Creature? target, ref string description)
    {
        if (card is not CardiacArrestHypnosis { IsUpgraded: true }) return;

        if (description.Contains("[green]追加のレリック報酬を獲得する[/green]", StringComparison.Ordinal)
            || description.Contains("[green]you gain an extra Relic reward[/green]", StringComparison.OrdinalIgnoreCase))
            return;

        if (description.Contains(JpnUpgradeFrom, StringComparison.Ordinal))
        {
            description = description.Replace(JpnUpgradeFrom, JpnUpgradeTo, StringComparison.Ordinal);
            return;
        }

        if (description.Contains(EngUpgradeFrom, StringComparison.OrdinalIgnoreCase))
            description = description.Replace(EngUpgradeFrom, EngUpgradeTo, StringComparison.OrdinalIgnoreCase);
    }

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
