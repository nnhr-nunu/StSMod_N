using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ヒプノクリエイター付与時の「締め付け」文言。絞蛇付与時は本家キーのままにする。
/// </summary>
public static class ConstrictPowerHcText
{
    public static LocString Description =>
        new("powers", "HYPNOSISCREATOR-CONSTRICT_FROM_HC.description");

    public static LocString SmartDescription =>
        new("powers", "HYPNOSISCREATOR-CONSTRICT_FROM_HC.smartDescription");

    public static IHoverTip CardHoverTip()
    {
        var power = ModelDb.Power<ConstrictPower>();
        return new HoverTip(power.Title, Description, power.Icon);
    }

    public static bool ShouldUseHcText(PowerModel power)
    {
        if (power is not ConstrictPower) return false;
        return IsHypnosisCreator(power.Applier);
    }

    private static bool IsHypnosisCreator(Creature? creature) =>
        creature?.Player?.Character?.Id.Entry.Contains(
            Character.HypnosisCreator.CharacterId, StringComparison.OrdinalIgnoreCase) == true;
}
