using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings;

/// <summary>
/// Slay the Spire 2 の全要素（エポック・カード・レリック・ポーション・イベント・アクト、全キャラ A10、マルチ A10）を解放する。
/// MOD 導入でセーブが分かれ、解放状況が初期化された人向け。途中で失敗したら書き換え前へ戻す。
/// </summary>
internal static class FullGameUnlock
{
    /// <param name="confirmedProgress">確認ダイアログを開いた時点のプロフィール。押すまでに切り替わっていたら中止する。</param>
    public static ProgressEditResult Apply(ProgressState? confirmedProgress)
    {
        Snapshot? snapshot = null;
        try
        {
            if (RunManager.Instance.IsInProgress)
                return ProgressEditResult.RunInProgress;

            var save = SaveManager.Instance;
            var progress = save.Progress;
            if (confirmedProgress == null)
                return ProgressEditResult.Failed;
            if (!ReferenceEquals(progress, confirmedProgress))
                return ProgressEditResult.ProfileChanged;

            (IReadOnlySet<ModelId> Discovered, ModelId[] All)[] discoveries =
            [
                (progress.DiscoveredCards, ModelDb.AllCards.Select(m => m.Id).Distinct().ToArray()),
                (progress.DiscoveredRelics, ModelDb.AllRelics.Select(m => m.Id).Distinct().ToArray()),
                (progress.DiscoveredPotions, ModelDb.AllPotions.Select(m => m.Id).Distinct().ToArray()),
                (progress.DiscoveredEvents, ModelDb.AllEvents.Select(m => m.Id).Distinct().ToArray()),
                (progress.DiscoveredActs, ModelDb.Acts.Select(m => m.Id).Distinct().ToArray()),
            ];
            var characterIds = ModelDb.AllCharacters.Select(m => m.Id).Distinct().ToArray();
            snapshot = new Snapshot(progress, discoveries.Select(d => d.Discovered));

            var changed = false;
            foreach (var epochId in EpochModel.AllEpochIds)
            {
                if (progress.IsEpochRevealed(epochId))
                    continue;
                progress.ObtainEpochOverride(epochId, EpochState.Revealed);
                changed = true;
            }

            foreach (var (discovered, all) in discoveries)
            {
                var set = (HashSet<ModelId>)discovered;
                foreach (var id in all)
                    changed |= set.Add(id);
            }

            foreach (var characterId in characterIds)
            {
                var stats = progress.GetOrCreateCharacterStats(characterId);
                if (stats.MaxAscension >= AscensionUnlock.Maximum)
                    continue;
                stats.MaxAscension = AscensionUnlock.Maximum;
                changed = true;
            }

            if (progress.MaxMultiplayerAscension < AscensionUnlock.Maximum)
            {
                progress.MaxMultiplayerAscension = AscensionUnlock.Maximum;
                changed = true;
            }

            if (!changed)
                return ProgressEditResult.AlreadyDone;

            save.SaveProgressFile();
            return ProgressEditResult.Changed;
        }
        catch (Exception ex)
        {
            snapshot?.Restore();
            MainFile.Logger.Warn($"Could not unlock all game content: {ex.Message}");
            return ProgressEditResult.Failed;
        }
    }

    /// <summary>書き換え前の解放状況。保存に失敗したときだけ使う。</summary>
    private sealed class Snapshot
    {
        private readonly ProgressState _progress;
        private readonly List<SerializableEpoch> _epochs;
        private readonly (SerializableEpoch Epoch, EpochState State, long ObtainDate)[] _previousEpochs;
        private readonly Dictionary<ModelId, CharacterStats> _characters;
        private readonly (ModelId Id, CharacterStats Stats, int MaxAscension)[] _previousCharacters;
        private readonly (HashSet<ModelId> Set, HashSet<ModelId> Previous)[] _discoveries;
        private readonly int _multiplayerAscension;

        public Snapshot(ProgressState progress, IEnumerable<IReadOnlySet<ModelId>> discoveries)
        {
            _progress = progress;
            _epochs = progress.Epochs as List<SerializableEpoch>
                      ?? throw new InvalidOperationException("Unsupported epoch storage.");
            _characters = progress.CharacterStats as Dictionary<ModelId, CharacterStats>
                          ?? throw new InvalidOperationException("Unsupported character storage.");
            _previousEpochs = _epochs.Select(e => (e, e.State, e.ObtainDate)).ToArray();
            _previousCharacters = _characters.Select(c => (c.Key, c.Value, c.Value.MaxAscension)).ToArray();
            _discoveries = discoveries
                .Select(d => d as HashSet<ModelId> ?? throw new InvalidOperationException("Unsupported discovery storage."))
                .Select(set => (set, new HashSet<ModelId>(set)))
                .ToArray();
            _multiplayerAscension = progress.MaxMultiplayerAscension;
        }

        public void Restore()
        {
            _epochs.Clear();
            foreach (var (epoch, state, obtainDate) in _previousEpochs)
            {
                epoch.State = state;
                epoch.ObtainDate = obtainDate;
                _epochs.Add(epoch);
            }

            _characters.Clear();
            foreach (var (id, stats, maxAscension) in _previousCharacters)
            {
                stats.MaxAscension = maxAscension;
                _characters.Add(id, stats);
            }

            foreach (var (set, previous) in _discoveries)
            {
                set.Clear();
                set.UnionWith(previous);
            }

            _progress.MaxMultiplayerAscension = _multiplayerAscension;
        }
    }
}
