using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 催眠メトロノーム — ヒプノクリエイター専用。
/// 戦闘1ターン目: 開幕ドロー+1（Bag of Preparation と同様）＋敵全体にトランス1。
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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (Owner.PlayerCombatState.TurnNumber > 1) return;
        if (Owner.Creature.CombatState == null) return;

        Flash();
        foreach (var enemy in Owner.Creature.CombatState.HittableEnemies.ToList())
        {
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, TranceAmount, Owner.Creature, cardSource: null);
        }
    }

    private static bool IsHypnosisCreator(Player player) =>
        player.Character?.Id.Entry.Contains(
            HcCharacter.CharacterId, StringComparison.OrdinalIgnoreCase) == true;
}
