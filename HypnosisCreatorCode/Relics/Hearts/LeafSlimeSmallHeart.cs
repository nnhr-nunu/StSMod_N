using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リーフスライム(小)の心臓 — 希少。0コスト粘液を手札へ。</summary>
public class LeafSlimeSmallHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "LEAF_SLIME_SMALL";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        var slime = ModelDb.Card<Slimed>().ToMutable();
        slime.Owner = player;
        slime.EnergyCost.SetThisCombat(0);
        await CardPileCmd.Add(slime, PileType.Hand, CardPilePosition.Bottom, this);
        MarkUsed();
    }
}
