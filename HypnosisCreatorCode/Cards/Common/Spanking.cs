using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>スパンキング — SMアタック 5×2。刺さったらリプレイ1（UGで2）。説明はリプレイキーワードと二重化しない。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Spanking() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5M, ValueProp.Move),
        new DynamicVar("Replays", 1M)
    ];

    /// <summary>GeneratePlayCount Prefix から呼ぶ。PlayCount 確定より前に BaseReplayCount をセットする。</summary>
    internal void PrepareReplay(Creature? target)
    {
        BaseReplayCount = 0;
        if (target is not { IsAlive: true, IsEnemy: true }) return;
        if (!ShouldReplayForTarget(target)) return;
        BaseReplayCount = DynamicVars["Replays"].IntValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["Replays"].UpgradeValueBy(1M);

    private bool ShouldReplayForTarget(Creature target)
    {
        if (FetishCombat.CultLeaderActive) return true;
        if (FetishCombat.WasAwakenedThisPlay(target, FetishType.Sm)) return false;
        return FetishCombat.HasFetish(target, FetishType.Sm);
    }
}
