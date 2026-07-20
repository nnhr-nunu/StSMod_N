using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Extensions;

namespace HypnosisCreator.HypnosisCreatorCode.Potions;

[Pool(typeof(HypnosisCreatorPotionPool))]
public abstract class HypnosisCreatorPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionImagePath();
}
