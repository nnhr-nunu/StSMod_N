using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Relics.Starter;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Character;

public class HypnosisCreator : PlaceholderCharacterModel
{
    public const string CharacterId = "HypnosisCreator";

    // 催眠っぽいマゼンタ寄り
    public static readonly Color Color = new("e85aad");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 72;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<HcStrike>(),
        ModelDb.Card<HcStrike>(),
        ModelDb.Card<HcStrike>(),
        ModelDb.Card<HcStrike>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<HcDefend>(),
        ModelDb.Card<CountBeat>(),
        ModelDb.Card<PleasureCycle>(),
        ModelDb.Card<HeartClaim>()
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

    // バトル立ち絵（左向き素材のためシーン側で Flip H）
    public override string CustomVisualPath =>
        $"{MainFile.ResPath}/scenes/creature_visuals/hypnosis_creator.tscn";

    // キャラ選択画面の1枚絵背景
    public override string CustomCharacterSelectBg =>
        $"{MainFile.ResPath}/scenes/char_select/hypnosis_creator_bg.tscn";
}
