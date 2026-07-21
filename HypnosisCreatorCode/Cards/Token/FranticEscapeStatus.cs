using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>
/// 決死の逃亡 — 相手を遠ざけ、蟻地獄+1、このカードのコスト+1。廃棄。
/// 備考: 貪りの獣想定。自分が逃げるのではなく相手が離れる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FranticEscapeStatus() : PlayableStatusCard(0,
    CardType.Status, CardRarity.Status, TargetType.AnyEnemy)
{
    public override string PortraitPath => "frantic_escape.png".CardImagePath();
    public override string CustomPortraitPath => "frantic_escape.png".BigCardImagePath();

    // CSV: 廃棄なし（コスト増加を積み上げる想定）
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<SandpitPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PullTracker.TryPushAway(play.Target, Owner.Creature);

        var sandpitEnemy = CombatState?.Enemies
            .FirstOrDefault(c => c.IsAlive && c.HasPower<SandpitPower>());
        var sandpit = sandpitEnemy?.Powers.OfType<SandpitPower>()
            .FirstOrDefault(s => s.Target == Owner.Creature);
        if (sandpit != null && sandpitEnemy != null)
            await PowerCmd.ModifyAmount(choiceContext, sandpit, 1m, sandpitEnemy, this);

        EnergyCost.AddThisCombat(1);
    }
}
