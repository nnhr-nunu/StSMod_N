using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Rewards;

/// <summary>
/// デトックス用の追加報酬。禁断の魔導書（カード削除報酬）と同型で報酬画面に載せ、
/// 選択後にデッキUG画面へ遷移する（スキップ可）。
/// </summary>
public sealed class DetoxDeckUpgradeReward(Player player) : Reward(player)
{
    private const string LocTable = "gameplay_ui";
    private const string LocKey = "HYPNOSISCREATOR-DETOX_UPGRADE_REWARD";
    /// <summary>休憩所鍛冶と同じアイコン（<c>option_smith</c> は小文字）。</summary>
    private const string RewardIconPath = "ui/rest_site/option_smith.png";

    protected override RewardType RewardType => RewardType.SpecialCard;

    public override int RewardsSetIndex => 7;

    protected override string IconPath => RewardIcon;

    public IEnumerable<string> AssetPaths => [RewardIcon, ..NCardSmithVfx.AssetPaths];

    public override LocString Description => new(LocTable, LocKey);

    public override bool IsPopulated => true;

    public override void Populate() { }

    public override void MarkContentAsSeen() { }

    /// <summary>カード実体なし（<see cref="SerializableReward.SpecialCard"/> は null）がセーブ上の識別子。</summary>
    public override SerializableReward ToSerializable()
    {
        var save = base.ToSerializable();
        save.SpecialCard = null;
        return save;
    }

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

        var upgraded = selected.ToArray();
        foreach (var card in upgraded)
            CardCmd.Upgrade(card, CardPreviewStyle.None);

        await PlaySmithUpgradePresentation(upgraded);
        return true;
    }

    /// <summary>休憩所鍛冶と同じ <see cref="NCardSmithVfx"/>（かんかん SE 付き）。</summary>
    private static async Task PlaySmithUpgradePresentation(IEnumerable<CardModel> cards)
    {
        var container = NRun.Instance?.GlobalUi?.CardPreviewContainer;
        if (container == null)
            return;

        var vfx = NCardSmithVfx.Create(cards, playSfx: true);
        GodotTreeExtensions.AddChildSafely(container, vfx);
        await Cmd.CustomScaledWait(1f, 2f);
    }
}
