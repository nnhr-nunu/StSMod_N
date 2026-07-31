using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// UGの定性差分を説明文へ緑表示する。全文差し替えはしない（キーワード色・性癖行を消さない）。
/// 文言の正本は <c>localization/*/cards.json</c> の upgrade* キー。
/// カードライブラリ表示でも適用する（他の mod 改変は <see cref="CardLibraryUiGuard"/> で継続抑止）。
/// </summary>
public static class UpgradeDescriptionHooks
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        _ = target;
        UpgradeCardText.ApplyLocalizedUpgrade(card, ref description);
    }
}
