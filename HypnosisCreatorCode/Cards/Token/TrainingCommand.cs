using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 調教命令カード（No.56-66）の共通基盤。コスト0・廃棄・アップグレード無し・報酬プールに出ない。
/// CSVビルドはすべて性癖：DomSub。
/// </summary>
public abstract class TrainingCommand(TargetType target = TargetType.AnyEnemy, CardType type = CardType.Skill) :
    HypnosisCreatorCard(0, type, CardRarity.Token, target,
        showInCardLibrary: false)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <summary>生成カードのためスキルポーション等の戦闘中生成対象外。</summary>
    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;
}
