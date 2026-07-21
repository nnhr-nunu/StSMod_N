using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 調教命令カード（No.56-66）の共通基盤。コスト0・廃棄・アップグレード無し・報酬プールに出ない。
/// CSVビルドはすべて性癖：DomSub。本体効果のあと DomSub 性癖刺さり（破滅）を自動解決する。
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

    /// <summary>
    /// 調教コマンド等で手札生成するとき、生成バッチ内で左側に寄せる。
    /// 沼付与・DomSub性癖目覚めなど、先に使われやすいコマンド向け。
    /// </summary>
    public virtual bool PreferLeftWhenGenerated => false;

    /// <summary>生成カードを手札へ。PreferLeftWhenGenerated をバッチ内左寄せしてから追加する。</summary>
    public static async Task AddGeneratedToHandOrderedAsync(IEnumerable<CardModel> cards, Player owner)
    {
        foreach (var card in cards.OrderByDescending(c =>
                     c is TrainingCommand { PreferLeftWhenGenerated: true }))
        {
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner);
        }
    }

    /// <summary>
    /// 本体効果のあと DomSub 刺さりを解決する。
    /// 同一プレイで目覚めたばかりの DomSub は刺さらない（FetishCombat 側で除外）。
    /// </summary>
    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await OnCommandPlay(choiceContext, play);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    /// <summary>調教命令の固有効果。性癖刺さりは OnPlay 側で行う。</summary>
    protected abstract Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play);
}
