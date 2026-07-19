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
using MegaCrit.Sts2.Core.Localization;
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
    private const string JpnUpgradedDescription =
        "カウント。3ターン後、相手の心臓が止まり追加のレリック報酬を獲得する。ボスの場合は2倍のターン数になる。トランス1。";
    private const string EngUpgradeFrom = "their heart stops.";
    private const string EngUpgradeTo = "their heart stops and you gain an extra Relic reward.";

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

        if (IsJapaneseUi())
        {
            description = JpnUpgradedDescription;
            return;
        }

        if (description.Contains(EngUpgradeTo, StringComparison.OrdinalIgnoreCase)) return;
        description = description.Replace(EngUpgradeFrom, EngUpgradeTo, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsJapaneseUi()
    {
        try
        {
            var lang = LocManager.Instance?.Language ?? "";
            return lang.Contains("jpn", StringComparison.OrdinalIgnoreCase)
                   || lang.Contains("ja", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
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
