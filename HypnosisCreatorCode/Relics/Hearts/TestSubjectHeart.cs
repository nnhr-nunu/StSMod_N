using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 実験体の心臓 — 希少。激怒2・苦痛の一刺し1・霊体1。
/// </summary>
public class TestSubjectHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TEST_SUBJECT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<TestSubjectEnragePower>(
            choiceContext, player.Creature, 2, player.Creature, null!);
        await PowerCmd.Apply<TestSubjectPainfulStabsPower>(
            choiceContext, player.Creature, 1, player.Creature, null!);
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext, player.Creature, 1, player.Creature, null!);
        MarkUsed();
    }
}
