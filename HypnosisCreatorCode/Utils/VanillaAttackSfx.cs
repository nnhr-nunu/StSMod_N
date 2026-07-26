using MegaCrit.Sts2.Core.Commands;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>本家 StS2 のヒット SE（mp3 / FMOD event）。</summary>
public static class VanillaAttackSfx
{
    /// <summary>通常打撃（時止めストライク等）。</summary>
    public const string StrikeHitFile = "blunt_attack.mp3";

    /// <summary>重めの鈍撃（Uppercut / Bludgeon 系）。</summary>
    public const string HeavyHitFile = "heavy_attack.mp3";

    /// <summary>サイレント系ナイフ（Dagger Throw 等）。</summary>
    public const string KnifeHitFile = "dagger_throw.mp3";

    /// <summary>絞め蛇（Slithering Strangler）の締め付け。</summary>
    public const string ConstrictCastEvent =
        "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_cast";

    public static void PlayStrike() => SfxCmd.Play(StrikeHitFile, 1f);

    public static void PlayConstrictCast() => SfxCmd.Play(ConstrictCastEvent, 1f);
}
