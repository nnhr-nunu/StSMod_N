using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ぜんぶ知ってるよ — 性癖刺さりの破滅倍率をこの戦闘中だけ上げる。
/// 戦闘終了時のリセットは <see cref="Patches.FetishCombatResetPatch"/> が担当。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class KnowItAll() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", 0.5M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        FetishCombat.FetishHitMultiplier += DynamicVars["Multiplier"].BaseValue;
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Multiplier"].UpgradeValueBy(0.5M);
}
