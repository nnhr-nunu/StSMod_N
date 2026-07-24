using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル — キャラ選択後のパワー付与・見た目差し替え。
/// OnPlay 内の await PowerCmd は CustomScaledWait でカード宙吊りになるため、
/// <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterCardPlayed"/> 後に非同期で実行する。
/// </summary>
public static class CognitiveShuffleCompletion
{
    public static void TrySchedule(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Card is not Cards.Rare.CognitiveShuffle shuffle) return;
        if (!shuffle.HasPendingCompletion) return;
        shuffle.HasPendingCompletion = false;
        _ = CompleteAsync(choiceContext, shuffle);
    }

    private static async Task CompleteAsync(
        PlayerChoiceContext choiceContext,
        Cards.Rare.CognitiveShuffle shuffle)
    {
        var self = shuffle.Owner?.Creature;
        if (self == null) return;

        var formType = shuffle.PendingFormType;
        var disguise = shuffle.PendingDisguise;
        var tranceTarget = shuffle.PendingTranceTarget;
        var cardsAmount = shuffle.PendingCardsAmount;
        var formCanonical = shuffle.PendingFormCanonical;

        shuffle.PendingFormType = null;
        shuffle.PendingDisguise = null;
        shuffle.PendingTranceTarget = null;
        shuffle.PendingFormCanonical = null;

        try
        {
            var shufflePower = await PowerCmd.Apply<CognitiveShufflePower>(
                choiceContext, self, cardsAmount, self, shuffle, silent: true);
            if (shufflePower != null)
            {
                shufflePower.FormCanonical = formCanonical;
                if (tranceTarget != null)
                    shufflePower.TrackTranceTarget(tranceTarget);
            }

            if (formType != null)
                await ApplyFormPowerAsync(choiceContext, shuffle, self, formType);

            if (disguise != null)
                shufflePower?.ApplyDisguise(disguise);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Cognitive shuffle completion failed: {e.Message}");
        }
    }

    private static async Task ApplyFormPowerAsync(
        PlayerChoiceContext choiceContext,
        CardModel cardSource,
        Creature self,
        Type formCardType)
    {
        switch (formCardType.Name)
        {
            case nameof(DemonForm):
                await PowerCmd.Apply<DemonFormPower>(choiceContext, self, 3M, self, cardSource, silent: true);
                break;
            case nameof(SerpentForm):
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, self, 4M, self, cardSource, silent: true);
                break;
            case nameof(VoidForm):
                await PowerCmd.Apply<CognitiveVoidBypassPower>(
                    choiceContext, self, 1M, self, cardSource, silent: true);
                await PowerCmd.Apply<VoidFormPower>(choiceContext, self, 2M, self, cardSource, silent: true);
                break;
            case nameof(ReaperForm):
                await PowerCmd.Apply<ReaperFormPower>(choiceContext, self, 1M, self, cardSource, silent: true);
                break;
            case nameof(EchoForm):
                await PowerCmd.Apply<EchoFormPower>(choiceContext, self, 1M, self, cardSource, silent: true);
                break;
        }
    }
}
