using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 集団催眠 — 単体対象のカウントカードを、他の敵にも AutoPlay コピーで波及させる。
/// 再入防止付き。カード効果の完全複製に近い best-effort。
/// </summary>
public class MassHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    [ThreadStatic]
    private static bool _resolving;

    [ThreadStatic]
    private static int _suppressDepth;

    /// <summary>集団催眠の AutoPlay 波及中か。プレイヤー側の「1枚1回」副作用を抑止する判定用。</summary>
    public static bool IsPropagating => _resolving;

    /// <summary>
    /// オールインワン等の内部 AutoPlay 中。
    /// 集団催眠の再波及を止め、内部プレイは対象1体に留める。
    /// </summary>
    public static bool IsPropagationSuppressed => _suppressDepth > 0;

    public static void BeginSuppressPropagation() => _suppressDepth++;

    public static void EndSuppressPropagation()
    {
        if (_suppressDepth > 0)
            _suppressDepth--;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_resolving || IsPropagationSuppressed) return;
        if (Owner == null || CombatState == null) return;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var others = CombatState.HittableEnemies.Where(e => e != primary && e.IsAlive).ToList();
        if (others.Count == 0) return;

        _resolving = true;
        try
        {
            var canonical = cardPlay.Card.CanonicalInstance ?? cardPlay.Card;
            foreach (var enemy in others)
            {
                var copy = CombatState.CreateCard(canonical, player);
                await CardCmd.AutoPlay(choiceContext, copy, enemy);
            }
        }
        finally
        {
            _resolving = false;
        }
    }
}
