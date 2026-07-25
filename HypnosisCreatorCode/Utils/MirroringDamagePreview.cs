using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ミラーリングの実効ダメージプレビュー。敵意図の1ヒット値に対し、
/// スロウ・脆弱など攻撃補正は <see cref="ValueProp.Move"/> で反映し、
/// プレイヤー筋力だけ <see cref="Patches.MirroringStrengthExclusionPatch"/> で除外する。
/// </summary>
public static class MirroringDamagePreview
{
    public static decimal Resolve(
        CardModel card,
        Creature? target,
        decimal rawIntentPerHit,
        CardPreviewMode previewMode = CardPreviewMode.Normal)
    {
        if (rawIntentPerHit <= 0) return 0;

        return CardDamagePreview.ApplyModifiers(
            card, target, rawIntentPerHit, ValueProp.Move, previewMode);
    }
}
