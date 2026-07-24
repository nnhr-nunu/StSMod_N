using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 認知シャッフル催眠 — カウント。トランス5。3色から1枚選び対応パワーを得る。
/// 対象がトランス中（残り2以上のターン）、ターン開始（ドロー前）に同系カードを生成（エセリアル・廃棄・このターン0コスト）。トランス5なら生成4回。UGで生成3枚。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CognitiveShuffle() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 5M),
        new DynamicVar("Cards", 2M)
    ];

    private static readonly Type[] FormCardTypes =
    [
        typeof(DemonForm),
        typeof(SerpentForm),
        typeof(VoidForm),
        typeof(ReaperForm),
        typeof(EchoForm)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (CombatState == null) return;

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);

        // 集団催眠の波及: トランスのみ。選択・形態パワーは手動1回だけ。
        if (MassHypnosisPower.IsPropagating)
        {
            Owner.Creature?.GetPower<CognitiveShufflePower>()?.TrackTranceTarget(play.Target);
            return;
        }

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var pickedTypes = FormCardTypes.OrderBy(_ => rng.NextInt(int.MaxValue)).Take(3).ToList();
        var options = new List<CardModel>();
        foreach (var formType in pickedTypes)
        {
            var choice = (CognitiveCharacterChoice)CombatState.CreateCard(
                ModelDb.Card<CognitiveCharacterChoice>(), Owner);
            choice.FormCardType = formType;
            choice.LinkedCharacter = CognitiveCharacterFaces.CharacterForFormType(formType);
            options.Add(choice);
        }

        CardModel chosenCard;
        CognitiveShuffleCompletion.BeginGate(Owner);
        try
        {
            chosenCard = await CardSelectCmd.FromChooseACardScreen(
                choiceContext, options, Owner, canSkip: false)
                ?? options[rng.NextInt(options.Count)];
        }
        catch
        {
            chosenCard = options[rng.NextInt(options.Count)];
        }

        var chosen = chosenCard as CognitiveCharacterChoice;
        var formTypeChosen = chosen?.FormCardType ?? pickedTypes[0];
        var linked = chosen?.LinkedCharacter
                     ?? CognitiveCharacterFaces.CharacterForFormType(formTypeChosen);

        var formCanonical = ModelDb.AllCards.First(c => c.GetType() == formTypeChosen);
        CognitiveShufflePendingStore.Set(
            Owner, formTypeChosen, linked, play.Target,
            DynamicVars["Cards"].BaseValue, formCanonical);
    }

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1M);
}
