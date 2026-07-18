using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>布教欲求 — 対象に沼を付与し、自ターン終了時にその破滅・沼を他の敵全員へコピーする。戦闘終了時にゴールドを得る。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Proselytize() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BogPower>(2M),
        new DynamicVar("Gold", 15M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<OshiActivityPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        var oshi = Owner.Creature.GetPower<OshiActivityPower>();
        if (oshi != null)
        {
            oshi.SourceEnemy = play.Target;
            oshi.GoldToGain += DynamicVars["Gold"].BaseValue;
        }

        await PowerCmd.Apply<BogPower>(
            choiceContext, play.Target, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(10M);
}
