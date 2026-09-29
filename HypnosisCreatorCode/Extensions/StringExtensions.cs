using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    /// <summary>
    /// res:// パスは常に '/' 区切り。Path.Join は Windows で '' を挟み、
    /// 本家のアセットキャッシュ照合に外れる（"Asset not cached" 警告・遅延ロード）。
    /// </summary>
    private static string Join(params string[] parts) => string.Join('/', parts);

    private static readonly HashSet<string> MissingLogged = [];

    /// <summary>欠落アセットは描画のたびに引かれるので、同じパスは1回だけログに出す。</summary>
    private static void LogMissingOnce(string what, string path)
    {
        lock (MissingLogged)
        {
            if (!MissingLogged.Add(path)) return;
        }

        MainFile.Logger.Info($"Could not find {what} path: {path}");
    }

    public static string ImagePath(this string path)
    {
        return Join(MainFile.ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;

        LogMissingOnce("card image", path);
        return Join(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        LogMissingOnce("big card image", path);
        return Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;
        
        LogMissingOnce("power image", path);
        return Join(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        LogMissingOnce("big power image", path);
        return Join(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;
        
        LogMissingOnce("relic image", path);
        return Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        LogMissingOnce("big relic image", path);
        return Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return Join(MainFile.ResPath, "images", "charui", path);
    }

    public static string OrbImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "orbs", path);
        if (ResourceLoader.Exists(path)) return path;

        LogMissingOnce("orb image", path);
        return Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string PotionImagePath(this string path)
    {
        path = Join(MainFile.ResPath, "images", "potions", path);
        if (ResourceLoader.Exists(path)) return path;

        LogMissingOnce("potion image", path);
        return Join(MainFile.ResPath, "images", "potions", "potion.png");
    }
}