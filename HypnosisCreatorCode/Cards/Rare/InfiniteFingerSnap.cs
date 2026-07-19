using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 連続指パッチン — すべての敵に1ダメージ×5。リプレイX。廃棄。UGで保留。
/// X解決後にリプレイ回数をセットし、本家 Whirlwind と同様に1回の Attack（多段ヒット）で解決する。
/// （Execute を手動ループするとカードが宙吊りになるため）
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteFingerSnap() : HypnosisCreatorCard(-1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    /// <summary>
    /// X消費が確定した直後にリプレイXを載せる。
    /// GeneratePlayCount より前の PrePlay でセットする。
    /// </summary>
    public override Task AfterAutoPrePlayPhaseEnteredEarly(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
            BaseReplayCount = Math.Max(0, ResolveEnergyXValue());
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        // リプレイはゲーム側が OnPlay を繰り返す。ここは「1ダメージ×5」だけ。
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(5)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
