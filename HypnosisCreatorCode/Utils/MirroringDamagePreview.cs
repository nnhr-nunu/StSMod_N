using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ミラーリングのダメージプレビュー。
/// 既定は敵意図の1ヒット値をそのまま表示（状態異常・筋力等の追加補正なし）。
/// </summary>
public static class MirroringDamagePreview
{
    /// <summary>
    /// false にすると旧挙動（<see cref="ValueProp.Move"/> で筋力・スロウ・弱体等を反映）に戻せる。
    /// </summary>
    public const bool UseIntentValuesOnly = true;

    public static decimal Resolve(
        CardModel card,
        Creature? target,
        decimal rawIntentPerHit,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (rawIntentPerHit <= 0) return 0;

        return UseIntentValuesOnly
            ? ResolveIntentOnly(rawIntentPerHit)
            : ResolveWithAttackModifiers(card, target, rawIntentPerHit, previewMode);
    }

    /// <summary>敵の攻撃意図表示と同じ1ヒット値（補正なし）。</summary>
    public static decimal ResolveIntentOnly(decimal rawIntentPerHit) =>
        Math.Max(0, rawIntentPerHit);

    /// <summary>旧実装: プレイヤー攻撃として Move 補正を載せる。</summary>
    public static decimal ResolveWithAttackModifiers(
        CardModel card,
        Creature? target,
        decimal rawIntentPerHit,
        CardPreviewMode previewMode = CardPreviewMode.Normal) =>
        CardDamagePreview.ApplyModifiers(
            card, target, rawIntentPerHit, ValueProp.Move, previewMode);
}
