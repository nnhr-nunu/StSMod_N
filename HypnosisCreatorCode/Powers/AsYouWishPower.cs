using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ご主人様の言うとおり — 性癖カードをプレイするたび、積み上げた合算値で自己強化する。
/// 未UG寄与: Abn 筋力1 / SM 活力2 / DomSub ブロック1。
/// UG寄与: Abn 筋力1+敏捷1 / SM 活力3 / DomSub ブロック2。
/// </summary>
public class AsYouWishPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int StrengthBonus { get; private set; }
    public int DexterityBonus { get; private set; }
    public int VigorBonus { get; private set; }
    public int BlockBonus { get; private set; }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is { IsUpgraded: true })
        {
            StrengthBonus += 1;
            DexterityBonus += 1;
            VigorBonus += 3;
            BlockBonus += 2;
        }
        else
        {
            StrengthBonus += 1;
            VigorBonus += 2;
            BlockBonus += 1;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (MassHypnosisPower.IsPropagating) return;

        var fetishes = CardFetishLookup.GetFetishes(cardPlay.Card);
        if (fetishes.Count == 0) return;

        foreach (var fetish in fetishes.Distinct())
        {
            switch (fetish)
            {
                case FetishType.Abnormal:
                    if (StrengthBonus > 0)
                        await PowerCmd.Apply<StrengthPower>(
                            choiceContext, Owner, StrengthBonus, Owner, cardPlay.Card);
                    if (DexterityBonus > 0)
                        await PowerCmd.Apply<DexterityPower>(
                            choiceContext, Owner, DexterityBonus, Owner, cardPlay.Card);
                    break;
                case FetishType.Sm:
                    if (VigorBonus > 0)
                        await PowerCmd.Apply<VigorPower>(
                            choiceContext, Owner, VigorBonus, Owner, cardPlay.Card);
                    break;
                case FetishType.DomSub:
                    if (BlockBonus > 0)
                        await CreatureCmd.GainBlock(Owner, BlockBonus, ValueProp.Unpowered, null);
                    break;
            }
        }
    }
}
