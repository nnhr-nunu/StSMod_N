using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 不意打ち催眠 — 相手の数だけドローし、引いたカードをランダムな相手へ自動プレイ。
/// UGでドロー枚数+1。枚数プレビューは戦闘中のみ（本家 CalculatedVar と同様、非戦闘では出さない）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AmbushHypnosis() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    static AmbushHypnosis()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendDrawPreview;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Extra", 0M)];

    private static int CalcDraw(CardModel card)
    {
        var enemies = card.CombatState?.HittableEnemies.Count ?? 0;
        var extra = (int)card.DynamicVars["Extra"].BaseValue;
        return enemies + extra;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var drawCount = CalcDraw(this);
        if (drawCount <= 0) return;

        var player = Owner;
        var drawn = await CardPileCmd.Draw(choiceContext, drawCount, player);
        var rng = player.RunState.Rng.CombatCardSelection;

        foreach (var card in drawn.ToList())
        {
            var enemies = CombatState.HittableEnemies.ToList();
            if (enemies.Count == 0) break;
            var target = enemies[rng.NextInt(enemies.Count)];
            await CardCmd.AutoPlay(choiceContext, card, target);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Extra"].UpgradeValueBy(1M);

    private static void AppendDrawPreview(CardModel card, Creature? _, ref string description)
    {
        if (card is not AmbushHypnosis ambush) return;
        var n = CalcDraw(ambush);
        if (n <= 0) return;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（{n}枚）"
            : $" ({n})";
        CombatPreviewText.AppendSuffix(ambush, ref description, suffix);
    }
}
