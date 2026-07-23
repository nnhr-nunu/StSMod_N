using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>希少な心臓の戦闘中 UI（色付きアイコン・発動ホバー）。</summary>
public static class HeartRelicUi
{
    private const string LocTable = "relics";
    private const string ActivateTitleKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.title";
    private const string ActivateDescriptionKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.description";

    /// <summary>筋力／弱体のキーワードに近い金色。</summary>
    public static readonly Color ActivatableModulate = new(1f, 0.82f, 0.35f);

    public static bool ShouldShowActivationHint(EnemyHeartRelic heart, Player? player) =>
        CombatManager.Instance is { IsInProgress: true }
        && HeartRelicActivation.ShouldHighlight(heart, player);

    public static IHoverTip CreateActivationHoverTip() =>
        new HoverTip(
            new LocString(LocTable, ActivateTitleKey),
            new LocString(LocTable, ActivateDescriptionKey),
            icon: null);

    public static void ApplyHolderVisual(RelicModel? model, TextureRect? icon, Player? player)
    {
        if (icon == null || model is not EnemyHeartRelic heart) return;

        if (ShouldShowActivationHint(heart, player))
            icon.Modulate = ActivatableModulate;
    }
}
