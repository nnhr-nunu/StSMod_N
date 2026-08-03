using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Basic;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家のストライク／防御タグ前提レリック・イベントを HC スターターへ橋渡しする。
/// </summary>
public static class HcStarterStrikeCompatPatch
{
    [HarmonyPatch(typeof(CardModel), "get_Tags")]
    public static class TagsPatch
    {
        public static IEnumerable<CardTag> Postfix(IEnumerable<CardTag> __result, CardModel __instance)
        {
            if (!HcStarterStrikeCompat.ShouldInjectStrikeTag(__instance))
                return __result;

            if (__result.Contains(CardTag.Strike))
                return __result;

            return __result.Append(CardTag.Strike);
        }
    }

    [HarmonyPatch(typeof(CardModel), "get_IsBasicStrikeOrDefend")]
    public static class IsBasicStrikeOrDefendPatch
    {
        public static void Postfix(CardModel __instance, ref bool __result)
        {
            if (__result) return;
            if (HcStarterStrikeCompat.MatchesBasicStrikeOrDefend(__instance))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
    public static class LargeCapsuleStrikePatch
    {
        public static bool Prefix(LargeCapsule __instance, CharacterModel character, ref CardModel __result)
        {
            if (!HcStarterStrikeCompat.IsHcCharacter(character))
                return true;

            var owner = __instance.Owner;
            __result = owner != null
                ? HcStarterStrikeCompat.GetStandInPrototype(owner)
                : ModelDb.Card<StunGun>();
            return false;
        }
    }

    [HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
    public static class LargeCapsuleDefendPatch
    {
        public static bool Prefix(CharacterModel character, ref CardModel __result)
        {
            if (!HcStarterStrikeCompat.IsHcCharacter(character))
                return true;

            __result = ModelDb.Card<HcDefend>();
            return false;
        }
    }

    [HarmonyPatch(typeof(Amalgamator), "IsValid")]
    public static class AmalgamatorIsValidPatch
    {
        public static void Postfix(CardTag tag, CardModel card, ref bool __result)
        {
            if (__result || tag != CardTag.Strike) return;
            if (HcStarterStrikeCompat.IsStandInStrike(card))
                __result = true;
        }
    }

    /// <summary>スターター攻撃3枚は仮想 Strike を付けてもダミー対象外（時止めストライク等の実タグのみ）。</summary>
    [HarmonyPatch(typeof(StrikeDummy), "ModifyDamageAdditive")]
    [HarmonyPatch(typeof(FakeStrikeDummy), "ModifyDamageAdditive")]
    public static class StrikeDummyExcludeHcStarterPatch
    {
        public static void Postfix(CardModel card, ref decimal __result)
        {
            if (HcStarterStrikeCompat.IsStrikeDummyExcludedStarter(card))
                __result = 0M;
        }
    }
}
