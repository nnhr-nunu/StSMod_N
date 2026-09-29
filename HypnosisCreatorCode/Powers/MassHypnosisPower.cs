using HypnosisCreator.HypnosisCreatorCode.Utils;
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

    // await の流れごとに持つ（AsyncLocal）。ThreadStatic だと全プレイヤーで共有され、共闘で片方のカードが
    // 選択待ちで止まっている間、もう片方の行動まで「波及中／抑止中」扱いになる。
    private static readonly AsyncLocal<bool> Resolving = new();

    private static readonly AsyncLocal<int> SuppressDepth = new();

    /// <summary>集団催眠の AutoPlay 波及中か。プレイヤー側の「1枚1回」副作用を抑止する判定用。</summary>
    public static bool IsPropagating => Resolving.Value;

    /// <summary>
    /// オールインワン等の内部 AutoPlay 中。
    /// 集団催眠の再波及を止め、内部プレイは対象1体に留める。
    /// </summary>
    public static bool IsPropagationSuppressed => SuppressDepth.Value > 0;

    public static void BeginSuppressPropagation() => SuppressDepth.Value++;

    public static void EndSuppressPropagation()
    {
        if (SuppressDepth.Value > 0)
            SuppressDepth.Value--;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Resolving.Value || IsPropagationSuppressed) return;
        if (Owner == null || CombatState == null) return;
        // 自分がプレイしたカードだけ（共闘で仲間のカードまで波及させない／2人とも所持で二重波及しない）
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var others = CombatState.HittableEnemies.Where(e => e != primary && e.IsAlive).ToList();
        if (others.Count == 0) return;

        Resolving.Value = true;
        try
        {
            foreach (var enemy in others)
                await PropagatedCardPlay.OnEnemy(choiceContext, CombatState, cardPlay.Card, player, enemy);
        }
        finally
        {
            Resolving.Value = false;
        }
    }
}
