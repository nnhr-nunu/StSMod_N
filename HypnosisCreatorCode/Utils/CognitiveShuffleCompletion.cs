using HypnosisCreator.HypnosisCreatorCode.Powers;
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
    public static void TrySchedule(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = cardPlay.Card.Owner;
        if (owner == null) return;
        if (!CognitiveShufflePendingStore.TryTake(owner, out var pending)) return;
        _ = CompleteAsync(choiceContext, cardPlay.Card, pending);
    }

    private static async Task CompleteAsync(
        PlayerChoiceContext choiceContext,
        CardModel cardSource,
        CognitiveShufflePendingStore.Pending pending)
    {
        var self = cardSource.Owner?.Creature;
        if (self == null) return;

        try
        {
            var shufflePower = await PowerCmd.Apply<CognitiveShufflePower>(
                choiceContext, self, pending.CardsAmount, self, cardSource, silent: true);
            if (shufflePower != null)
            {
                shufflePower.FormCanonical = pending.FormCanonical;
                if (pending.TranceTarget != null)
                    shufflePower.TrackTranceTarget(pending.TranceTarget);
            }

            await ApplyFormPowerAsync(choiceContext, cardSource, self, pending.FormType);

            if (pending.Disguise != null)
                shufflePower?.ApplyDisguise(pending.Disguise);
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
