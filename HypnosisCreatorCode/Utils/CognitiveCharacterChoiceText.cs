using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル選択カードの説明を、各キャラ色の Form パワー効果文に差し替える。
/// </summary>
public static class CognitiveCharacterChoiceText
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        _ = target;
        if (card is not CognitiveCharacterChoice choice) return;

        var text = DescribeForm(choice.FormCardType);
        if (text != null) description = text;
    }

    private static string? DescribeForm(Type? formCardType)
    {
        if (formCardType == null) return null;
        var japanese = UpgradeCardText.IsJapaneseUi();

        return formCardType.Name switch
        {
            nameof(DemonForm) => japanese
                ? "[red]ターン開始時、[gold]筋力[/gold][blue]3[/blue]を得る。[/red]"
                : "[red]At the start of your turn, gain [blue]3[/blue] [gold]Strength[/gold].[/red]",
            nameof(SerpentForm) => japanese
                ? "[green]カードをプレイするたび、ランダムな敵に[blue]4[/blue]ダメージを与える。[/green]"
                : "[green]Whenever you play a card, deal [blue]4[/blue] damage to a random enemy.[/green]",
            nameof(VoidForm) => japanese
                ? "[orange]毎ターン、最初にプレイする[blue]2[/blue]枚のカードはコストを支払わずにプレイできる。[/orange]"
                : "[orange]The first [blue]2[/blue] cards you play each turn cost [blue]0[/blue].[/orange]",
            nameof(ReaperForm) => japanese
                ? "[purple]アタックでダメージを与えるたび、そのダメージに等しい[gold]破滅[/gold]を付与する。[/purple]"
                : "[purple]Whenever you deal Attack damage, apply [gold]Doom[/gold] equal to the damage dealt.[/purple]",
            nameof(EchoForm) => japanese
                ? "[aqua]毎ターン、最初にプレイするカードをもう一度プレイする。[/aqua]"
                : "[aqua]The first card you play each turn is played an extra time.[/aqua]",
            _ => null
        };
    }
}
