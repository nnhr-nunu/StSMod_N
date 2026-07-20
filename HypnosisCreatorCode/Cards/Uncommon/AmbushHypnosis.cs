using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 不意打ち催眠 — 相手の数だけドローし、引いたカードをランダムな相手へ自動プレイ。
/// UGでドロー枚数+1。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AmbushHypnosis() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // UG用の追加ドロー（CalculatedVar の CalculationExtra とは別）
        new DynamicVar("Extra", 0M),
        // CalculatedVar = Base + CalculationExtra * Func。Extra=0 だとプレビューが常に0になる
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedVar("Draw").WithMultiplier(CalcDraw)
    ];

    /// <summary>静的必須（CalculatedVar.WithMultiplier の制約）。</summary>
    private static decimal CalcDraw(CardModel card, Creature? _)
    {
        var enemies = card.CombatState?.HittableEnemies.Count ?? 0;
        var extra = (int)card.DynamicVars["Extra"].BaseValue;
        return enemies + extra;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var drawCount = (int)CalcDraw(this, null);
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
}
