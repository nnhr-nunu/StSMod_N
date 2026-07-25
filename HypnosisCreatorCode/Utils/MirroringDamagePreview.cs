using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ミラーリングの実効ダメージプレビュー。敵意図の1ヒット値に攻撃補正（筋力・スロウ・弱体等）を載せる。
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
