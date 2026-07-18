using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 教祖化 — 有効な間 SM・DomSub・アブノーマルの性癖カードは必ず刺さる（トランス性癖は対象外）。
/// トランス付与時、次のターンのエナジー+1・ドロー+Amount を得る（<see cref="TranceCombat"/> 側から通知）。
/// </summary>
public class CultLeaderPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
    {
        FetishCombat.CultLeaderActive = true;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        FetishCombat.CultLeaderActive = false;
        return Task.CompletedTask;
    }
}
