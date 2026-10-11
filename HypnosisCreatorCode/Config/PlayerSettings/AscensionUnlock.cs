using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings;

/// <summary>
/// セーブのアセンション上限を書き換える。シングル＝Hypno Creator だけ、マルチ＝全キャラ共通。
/// ラン中は変更しない（メインメニューでのみ）。保存に失敗したら元の値へ戻す。
/// </summary>
internal static class AscensionUnlock
{
    public const int Maximum = 10;

    public static ProgressEditResult UnlockThrough(int target, bool multiplayer)
    {
        if (target is < 1 or > Maximum)
            return ProgressEditResult.Failed;
        return Apply(multiplayer, $"unlock A{target}",
            before => before.Max >= target ? null : (target, before.Preferred));
    }

    public static ProgressEditResult Reset(bool multiplayer) =>
        Apply(multiplayer, "reset", before => before is (0, 0) ? null : (0, 0));

    /// <summary>チェック表示用。読めないときは null（セーブ未ロードなど）。</summary>
    public static int? ReadMaximum(bool multiplayer)
    {
        try
        {
            var progress = SaveManager.Instance.Progress;
            if (multiplayer)
                return progress.MaxMultiplayerAscension;
            return progress.CharacterStats.TryGetValue(CharacterId, out var stats) ? stats.MaxAscension : 0;
        }
        catch
        {
            return null;
        }
    }

    private static ModelId CharacterId => ModelDb.Character<HcCharacter>().Id;

    private static ProgressEditResult Apply(
        bool multiplayer,
        string action,
        Func<(int Max, int Preferred), (int Max, int Preferred)?> change)
    {
        ProgressState? progress = null;
        (int Max, int Preferred) before = default;
        var written = false;
        try
        {
            if (RunManager.Instance.IsInProgress)
                return ProgressEditResult.RunInProgress;

            progress = SaveManager.Instance.Progress;
            before = Read(progress, multiplayer);
            var after = change(before);
            if (after == null)
                return ProgressEditResult.AlreadyDone;

            Write(progress, multiplayer, after.Value);
            written = true;
            SaveManager.Instance.SaveProgressFile();
            return ProgressEditResult.Changed;
        }
        catch (Exception ex)
        {
            if (written && progress != null)
                Write(progress, multiplayer, before);
            var scope = multiplayer ? "multiplayer" : "singleplayer";
            MainFile.Logger.Warn($"Could not {action} ({scope} ascension): {ex.Message}");
            return ProgressEditResult.Failed;
        }
    }

    private static (int Max, int Preferred) Read(ProgressState progress, bool multiplayer)
    {
        if (multiplayer)
            return (progress.MaxMultiplayerAscension, progress.PreferredMultiplayerAscension);
        var stats = progress.GetOrCreateCharacterStats(CharacterId);
        return (stats.MaxAscension, stats.PreferredAscension);
    }

    private static void Write(ProgressState progress, bool multiplayer, (int Max, int Preferred) value)
    {
        if (multiplayer)
        {
            progress.MaxMultiplayerAscension = value.Max;
            progress.PreferredMultiplayerAscension = value.Preferred;
            return;
        }

        var stats = progress.GetOrCreateCharacterStats(CharacterId);
        stats.MaxAscension = value.Max;
        stats.PreferredAscension = value.Preferred;
    }
}
