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
/// CSV: 本来は敵の支援(バフ)意図をプレイヤーへ向けさせる効果を想定。
/// ターゲットリダイレクトAPIが未確定のため、暫定で混乱(Confused)+トランスで近似する。
/// UGでは弱体も追加付与し「意図を阻害する」効果を近似する。
/// TODO: リダイレクトAPIが確定したら本実装に差し替える。
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
        await PowerCmd.Apply<ConfusedPower>(
            choiceContext, play.Target, DynamicVars["ConfusedPower"].BaseValue, Owner.Creature, this);

        if (IsUpgraded)
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, 1M, Owner.Creature, this);

        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["ConfusedPower"].UpgradeValueBy(1M);
}
