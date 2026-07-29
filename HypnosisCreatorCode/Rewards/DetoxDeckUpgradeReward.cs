using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;

namespace HypnosisCreator.HypnosisCreatorCode.Rewards;

/// <summary>
/// デトックス用の追加報酬。禁断の魔導書（カード削除報酬）と同型で報酬画面に載せ、
/// 選択後にデッキUG画面へ遷移する（スキップ可）。
/// </summary>
public sealed class DetoxDeckUpgradeReward(Player player) : Reward(player)
{
    private const string LocTable = "gameplay_ui";
    private const string LocKey = "HYPNOSISCREATOR-DETOX_UPGRADE_REWARD";
    private const string RewardIconPath = "ui/rest_site/option_SMITH.png";

    protected override RewardType RewardType => RewardType.SpecialCard;

    public override int RewardsSetIndex => 7;

    protected override string IconPath => RewardIcon;

    public override LocString Description => new(LocTable, LocKey);

    public override bool IsPopulated => true;

    public override void Populate() { }

    public override void MarkContentAsSeen() { }

    private static string RewardIcon => ImageHelper.GetImagePath(RewardIconPath);

    protected override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, minCount: 0, maxCount: 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };

        var selected = await CardSelectCmd.FromDeckForUpgrade(Player, prefs);
        if (!selected.Any())
            return false;

        foreach (var card in selected)
            CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);

        return true;
    }
}
