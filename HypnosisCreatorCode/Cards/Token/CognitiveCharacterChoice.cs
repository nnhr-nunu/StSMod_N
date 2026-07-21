using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 認知シャッフルの選択用キャラクターカード（ライブラリ非表示・プレイ不可）。
/// ポートレートはキャラ選択画面アイコンの顔トリム（Harmony で差し替え）。
/// </summary>
public class CognitiveCharacterChoice() : HypnosisCreatorCard(
    0, CardType.Skill, CardRarity.Token, TargetType.Self,
    showInCardLibrary: false)
{
    /// <summary>対応する本家 Form カード型。</summary>
    public Type? FormCardType { get; set; }

    /// <summary>見た目差し替え先キャラ。</summary>
    public CharacterModel? LinkedCharacter { get; set; }

    // 専用画像は無く、Harmony 差し替え前のフォールバック用
    public override string PortraitPath => "cognitive_shuffle.png".CardImagePath();
    public override string CustomPortraitPath => "cognitive_shuffle.png".BigCardImagePath();

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public override string Title =>
        LinkedCharacter?.Title.GetFormattedText()
        ?? base.Title;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 選択UI専用。手札プレイは想定しない。
        await Task.CompletedTask;
    }
}
