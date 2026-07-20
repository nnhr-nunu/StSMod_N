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
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 心停止催眠 — カウント。指定ターン後に対象を即死させる（ボス部屋は2倍のターン数。エリートは通常）。トランス1。
/// 既に心停止がある相手へ重ねがけすると、残りターンが1早くなる。
/// UG: 心臓停止時に追加のレリック報酬を獲得（説明は UpgradeDescriptionHooks）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CardiacArrestHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
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

        var existing = play.Target.GetPower<CardiacArrestPower>();
        if (existing != null)
        {
            await CardiacArrestPower.AdvanceCountdown(
                choiceContext, play.Target, Owner.Creature, this);
        }
        else
        {
            var turns = DynamicVars["Turns"].IntValue * (IsBossEncounter(play.Target) ? 2 : 1);

            await PowerCmd.Apply<CardiacArrestPower>(
                choiceContext, play.Target, turns, Owner.Creature, this);
            var power = play.Target.GetPower<CardiacArrestPower>();
            if (power != null)
            {
                power.GrantBonusRelic = IsUpgraded;
                power.BonusRelicPlayer = Owner;
            }
        }

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    /// <summary>
    /// ボス部屋のみ倍速対象。エリート（RoomType.Elite）は通常ターン。
    /// </summary>
    private static bool IsBossEncounter(Creature target) =>
        target.CombatState?.Encounter?.RoomType == RoomType.Boss;
}
