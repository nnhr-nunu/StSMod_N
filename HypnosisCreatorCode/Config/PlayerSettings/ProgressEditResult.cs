namespace HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings;

/// <summary>設定画面からセーブ（ProgressState）を書き換えた結果。</summary>
internal enum ProgressEditResult
{
    Changed,
    AlreadyDone,
    RunInProgress,
    ProfileChanged,
    Failed,
}
