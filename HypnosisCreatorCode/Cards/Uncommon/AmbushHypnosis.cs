using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>不意打ち催眠 — 敵の数だけカードを引き、それぞれをランダムな敵へ自動でプレイする（コスト無視）。UGでコスト1。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AmbushHypnosis() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;
        var enemyCount = CombatState.HittableEnemies.Count;
        if (enemyCount <= 0) return;

        var player = Owner;
        var drawn = await CardPileCmd.Draw(choiceContext, enemyCount, player);
        var rng = player.RunState.Rng.CombatCardSelection;

        foreach (var card in drawn.ToList())
        {
            var enemies = CombatState.HittableEnemies.ToList();
            if (enemies.Count == 0) break;
            var target = enemies[rng.NextInt(enemies.Count)];
            await CardCmd.AutoPlay(choiceContext, card, target);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
