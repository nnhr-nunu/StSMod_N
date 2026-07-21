using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Models;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル選択カードのポートレートを、キャラ選択画面の顔トリムへ差し替える。
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_Portrait")]
public static class CognitiveCharacterPortraitPatch
{
    public static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        if (__instance is not CognitiveCharacterChoice choice) return;
        if (choice.LinkedCharacter == null) return;

        var face = CognitiveCharacterFaces.GetFacePortrait(choice.LinkedCharacter);
        if (face != null)
            __result = face;
    }
}
