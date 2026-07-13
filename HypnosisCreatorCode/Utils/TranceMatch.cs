using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

public static class TranceMatch
{
    public static bool IsMatched(Creature player, Creature enemy)
    {
        var dom = player.GetPowerAmount<DominationPower>();
        var sub = enemy.GetPowerAmount<SubmissionPower>();
        return dom > 0 && dom == sub;
    }

    /// <summary>
    /// 性癖一致: MaxHP 割合ダメージ。不一致: 敵を回復（デメリット）。
    /// </summary>
    public static async Task ResolveMatchOutcome(
        PlayerChoiceContext choiceContext,
        Creature player,
        Creature enemy,
        CardModel sourceCard,
        decimal matchPercent,
        decimal mismatchHeal)
    {
        if (IsMatched(player, enemy))
        {
            var dmg = Math.Max(1, (int)Math.Floor(enemy.MaxHp * (double)matchPercent / 100.0));
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                dmg,
                ValueProp.Unblockable,
                player,
                sourceCard,
                null);
            MainFile.Logger.Info($"Trance match: Dom==Sub → {dmg} ({matchPercent}% MaxHP)");
        }
        else if (mismatchHeal > 0)
        {
            await CreatureCmd.Heal(enemy, mismatchHeal);
            MainFile.Logger.Info($"Trance mismatch: heal enemy {mismatchHeal}");
        }
    }
}
