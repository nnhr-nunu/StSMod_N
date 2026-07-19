using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 連続トランス — トランス1→沼1→トランス性癖目覚め。リプレイ2。
/// 未所持なら1回目は目覚めのみ（刺さらず）、2〜3回目で刺さる。所持済みなら最大3回刺さる。
/// UGで相手すべてに同効果。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ContinuousTrance() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 1M),
        new PowerVar<BogPower>(1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 説明文には「リプレイ」を書かない（BaseReplayCount の自動追記と二重になるため）。
        // シリーズ中に再セットするとリプレイが伸びるので、先頭プレイだけ付与する。
        if (play.IsFirstInSeries)
            BaseReplayCount = 2;

        if (IsUpgraded && CombatState != null)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
                await ApplyToEnemy(choiceContext, enemy);
            return;
        }

        ArgumentNullException.ThrowIfNull(play.Target);
        await ApplyToEnemy(choiceContext, play.Target);
    }

    /// <summary>
    /// トランス→沼→目覚め。トランス付与時に既にトランス性癖があれば刺さる。
    /// 沼はトランスのあとに付与（トランスで沼が深まる演出）。
    /// </summary>
    private async Task ApplyToEnemy(PlayerChoiceContext choiceContext, Creature enemy)
    {
        await TranceCombat.ApplyTrance(
            choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<BogPower>(
            choiceContext, enemy, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
        FetishCombat.Awaken(enemy, FetishType.Trance, Owner);
    }

    protected override void OnUpgrade() { }
}
