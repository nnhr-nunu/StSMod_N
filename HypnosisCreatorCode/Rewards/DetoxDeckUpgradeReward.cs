using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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

    /// <summary>
    /// セーブ上の識別子（<see cref="SerializableReward.OptionCount"/>）。種別は本家カード削除報酬と同じ RemoveCard にする。
    /// 以前の「SpecialCard でカード実体なし」は、共闘のセーブ読込でホストが報酬を送るとき本家
    /// <see cref="SerializableReward.Serialize"/> が null のカードを書こうとして例外になり、続きから遊べなかった。
    /// </summary>
    internal const int SaveMarker = -0x4843;

    protected override RewardType RewardType => RewardType.RemoveCard;

    public override int RewardsSetIndex => 7;

    protected override string IconPath => RewardIcon;

    public IEnumerable<string> AssetPaths => [RewardIcon, ..NCardSmithVfx.AssetPaths];

    public override LocString Description => new(LocTable, LocKey);

    public override bool IsPopulated => true;

    public override void Populate() { }

    public override void MarkContentAsSeen() { }

    public override SerializableReward ToSerializable()
    {
        var save = base.ToSerializable();
        save.OptionCount = SaveMarker;
        return save;
    }

    /// <summary>セーブ上の報酬がデトックス鍛冶報酬か（旧形式: SpecialCard でカード実体なし、も含む）。</summary>
    internal static bool IsSaved(SerializableReward save) =>
        (save.RewardType == RewardType.RemoveCard && save.OptionCount == SaveMarker)
        || (save.RewardType == RewardType.SpecialCard && save.SpecialCard == null);

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

        // 共闘では選択結果が全員の端末でこの OnSelect を通る。演出と待ちは本人の画面だけ（本家鍛冶の DoLocalPostSelectVfx と同じ）。
        if (LocalContext.IsMe(Player))
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
        if (vfx == null)
            return;

        GodotTreeExtensions.AddChildSafely(container, vfx);
        await Cmd.CustomScaledWait(1f, 2f);
    }
}
