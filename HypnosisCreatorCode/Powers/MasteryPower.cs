using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 練達 — このパワーが有効な間にプレイしたカウントカードを、戦闘終了時に永続的にアップグレードする。
/// カード実体はデッキと共有されるため、<see cref="MegaCrit.Sts2.Core.Commands.CardCmd.Upgrade"/> がそのまま永続化に使える。
/// 実際のアップグレード処理は <see cref="Patches.MasteryUpgradePatch"/> が戦闘終了フックから呼び出す。
/// </summary>
public class MasteryPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private readonly HashSet<CardModel> _playedCountCards = [];
    public IReadOnlyCollection<CardModel> PlayedCountCards => _playedCountCards;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner) return Task.CompletedTask;
        // 波及コピーは一時カード。デッキ実体の追跡に混ぜない。
        if (MassHypnosisPower.IsPropagating) return Task.CompletedTask;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return Task.CompletedTask;
        _playedCountCards.Add(cardPlay.Card);
        return Task.CompletedTask;
    }
}
