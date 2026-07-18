using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 調教命令カード（No.56-66）の共通基盤。コスト0・廃棄・アップグレード無し・報酬プールに出ない。
/// CSV: 個別の数値・効果は未確定分あり。命令ごとの意図に沿った暫定効果を実装。
/// </summary>
public abstract class TrainingCommand(TargetType target = TargetType.AnyEnemy, CardType type = CardType.Skill) :
    HypnosisCreatorCard(0, type, CardRarity.Token, target,
        showInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}
