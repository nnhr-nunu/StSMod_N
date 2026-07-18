using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>ミラーリング — 相手の攻撃予定と同じ攻撃（筋力等は反映しない）。アタック意図時のみ。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mirroring() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, ValueProp.Move)];

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    public static bool HasAttackIntent(Creature target) =>
        target.Monster?.IntendsToAttack == true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (!TryGetAttackDamage(play.Target, out var damage, out var hits))
            return;

        DynamicVars.Damage.BaseValue = damage;
        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
                .Execute(choiceContext);
        }
    }

    private static bool TryGetAttackDamage(Creature enemy, out int perHit, out int hits)
    {
        perHit = 0;
        hits = 0;
        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return false;

        var move = monster.NextMove;
        if (move?.Intents == null) return false;

        foreach (var intent in move.Intents)
        {
            if (intent is not AttackIntent attack) continue;
            perHit = (int)attack.DamageCalc();
            hits = Math.Max(1, attack.Repeats);
            return true;
        }

        return false;
    }
}
