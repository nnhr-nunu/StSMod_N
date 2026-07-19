using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 調教命令カード（No.56-66）の共通基盤。コスト0・廃棄・アップグレード無し・報酬プールに出ない。
/// CSVビルドはすべて性癖：DomSub。
/// サムネは調教コマンド（No.48 / training_command_card）と共通。
/// </summary>
public abstract class TrainingCommand(TargetType target = TargetType.AnyEnemy, CardType type = CardType.Skill) :
    HypnosisCreatorCard(0, type, CardRarity.Token, target,
        showInCardLibrary: false)
{
    /// <summary>No.48 調教コマンドと同一アート（CSV original/48）。</summary>
    private const string SharedPortraitFile = "training_command_card.png";

    public override string CustomPortraitPath => SharedPortraitFile.BigCardImagePath();
    public override string PortraitPath => SharedPortraitFile.CardImagePath();
    public override string BetaPortraitPath => $"beta/{SharedPortraitFile}".CardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <summary>生成カードのためスキルポーション等の戦闘中生成対象外。</summary>
    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;
}
