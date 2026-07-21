using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ソウルネクサスの心臓 — 希少。ランダム敵に18ダメージ＋弱体2＋脱力2。</summary>
public class SoulNexusHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SOUL_NEXUS";

    protected override decimal PreviewDamage => 18;

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var enemy = HeartActivationHelpers.PickRandomEnemy(player);
        if (enemy == null) return;

        Flash();
        await CreatureCmd.Damage(
            choiceContext, enemy, DynamicVars.Damage.BaseValue,
            ValueProp.Move, player.Creature, null, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 2, player.Creature, null!);
        await PowerCmd.Apply<FrailPower>(choiceContext, enemy, 2, player.Creature, null!);
        MarkUsed();
    }
}
