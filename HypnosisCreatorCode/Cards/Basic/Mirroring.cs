using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// ミラーリング — 相手の攻撃予定と同じ攻撃。廃棄。
/// 数値は相手と同じ。こちら側の筋力は反映しない。弱体は敵デバフなので有効（CSV備考）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mirroring() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // Unpowered: プレイヤー筋力を載せない
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, ValueProp.Unpowered)];

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    public static bool HasAttackIntent(Creature target) =>
        EnemyAttackIntents.IntendsToAttack(target);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!EnemyAttackIntents.TryGetPerHit(play.Target, out var damage, out var hits))
            return;

        DynamicVars.Damage.BaseValue = damage;
        // 複数ヒットは Attack を都度差し替えず、指パッチンを攻撃中ループ
        if (hits > 1)
            CombatFrameAnimator.BeginAttackLoop(Owner.Creature);
        try
        {
            for (var i = 0; i < hits; i++)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this, play)
                    .Targeting(play.Target)
                    .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
                    .WithNoAttackerAnim()
                    .Execute(choiceContext);
            }
        }
        finally
        {
            if (hits > 1)
                CombatFrameAnimator.EndAttackLoop(Owner.Creature);
        }
    }
}

