using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 深淵からの声援 — 1ブロック。敵を破滅キルするたび、デッキ共有のこのカードのブロックが永続増加。廃棄。
/// UG: 増加量2→3。破滅キル時、次ターン開始にあらゆる場所から手札へ戻る（顕現 / SummonForth 型）。
/// 永続値は本家 GeneticAlgorithm と同じ SavedProperty ＋ DynamicVars.Block 同期パターン。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CheerFromTheAbyss() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private const int BaseBlock = 1;

    private int _currentBlock = BaseBlock;
    private int _increasedBlock;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // CardTag.Defend は付けない。本家締め直し(Fasten)はタグDefend付き全カードに乗るため、
    // スターター「防御」と「究極の防御」以外へ付けると誤って＋ブロックされる。

    [SavedProperty]
    public int CurrentBlock
    {
        get => _currentBlock;
        set
        {
            AssertMutable();
            _currentBlock = value;
            DynamicVars.Block.BaseValue = value;
        }
    }

    [SavedProperty]
    public int IncreasedBlock
    {
        get => _increasedBlock;
        set
        {
            AssertMutable();
            _increasedBlock = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(CurrentBlock, ValueProp.Move),
        new IntVar("Increase", 2M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    /// <summary>
    /// 戦闘中の山札／手札／捨て札／廃棄札にいるときフックを受ける。
    /// 「デッキにある＝所持していれば有効」は戦闘中の共有カード実体で満たす。
    /// </summary>
    public override async Task AfterDiedToDoom(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<Creature> creatures)
    {
        var killCount = creatures.Count(c => c.IsEnemy);
        if (killCount <= 0) return;

        var increase = DynamicVars["Increase"].IntValue;
        for (var i = 0; i < killCount; i++)
            BuffFromDoom(increase);

        if (DeckVersion is CheerFromTheAbyss deck && !ReferenceEquals(deck, this))
        {
            for (var i = 0; i < killCount; i++)
                deck.BuffFromDoom(increase);
        }

        if (!IsUpgraded || Owner?.Creature == null) return;

        // Single 再付与で Schedule 済み集合が消えないよう、未所持のときだけ Apply する
        var power = Owner.Creature.GetPower<AbyssCheerReturnPower>();
        if (power == null)
        {
            await PowerCmd.Apply<AbyssCheerReturnPower>(
                choiceContext, Owner.Creature, 1M, Owner.Creature, this);
            power = Owner.Creature.GetPower<AbyssCheerReturnPower>();
        }

        power?.Schedule(this);
    }

    public void BuffFromDoom(int extraBlock)
    {
        if (extraBlock <= 0) return;
        IncreasedBlock += extraBlock;
        UpdateBlock();
    }

    public void UpdateBlock() => CurrentBlock = BaseBlock + IncreasedBlock;

    protected override void OnUpgrade() => DynamicVars["Increase"].UpgradeValueBy(1M);

    protected override void AfterDowngraded() => UpdateBlock();
}
