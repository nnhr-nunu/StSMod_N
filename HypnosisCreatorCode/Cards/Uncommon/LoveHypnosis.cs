using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 好き好き催眠 — カウント・DomSub。
/// CSV: 敵のバフ意図をプレイヤーへ向ける想定だが、BuffIntent の着弾リダイレクトAPIが無いため
/// Confused で近似し、トランスを付与する。UGで弱体も追加。
/// TODO: BuffIntent / 支援対象の書き換えAPIが公開されたら本実装へ差し替え。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class LoveHypnosis() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ConfusedPower>(2M),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        // Buff リダイレクトAPI未検出のため Confused で意図を阻害する近似を継続
        await PowerCmd.Apply<ConfusedPower>(
            choiceContext, play.Target, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);

        if (IsUpgraded)
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, 1M, Owner.Creature, this);

        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["ConfusedPower"].UpgradeValueBy(1M);
}
