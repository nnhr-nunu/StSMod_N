using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 罪悪感 — 5回の戦闘後にデッキから削除。廃棄。アブノーマル。
/// 本家 Guilty と同じ CombatsSeen カウンタ。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class GuiltyCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    private const int MaxCombats = 5;
    private int _combatsSeen;

    public override string PortraitPath => "guilty.png".CardImagePath();
    public override string CustomPortraitPath => "guilty.png".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Combats", MaxCombats)];

    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        set
        {
            AssertMutable();
            _combatsSeen = value;
            DynamicVars["Combats"].BaseValue = MaxCombats - CombatsSeen;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ResolveFetishOnTarget(choiceContext, play);
    }

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        if (Pile?.Type != PileType.Deck) return;

        CombatsSeen++;
        if (CombatsSeen >= MaxCombats && Pile.Type == PileType.Deck)
            await CardPileCmd.RemoveFromDeck(this);
    }
}
