using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>連続指パッチン — すべての敵に1ダメージ×5。リプレイX。廃棄。UGで保留。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class InfiniteFingerSnap() : HypnosisCreatorCard(-1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        BaseReplayCount = ResolveEnergyXValue();
        if (CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(5)
                .FromCard(this, play)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
