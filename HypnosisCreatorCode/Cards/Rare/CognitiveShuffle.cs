using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 認知シャッフル催眠 — カウント。トランス3。キャラクターカード3枚から1枚選び対応パワーを得る。
/// 対象がトランス中、ターン開始に同キャラプールのカードを生成（エセリアル・廃棄・このターン0コスト）。
/// プレイヤー見た目を一時差し替え。虚無化の強制ターン終了はバイパス。UGで生成3枚。性癖タグなし。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CognitiveShuffle() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 3M),
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
        try
        {
            var selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, options, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToList();
            chosenCard = selected.FirstOrDefault() ?? options[rng.NextInt(options.Count)];
        }
        catch
        {
            chosenCard = options[rng.NextInt(options.Count)];
        }

        var chosen = chosenCard as CognitiveCharacterChoice;
        var formTypeChosen = chosen?.FormCardType ?? pickedTypes[0];
        var linked = chosen?.LinkedCharacter
                     ?? CognitiveCharacterFaces.CharacterForFormType(formTypeChosen);

        await ApplyFormPower(choiceContext, formTypeChosen);

        var formCanonical = ModelDb.AllCards.First(c => c.GetType() == formTypeChosen);
        var shuffle = await PowerCmd.Apply<CognitiveShufflePower>(
            choiceContext, Owner.Creature, DynamicVars["Cards"].BaseValue, Owner.Creature, this);
        if (shuffle != null)
        {
            shuffle.FormCanonical = formCanonical;
            shuffle.TrackTranceTarget(play.Target);
            if (linked != null)
                shuffle.ApplyDisguise(linked);
        }
    }

    private async Task ApplyFormPower(PlayerChoiceContext choiceContext, Type formCardType)
    {
        var self = Owner.Creature;
        switch (formCardType.Name)
        {
            case nameof(DemonForm):
                // 本家 DemonForm CanonicalVars Amount=3
                await PowerCmd.Apply<DemonFormPower>(choiceContext, self, 3M, self, this);
                break;
            case nameof(SerpentForm):
                // 本家 SerpentForm Amount=4
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, self, 4M, self, this);
                break;
            case nameof(VoidForm):
                // 本家 VoidForm Amount=2
                await PowerCmd.Apply<VoidFormPower>(choiceContext, self, 2M, self, this);
                await PowerCmd.Apply<CognitiveVoidBypassPower>(choiceContext, self, 1M, self, this);
                break;
            case nameof(ReaperForm):
                await PowerCmd.Apply<ReaperFormPower>(choiceContext, self, 1M, self, this);
                break;
            case nameof(EchoForm):
                await PowerCmd.Apply<EchoFormPower>(choiceContext, self, 1M, self, this);
                break;
        }
    }

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1M);
}
