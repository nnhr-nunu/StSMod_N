using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>希少な心臓の UI（金色ハイライト・発動ホバー）。</summary>
public static class HeartRelicUi
{
    private const string LocTable = "relics";
    private const string ActivateTitleKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.title";
    private const string ActivateDescriptionKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.description";

    /// <summary>筋力／弱体のキーワードに近い金色。</summary>
    public static readonly Color ActivatableModulate = new(1f, 0.82f, 0.35f);

    /// <summary>右クリック説明ホバー（発動可否とは無関係に常に表示）。</summary>
    public static bool ShouldShowActivationHover(EnemyHeartRelic heart, Player? player) =>
        heart.IsRareHeart;

    /// <summary>戦闘中に今すぐ右クリック発動できるときだけ金色にする。</summary>
    public static bool ShouldHighlightForActivation(EnemyHeartRelic heart, Player? player) =>
        HeartRelicActivation.ShouldHighlight(heart, player);

    public static IHoverTip CreateActivationHoverTip() =>
        new HoverTip(
            new LocString(LocTable, ActivateTitleKey),
            new LocString(LocTable, ActivateDescriptionKey),
            icon: null);

    public static void ApplyHolderVisual(RelicModel? model, TextureRect? icon, Player? player)
    {
        if (icon == null || model is not EnemyHeartRelic heart) return;

        if (ShouldHighlightForActivation(heart, player))
            icon.Modulate = ActivatableModulate;
    }
}
