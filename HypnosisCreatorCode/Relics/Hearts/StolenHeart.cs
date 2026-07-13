using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 奪った心臓の汎用レリック。スタックする。
/// 個別モンスター心臓は今後この枠を拡張する。
/// </summary>
public class StolenHeart : HypnosisCreatorRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool IsStackable => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Hearts", 1M)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (Owner.PlayerCombatState.TurnNumber > 1) return;
        var stacks = Owner.Relics.Count(r => r is StolenHeart);
        if (stacks <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, stacks * 2m, ValueProp.Move, null);
    }
}
