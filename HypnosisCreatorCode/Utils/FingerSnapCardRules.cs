using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>指パッチン SE を鳴らすカード／プレイヤーの判定。</summary>
internal static class FingerSnapCardRules
{
    public static bool IsHypnosisCreatorPlayer(Player? player) =>
        player?.Character?.Id.Entry.Contains(
            HcCharacter.CharacterId, StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>攻撃以外の指パッチン／催眠系カード。攻撃はヒット時 SE に任せる。</summary>
    public static bool WantsPlayFingerSnap(CardModel card) =>
        card.Type != CardType.Attack && (IsFingerThemed(card) || IsHypnosisThemed(card));

    private static bool IsFingerThemed(CardModel card) =>
        card is FingerSnap or Prefinger or FingerCount;

    private static bool IsHypnosisThemed(CardModel card) =>
        card.GetType().Name.Contains("Hypnosis", StringComparison.Ordinal);
}
