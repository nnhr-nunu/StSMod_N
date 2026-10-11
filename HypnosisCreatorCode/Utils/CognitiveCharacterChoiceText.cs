using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル選択カードの説明を、キャラ色の一文＋改行＋Form パワー効果に差し替える。
/// </summary>
public static class CognitiveCharacterChoiceText
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        _ = target;
        if (CardLibraryUiGuard.IsActive) return;
        if (card is not CognitiveCharacterChoice choice) return;

        var text = DescribeForm(choice.FormCardType);
        if (text != null) description = text;
    }

    private static string? DescribeForm(Type? formCardType)
    {
        if (formCardType == null) return null;

        // 文面は gameplay_ui の HYPNOSISCREATOR-COGNITIVE_{Key}_FLAVOR / _EFFECT。以下は .pck 未更新時のフォールバック
        (string Key, string FlavorJp, string EffectJp, string FlavorEn, string EffectEn)? form = formCardType.Name switch
        {
            nameof(DemonForm) => ("DEMON_FORM",
                "[red]血のような赤色。[/red]", "ターン開始時、[gold]筋力[/gold][blue]3[/blue]を得る。",
                "[red]A blood-like red.[/red]", "At the start of your turn, gain [blue]3[/blue] [gold]Strength[/gold]."),
            nameof(SerpentForm) => ("SERPENT_FORM",
                "[green]毒々しい緑色。[/green]", "カードをプレイするたび、ランダムな敵に[blue]4[/blue]ダメージを与える。",
                "[green]A venomous green.[/green]", "Whenever you play a card, deal [blue]4[/blue] damage to a random enemy."),
            nameof(VoidForm) => ("VOID_FORM",
                "[orange]高貴な橙色。[/orange]", "毎ターン、最初にプレイする[blue]2[/blue]枚のカードはコストを支払わずにプレイできる。",
                "[orange]A noble orange.[/orange]", "The first [blue]2[/blue] cards you play each turn are free to play."),
            nameof(ReaperForm) => ("REAPER_FORM",
                "[purple]闇をまとう紫色。[/purple]", "アタックでダメージを与えるたび、そのダメージに等しい[gold]破滅[/gold]を付与する。",
                "[purple]A purple shrouded in darkness.[/purple]", "Whenever Attacks deal damage, apply that much [gold]Doom[/gold]."),
            nameof(EchoForm) => ("ECHO_FORM",
                "[aqua]輝きをはなつ水色。[/aqua]", "毎ターン、最初にプレイするカードをもう一度プレイする。",
                "[aqua]A shining aqua.[/aqua]", "The first card you play each turn is played an extra time."),
            _ => null
        };

        if (form is not { } f) return null;
        var flavor = ModLocText.Get($"HYPNOSISCREATOR-COGNITIVE_{f.Key}_FLAVOR", f.FlavorJp, f.FlavorEn);
        var effect = ModLocText.Get($"HYPNOSISCREATOR-COGNITIVE_{f.Key}_EFFECT", f.EffectJp, f.EffectEn);
        return $"{flavor}\n{effect}";
    }
}
