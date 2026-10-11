using System.Globalization;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Telemetry;

/// <summary>
/// ラン終了時に送る匿名プレイデータ。Steam ID・名前・シードは含めない。
/// 項目を変えたら server/src/index.js の isValidRun と schemaVersion も合わせる。
/// </summary>
internal sealed record RunReport(
    int SchemaVersion,
    string ReportId,
    string ModVersion,
    string GameVersion,
    string Language,
    string GameMode,
    int ModifierCount,
    int Ascension,
    int PlayerCount,
    string Character,
    string[] PartyCharacters,
    string Outcome,
    int FloorsVisited,
    int ActsVisited,
    string? KilledBy,
    int? RunTimeSeconds,
    int? FinalHp,
    int? FinalMaxHp,
    bool HasOtherGameplayMods,
    RunDeckCard[] Deck,
    string[] Relics,
    RunCardChoice[] CardChoices,
    RunModelChoice[] RelicChoices);

internal sealed record RunDeckCard(string Id, int Upgrade, int Count);

internal sealed record RunCardChoice(int Floor, string Id, int Upgrade, bool Picked);

internal sealed record RunModelChoice(int Floor, string Id, bool Picked);

internal static class RunReportBuilder
{
    // Worker 側の上限（isValidRun）と揃える。超えた分は送らない
    private const int MaxDeckEntries = 1000;
    private const int MaxRelics = 500;
    private const int MaxChoices = 2000;
    private const int MaxParty = 16;

    /// <summary>自分のキャラが Hypno Creator のランだけ作る。それ以外は null。</summary>
    public static RunReport? TryCreate(RunHistory history)
    {
        var player = FindLocalPlayer(history);
        if (player == null || player.Character.ToString() != ModelDb.Character<HcCharacter>().Id.ToString())
            return null;

        var cardChoices = new List<RunCardChoice>();
        var relicChoices = new List<RunModelChoice>();
        PlayerMapPointHistoryEntry? lastStats = null;
        var floor = 0;
        foreach (var act in history.MapPointHistory)
        {
            foreach (var point in act)
            {
                floor++;
                var stats = point.PlayerStats.FirstOrDefault(p => p.PlayerId == player.Id);
                if (stats == null)
                    continue;
                lastStats = stats;

                foreach (var choice in stats.CardChoices)
                {
                    var id = choice.Card?.Id?.ToString();
                    if (TelemetryClient.IsSafeModelId(id))
                        cardChoices.Add(new RunCardChoice(floor, id!, choice.Card!.CurrentUpgradeLevel, choice.wasPicked));
                }

                foreach (var choice in stats.RelicChoices)
                {
                    var id = choice.choice?.ToString();
                    if (TelemetryClient.IsSafeModelId(id))
                        relicChoices.Add(new RunModelChoice(floor, id!, choice.wasPicked));
                }
            }
        }

        var deck = player.Deck
            .Select(card => (Id: card.Id?.ToString(), Upgrade: card.CurrentUpgradeLevel))
            .Where(card => TelemetryClient.IsSafeModelId(card.Id))
            .GroupBy(card => card)
            .OrderBy(group => group.Key.Id, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Upgrade)
            .Select(group => new RunDeckCard(group.Key.Id!, group.Key.Upgrade, group.Count()))
            .Take(MaxDeckEntries)
            .ToArray();

        var playerIndex = history.Players.IndexOf(player);
        return new RunReport(
            SchemaVersion: 1,
            ReportId: TelemetryClient.StableReportId(string.Join("|",
                "run", history.Seed, history.StartTime.ToString(CultureInfo.InvariantCulture),
                playerIndex.ToString(CultureInfo.InvariantCulture))),
            ModVersion: TelemetryClient.ModVersion,
            GameVersion: TelemetryClient.SafeToken(history.BuildId),
            Language: TelemetryClient.Language,
            GameMode: TelemetryClient.SafeToken(history.GameMode.ToString()),
            ModifierCount: Math.Min(history.Modifiers.Count, 100),
            Ascension: Math.Clamp(history.Ascension, 0, 20),
            PlayerCount: Math.Clamp(history.Players.Count, 1, 16),
            Character: player.Character.ToString(),
            PartyCharacters: history.Players
                .Where(p => !ReferenceEquals(p, player))
                .Select(p => p.Character.ToString())
                .Where(TelemetryClient.IsSafeModelId)
                .Take(MaxParty)
                .ToArray(),
            Outcome: history.Win ? "win" : history.WasAbandoned ? "abandoned" : "loss",
            FloorsVisited: Math.Min(floor, 1000),
            ActsVisited: Math.Min(history.MapPointHistory.Count, 16),
            KilledBy: KilledBy(history),
            RunTimeSeconds: float.IsFinite(history.RunTime) && history.RunTime is >= 0 and <= 31536000
                ? (int)history.RunTime
                : null,
            FinalHp: lastStats == null ? null : Math.Max(lastStats.CurrentHp, 0),
            FinalMaxHp: lastStats == null ? null : Math.Max(lastStats.MaxHp, 0),
            HasOtherGameplayMods: HasOtherGameplayMods(),
            Deck: deck,
            Relics: player.Relics
                .Select(relic => relic.Id?.ToString())
                .Where(TelemetryClient.IsSafeModelId)
                .OfType<string>()
                .Take(MaxRelics)
                .ToArray(),
            CardChoices: cardChoices.Take(MaxChoices).ToArray(),
            RelicChoices: relicChoices.Take(MaxChoices).ToArray());
    }

    private static RunHistoryPlayer? FindLocalPlayer(RunHistory history)
    {
        var localId = LocalContext.NetId;
        if (localId.HasValue)
            return history.Players.FirstOrDefault(p => p.Id == localId.Value);
        return history.Players.Count == 1 ? history.Players[0] : null;
    }

    private static string? KilledBy(RunHistory history)
    {
        var none = ModelId.none.ToString();
        foreach (var id in new[] { history.KilledByEncounter?.ToString(), history.KilledByEvent?.ToString() })
        {
            if (id != null && id != none && TelemetryClient.IsSafeModelId(id))
                return id;
        }

        return null;
    }

    private static bool HasOtherGameplayMods() =>
        ModManager.Mods.Any(mod =>
            mod.state == ModLoadState.Loaded
            && mod.manifest is { affectsGameplay: true } manifest
            && manifest.id != MainFile.ModId
            && manifest.id != "BaseLib");
}
