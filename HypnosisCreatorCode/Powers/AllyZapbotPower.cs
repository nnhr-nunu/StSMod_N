using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 味方ザップマシン用。プレイヤーターン終了時にランダムな敵を1体攻撃する。
/// </summary>
public class AllyZapbotPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "fabricator_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "fabricator_heart.png".BigRelicImagePath();

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (!Owner.IsPet || Owner.Monster is not Zapbot) return;

        var ownerCreature = Owner.PetOwner?.Creature;
        if (ownerCreature == null) return;

        var list = participants as ICollection<Creature> ?? participants.ToList();
        if (!list.Contains(ownerCreature) && !list.Contains(Owner)) return;

        Flash();
        await AllyZapbotAttacks.Perform(choiceContext, Owner);
    }
}
