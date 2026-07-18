using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 集団催眠 — 単体対象のカウントカードをプレイすると、他の敵全員にも破滅が延焼する。
/// CSV: 本来はカード効果そのものを全体化する想定だが、個々カードの効果を汎用的に複製するAPIが
/// 無いため、破滅の疑似延焼で近似している。
/// TODO: 各カード効果を全体化する仕組みが確定したら差し替える。
/// </summary>
public class MassHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || CombatState == null) return;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        foreach (var enemy in CombatState.HittableEnemies.Where(e => e != primary).ToList())
        {
            await FetishCombat.ApplyDoom(
                choiceContext, enemy, FetishCombat.CalcFetishDoomAmount(enemy), Owner, cardPlay.Card);
        }
    }
}
