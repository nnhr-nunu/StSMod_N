using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ジャックフルーツの心臓 — 希少。フルーツジュースを入手。</summary>
public class JackfruitHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "JACKFRUIT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRarePotion<FruitJuice>(this, player);
}
