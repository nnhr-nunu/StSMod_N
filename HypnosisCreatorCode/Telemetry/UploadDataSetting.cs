using MegaCrit.Sts2.Core.Saves;

namespace HypnosisCreator.HypnosisCreatorCode.Telemetry;

/// <summary>
/// 匿名データ送信の ON/OFF。本家の「データ送信」設定（PrefsSave.UploadData）をそのまま使う。
/// 本家で OFF にしている人からは送らない。MOD 画面のスイッチは本家の設定も切り替える。
/// </summary>
internal static class UploadDataSetting
{
    public static bool TryGet(out bool enabled)
    {
        enabled = false;
        try
        {
            var save = SaveManager.Instance;
            if (!save.IsPrefsLoaded)
                return false;
            enabled = save.PrefsSave.UploadData;
            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Could not read the game's upload-data setting: {ex.Message}");
            return false;
        }
    }

    public static bool IsEnabled => TryGet(out var enabled) && enabled;

    /// <summary>本家の設定を書き換えて保存する。保存に失敗したら元に戻して false。</summary>
    public static bool TrySet(bool enabled)
    {
        try
        {
            var save = SaveManager.Instance;
            if (!save.IsPrefsLoaded)
                return false;

            var prefs = save.PrefsSave;
            var before = prefs.UploadData;
            if (before == enabled)
                return true;

            prefs.UploadData = enabled;
            try
            {
                save.SavePrefsFile();
            }
            catch
            {
                prefs.UploadData = before;
                throw;
            }

            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Could not save the game's upload-data setting: {ex.Message}");
            return false;
        }
    }
}
