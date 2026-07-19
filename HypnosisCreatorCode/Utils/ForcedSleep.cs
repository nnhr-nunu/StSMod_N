using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// バニラ <see cref="AsleepPower"/> は睡眠アイコン／ZZZ を出さない（ラガヴーリン等の専用ムーブ任せ）。
/// 睡眠前の行動予定を保存したうえで Sleep 表示にし、起床時に復元する。
/// </summary>
public static class ForcedSleep
{
    public static async Task EnsurePresentation(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature applier,
        CardModel? cardSource = null)
    {
        if (target is not { IsAlive: true, IsEnemy: true, Monster: not null }) return;
        if (!target.HasPower<AsleepPower>()) return;

        if (target.GetPower<ForcedSleepVisualPower>() == null)
        {
            await PowerCmd.Apply<ForcedSleepVisualPower>(
                choiceContext, target, 1M, applier, cardSource, silent: true);
        }

        target.GetPower<ForcedSleepVisualPower>()?.RefreshPresentation();
    }
}
