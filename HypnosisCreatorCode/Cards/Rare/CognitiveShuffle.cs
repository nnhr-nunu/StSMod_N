using BaseLib.Utils;
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
/// 認知シャッフル催眠 — カウント3。トランス2。形態カード3枚から1枚選び対応パワーを得る。
/// 対象がトランス中、ターン開始に同キャラプールのカードを生成（エセリアル・廃棄・このターン0コスト）。
/// 虚無化の強制ターン終了はバイパス。UGで生成3枚。性癖タグなし。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CognitiveShuffle() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 2M),
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

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var pickedTypes = FormCardTypes.OrderBy(_ => rng.NextInt(int.MaxValue)).Take(3).ToList();
        var options = pickedTypes
            .Select(t => CombatState.CreateCard(ModelDb.AllCards.First(c => c.GetType() == t), Owner))
            .ToList();

        CardModel chosen;
        try
        {
            var selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, options, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToList();
            chosen = selected.FirstOrDefault() ?? options[rng.NextInt(options.Count)];
        }
        catch
        {
            chosen = options[rng.NextInt(options.Count)];
        }

        await ApplyFormPower(choiceContext, chosen);

        var shuffle = await PowerCmd.Apply<CognitiveShufflePower>(
            choiceContext, Owner.Creature, DynamicVars["Cards"].BaseValue, Owner.Creature, this);
        if (shuffle != null)
        {
            shuffle.FormCanonical = chosen.CanonicalInstance ?? ModelDb.AllCards.First(c => c.GetType() == chosen.GetType());
            shuffle.TranceTarget = play.Target;
        }
    }

    private async Task ApplyFormPower(PlayerChoiceContext choiceContext, CardModel formCard)
    {
        var self = Owner.Creature;
        switch (formCard)
        {
            case DemonForm:
                await PowerCmd.Apply<DemonFormPower>(choiceContext, self, 2M, self, this);
                break;
            case SerpentForm:
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, self, 1M, self, this);
                break;
            case VoidForm:
                await PowerCmd.Apply<VoidFormPower>(choiceContext, self, 1M, self, this);
                await PowerCmd.Apply<CognitiveVoidBypassPower>(choiceContext, self, 1M, self, this);
                break;
            case ReaperForm:
                await PowerCmd.Apply<ReaperFormPower>(choiceContext, self, 1M, self, this);
                break;
            case EchoForm:
                await PowerCmd.Apply<EchoFormPower>(choiceContext, self, 1M, self, this);
                break;
            default:
                // 型不一致時はカノニカル名で再判定
                await ApplyFormPowerByTypeName(choiceContext, formCard.GetType().Name);
                break;
        }
    }

    private async Task ApplyFormPowerByTypeName(PlayerChoiceContext choiceContext, string typeName)
    {
        var self = Owner.Creature;
        switch (typeName)
        {
            case nameof(DemonForm):
                await PowerCmd.Apply<DemonFormPower>(choiceContext, self, 2M, self, this);
                break;
            case nameof(SerpentForm):
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, self, 1M, self, this);
                break;
            case nameof(VoidForm):
                await PowerCmd.Apply<VoidFormPower>(choiceContext, self, 1M, self, this);
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
