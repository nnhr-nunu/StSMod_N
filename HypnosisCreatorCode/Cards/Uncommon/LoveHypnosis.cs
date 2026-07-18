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
/// 好き好き催眠 — カウント。BuffIntent リダイレクトAPI未実装のため Confused で近似（task.md 参照）。
/// トランス1。UGはブロック意図も含む条件拡張だが、Weak 等の追加デバフは付けない。
/// TODO(task.md): BuffIntent / ブロック意図の着弾先変更API公開後に差し替え。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class LoveHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
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
        await PowerCmd.Apply<ConfusedPower>(
            choiceContext, play.Target, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }
}
