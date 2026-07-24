using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル — キャラ選択後のパワー付与・見た目差し替え。
/// OnPlay 内の await PowerCmd は CustomScaledWait でカード宙吊りになるため、
/// <see cref="MegaCrit.Sts2.Core.Hooks.Hook.AfterCardPlayed"/> の Task 完了後に直列実行する。
/// </summary>
public static class CognitiveShuffleCompletion
{
    /// <summary>AfterCardPlayed の本体完了後に呼ぶ（hook Task は呼び出し元で await 済み）。</summary>
    public static async Task RunIfPendingAsync(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = cardPlay.Card.Owner;
        if (owner == null) return;
        if (!CognitiveShufflePendingStore.HasPending(owner)) return;

        try
        {
            if (!CognitiveShufflePendingStore.TryTake(owner, out var pending))
                return;

            await CompleteAsync(choiceContext, cardPlay.Card, pending);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Cognitive shuffle completion failed: {e.Message}");
        }
    }

    private static async Task CompleteAsync(
        PlayerChoiceContext choiceContext,
        CardModel cardSource,
        CognitiveShufflePendingStore.Pending pending)
    {
        var self = cardSource.Owner?.Creature;
        if (self == null) return;

        CognitiveShuffleApplyContext.SetIconCharacter(self.Player, pending.Disguise);

        var shufflePower = await PowerCmd.Apply<CognitiveShufflePower>(
            choiceContext, self, pending.CardsAmount, self, cardSource, silent: true);
        if (shufflePower != null)
        {
            shufflePower.FormCanonical = pending.FormCanonical;
            if (pending.Disguise != null)
                shufflePower.SetDisguiseCharacter(pending.Disguise);
            if (pending.TranceTarget != null)
                shufflePower.TrackTranceTarget(pending.TranceTarget);
            CognitiveShufflePowerUi.RefreshRowIcon(shufflePower);
        }

        await ApplyFormPowerAsync(choiceContext, cardSource, self, pending.FormType, shufflePower);

        shufflePower?.ApplyDisguise();
    }

    private static async Task ApplyFormPowerAsync(
        PlayerChoiceContext choiceContext,
        CardModel cardSource,
        Creature self,
        Type formCardType,
        CognitiveShufflePower? shufflePower)
    {
        switch (formCardType.Name)
        {
            case nameof(DemonForm):
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<DemonFormPower>(
                    choiceContext, self, 3M, self, cardSource, silent: true));
                break;
            case nameof(SerpentForm):
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<SerpentFormPower>(
                    choiceContext, self, 4M, self, cardSource, silent: true));
                break;
            case nameof(VoidForm):
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<CognitiveVoidBypassPower>(
                    choiceContext, self, 1M, self, cardSource, silent: true));
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<VoidFormPower>(
                    choiceContext, self, 2M, self, cardSource, silent: true));
                break;
            case nameof(ReaperForm):
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<ReaperFormPower>(
                    choiceContext, self, 1M, self, cardSource, silent: true));
                break;
            case nameof(EchoForm):
                shufflePower?.RegisterGrantedForm(await PowerCmd.Apply<EchoFormPower>(
                    choiceContext, self, 1M, self, cardSource, silent: true));
                break;
        }
    }
}
