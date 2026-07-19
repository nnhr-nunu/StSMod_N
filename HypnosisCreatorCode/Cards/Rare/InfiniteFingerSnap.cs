using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>連続指パッチン — すべての敵に1ダメージ×5。リプレイX。廃棄。UGで保留。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteFingerSnap() : HypnosisCreatorCard(-1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        // GeneratePlayCount は virtual ではないため BaseReplayCount では間に合わない。
        // リプレイX = 追加X回 → 合計 (X+1) 回、攻撃本体を繰り返す。
        var times = Math.Max(0, ResolveEnergyXValue()) + 1;

        // 攻撃者アニメ待ちはスキップし、連番の指パッチンループだけを回す
        CombatFrameAnimator.BeginAttackLoop(Owner.Creature);
        try
        {
            for (var i = 0; i < times; i++)
            {
                if (CombatState == null) break;
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(5)
                    .FromCard(this, play)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                    .WithNoAttackerAnim()
                    .Execute(choiceContext);
            }
        }
        finally
        {
            CombatFrameAnimator.EndAttackLoop(Owner.Creature);
        }
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
