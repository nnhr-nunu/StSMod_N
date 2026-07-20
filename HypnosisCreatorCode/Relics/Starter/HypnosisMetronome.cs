using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 催眠メトロノーム — ヒプノクリエイター専用。
/// 戦闘1ターン目: 開幕ドロー+1（Bag of Preparation と同様）＋敵全体にトランス1。
/// トランス減少は AfterSideTurnStart のため、付与は AfterSideTurnStartLate で行う
/// （AfterPlayerTurnStart だと直後の減少で消える）。本家 Bag of Marbles 系の開幕デバフと同趣旨。
/// </summary>
public class HypnosisMetronome : HypnosisCreatorRelic
{
    private const int TranceAmount = 1;
    private const int ExtraDraw = 1;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", TranceAmount),
        new DynamicVar("Cards", ExtraDraw)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<TrancePower>()];

    /// <summary>他キャラの報酬・ショップ・Neow に出さない。</summary>
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Any(IsHypnosisCreator);

    public override bool IsAllowedAtNeow(Player player) =>
        IsHypnosisCreator(player);

    /// <summary>Bag of Preparation 系: 戦闘1ターン目の手札ドロー枚数を増やす。</summary>
    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || Owner?.PlayerCombatState == null) return count;
        if (Owner.PlayerCombatState.TurnNumber != 1) return count;
        return count + ExtraDraw;
    }

    /// <summary>
    /// SetupPlayerTurn（AfterPlayerTurnStart 含む）のあと、AfterSideTurnStart の減少パスを抜けた Late で付与する。
    /// </summary>
    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || side != CombatSide.Player) return;
        if (!participants.Contains(Owner.Creature)) return;
        if (Owner.PlayerCombatState == null || Owner.PlayerCombatState.TurnNumber > 1) return;

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, TranceAmount, Owner.Creature, cardSource: null);
        }
    }

    private static bool IsHypnosisCreator(Player player) =>
        player.Character?.Id.Entry.Contains(
            HcCharacter.CharacterId, StringComparison.OrdinalIgnoreCase) == true;
}
