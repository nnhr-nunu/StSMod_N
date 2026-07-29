using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// デトックス — 戦闘終了時、デッキからカードを1枚アップグレードしてもよい（スキップ可）。
/// </summary>
public class DetoxPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // No.108 カード原画をパワーアイコンに流用
    public override string CustomPackedIconPath => "detox.png".CardImagePath();
    public override string CustomBigIconPath => "detox.png".BigCardImagePath();

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        var player = Owner?.Player;
        if (player == null) return;

        try
        {
            var selected = await CardSelectCmd.FromDeckForUpgrade(
                player,
                new CardSelectorPrefs(
                    CardSelectorPrefs.UpgradeSelectionPrompt,
                    minCount: 0,
                    maxCount: 1));

            foreach (var card in selected)
                CardCmd.Upgrade(card, CardPreviewStyle.None);
        }
        catch
        {
            // 対象なし／キャンセル
        }

        await PowerCmd.Remove(this);
    }
}
