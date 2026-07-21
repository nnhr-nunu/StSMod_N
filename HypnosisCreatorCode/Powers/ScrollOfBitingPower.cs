using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 噛みつきの巻物 — ブロックされずにダメージを与えるたび、追加で Amount ダメージ。
/// </summary>
public class ScrollOfBitingPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "scroll_of_biting_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "scroll_of_biting_heart.png".BigRelicImagePath();

    private bool _echoing;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (_echoing) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (dealer != Owner) return;
        if (target == null || !target.IsAlive) return;
        if (result.UnblockedDamage <= 0) return;
        if (Amount <= 0) return;

        _echoing = true;
        try
        {
            await CreatureCmd.Damage(
                choiceContext,
                target,
                Amount,
                ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                Owner,
                null,
                null);
        }
        finally
        {
            _echoing = false;
        }
    }
}
