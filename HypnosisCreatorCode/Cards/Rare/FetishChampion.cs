using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 性癖の覇者 — 必ず性癖に刺さる。8ダメージ（UG12）。対象の性癖数だけリプレイ（最低1）。
/// リプレイ回数は戦闘中プレビュー。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishChampion() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Abnormal, FetishType.Sm, FetishType.DomSub];

    public override bool AlwaysHitsFetish => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8M, ValueProp.Move)];

    internal static int CalcReplayCount(Creature? target) =>
        target is not { IsAlive: true, IsEnemy: true }
            ? 0
            : Math.Max(1, FetishCombat.GetFetishes(target).Count);

    /// <summary>GeneratePlayCount Prefix から呼ぶ。PlayCount 確定より前に BaseReplayCount をセットする。</summary>
    internal void PrepareReplay(Creature? target)
    {
        var replays = CalcReplayCount(target);
        if (replays <= 0) return;
        BaseReplayCount = Math.Max(BaseReplayCount, replays);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);

    internal static void AppendDescriptionSuffix(CardModel card, Creature? target, ref string description)
    {
        if (card is not FetishChampion champion) return;
        if (!CombatPreviewText.IsActive(champion)) return;

        var previewTarget = target ?? champion.CurrentTarget;
        var replays = CalcReplayCount(previewTarget);
        if (replays <= 0) return;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（リプレイ{replays}）"
            : $" (Replay {replays})";
        CombatPreviewText.AppendSuffix(champion, ref description, suffix);
    }
}
