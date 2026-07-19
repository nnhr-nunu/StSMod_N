using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Relics.Starter;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Character;

public class HypnosisCreator : PlaceholderCharacterModel
{
    public const string CharacterId = "HypnosisCreator";
    public static readonly Color Color = new("e85aad");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 72;

    /// <summary>防御5＋他スターター各1（mechanics-lock / CSV）。</summary>
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<StunGun>(),
        ModelDb.Card<SayYoureSorry>(),
        ModelDb.Card<WristCut>(),
        ModelDb.Card<FingerSnap>(),
        ModelDb.Card<Harmony>(),
        ModelDb.Card<Mirroring>(),
        ModelDb.Card<BeginnerHypnosis>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<FetishBog>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<HypnosisCreatorCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HypnosisCreatorRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HypnosisCreatorPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
    public override string CustomVisualPath =>
        $"{MainFile.ResPath}/scenes/creature_visuals/hypnosis_creator.tscn";
    public override string CustomCharacterSelectBg =>
        $"{MainFile.ResPath}/scenes/char_select/hypnosis_creator_bg.tscn";

    // BaseLib 既定 (0.15 / 0.25) だとパワー詠唱がすぐ Idle に戻るため、連番尺に合わせる
    public override float AttackAnimDelay => CombatFrameAnimator.AttackAnimSeconds;
    public override float CastAnimDelay => CombatFrameAnimator.CastAnimSeconds;
    public override float PowerUpAnimDelay => CombatFrameAnimator.CastAnimSeconds;
}
