using System.Reflection;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 休憩所設定画面向けに、席 0〜3 へキャラを配置したマルチレイアウトをシミュレートする。
/// </summary>
public static class RestSiteLayoutSimulator
{
    public const string PreviewMeta = "hc_rest_sim_preview";
    public const string PreviewSeatMeta = "hc_rest_sim_seat";

    private static readonly FieldInfo CharacterContainersField =
        AccessTools.Field(typeof(NRestSiteRoom), "_characterContainers")!;

    private static readonly FieldInfo CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")!;

    private static readonly HashSet<NRestSiteCharacter> HiddenReals = new();

    public static bool IsSimulating() => HypnosisCreatorConfig.RestSiteSimulateMultiLayout > 0.5;

    public static int SimOccupiedSeatCount()
    {
        var count = 0;
        for (var i = 0; i < RestSiteSeatStore.SeatCount; i++)
        {
            if (GetSimCharacterForSeat(i) != RestSiteSimCharacter.Empty)
                count++;
        }

        return count;
    }

    public static RestSiteSimCharacter GetSimCharacterForSeat(int seat) =>
        RestSiteSimCharacterStore.Clamp((int)GetSimSeatConfig(seat));

    public static void Refresh()
    {
        if (!IsSimulating())
        {
            Clear();
            return;
        }

        var room = NRestSiteRoom.Instance;
        if (room == null)
            return;

        ClearPreviews();
        RestoreRealVisibility();

        var containers = (List<Control>)CharacterContainersField.GetValue(room)!;
        for (var seat = 0; seat < RestSiteSeatStore.SeatCount; seat++)
        {
            var simChar = GetSimCharacterForSeat(seat);
            var real = FindRealCharacterAtIndex(room, seat);

            if (simChar == RestSiteSimCharacter.Empty)
            {
                HideReal(real);
                continue;
            }

            if (real != null && SimMatchesReal(simChar, real))
            {
                real.Visible = true;
                if (simChar == RestSiteSimCharacter.HypnosisCreator)
                    RestSiteSeatTuning.Apply(real);
                continue;
            }

            HideReal(real);
            SpawnPreview(containers[seat], simChar, seat);
        }
    }

    public static void Clear()
    {
        ClearPreviews();
        RestoreRealVisibility();
    }

    private static double GetSimSeatConfig(int seat) => seat switch
    {
        0 => HypnosisCreatorConfig.RestSiteSimSeat0,
        1 => HypnosisCreatorConfig.RestSiteSimSeat1,
        2 => HypnosisCreatorConfig.RestSiteSimSeat2,
        3 => HypnosisCreatorConfig.RestSiteSimSeat3,
        _ => 0
    };

    private static NRestSiteCharacter? FindRealCharacterAtIndex(NRestSiteRoom room, int seatIndex)
    {
        foreach (var character in room.characterAnims)
        {
            if (!GodotObject.IsInstanceValid(character))
                continue;

            if ((int)CharacterIndexField.GetValue(character)! == seatIndex)
                return character;
        }

        return null;
    }

    private static bool SimMatchesReal(RestSiteSimCharacter sim, NRestSiteCharacter real)
    {
        var character = real.Player?.Character;
        if (character == null)
            return false;

        return sim switch
        {
            RestSiteSimCharacter.Ironclad => character is Ironclad,
            RestSiteSimCharacter.Silent => character is Silent,
            RestSiteSimCharacter.Defect => character is Defect,
            RestSiteSimCharacter.Regent => character is Regent,
            RestSiteSimCharacter.Necrobinder => character is Necrobinder,
            RestSiteSimCharacter.HypnosisCreator => character is HcCharacter,
            _ => false
        };
    }

    private static void HideReal(NRestSiteCharacter? real)
    {
        if (real == null || !GodotObject.IsInstanceValid(real))
            return;

        real.Visible = false;
        HiddenReals.Add(real);
    }

    private static void RestoreRealVisibility()
    {
        foreach (var real in HiddenReals)
        {
            if (GodotObject.IsInstanceValid(real))
                real.Visible = true;
        }

        HiddenReals.Clear();
    }

    private static void SpawnPreview(Control container, RestSiteSimCharacter simChar, int seatIndex)
    {
        var path = GetRestSitePath(simChar);
        if (string.IsNullOrEmpty(path) || !ResourceLoader.Exists(path))
        {
            MainFile.Logger.Warn($"RestSiteLayoutSimulator: scene not found for {simChar} ({path})");
            return;
        }

        PackedScene packed;
        if (path.Contains("HypnosisCreator", StringComparison.OrdinalIgnoreCase))
        {
            packed = ResourceLoader.Load<PackedScene>(path);
        }
        else
        {
            packed = PreloadManager.Cache.GetScene(path);
        }

        var instance = packed.Instantiate(PackedScene.GenEditState.Disabled);
        instance.SetMeta(PreviewMeta, true);
        instance.SetMeta(PreviewSeatMeta, seatIndex);
        instance.ProcessMode = Node.ProcessModeEnum.Disabled;

        if (instance is CanvasItem canvasItem)
            canvasItem.Modulate = Colors.White;

        container.AddChild(instance);
        if (instance is Node2D node2D)
            node2D.Position = Vector2.Zero;

        if (instance is NRestSiteCharacter restCharacter)
        {
            CharacterIndexField.SetValue(restCharacter, seatIndex);
            if (seatIndex % 2 == 1)
                restCharacter.FlipX();
        }

        if (simChar == RestSiteSimCharacter.HypnosisCreator && instance is Node2D hcRoot)
            RestSiteSeatTuning.ApplyHypnosisCreatorPreview(hcRoot, seatIndex);
    }

    private static string GetRestSitePath(RestSiteSimCharacter simChar)
    {
        CharacterModel? model = simChar switch
        {
            RestSiteSimCharacter.Ironclad => ModelDb.Character<Ironclad>(),
            RestSiteSimCharacter.Silent => ModelDb.Character<Silent>(),
            RestSiteSimCharacter.Defect => ModelDb.Character<Defect>(),
            RestSiteSimCharacter.Regent => ModelDb.Character<Regent>(),
            RestSiteSimCharacter.Necrobinder => ModelDb.Character<Necrobinder>(),
            RestSiteSimCharacter.HypnosisCreator => ModelDb.Character<HcCharacter>(),
            _ => null
        };

        return model?.RestSiteAnimPath ?? "";
    }

    private static void ClearPreviews()
    {
        var room = NRestSiteRoom.Instance;
        if (room == null)
            return;

        var containers = (List<Control>)CharacterContainersField.GetValue(room)!;
        foreach (var container in containers)
        {
            var toRemove = new List<Node>();
            foreach (var child in container.GetChildren())
            {
                if (child.HasMeta(PreviewMeta))
                    toRemove.Add(child);
            }

            foreach (var child in toRemove)
                child.QueueFree();
        }
    }
}

/// <summary>Mod設定スライダー値を <see cref="RestSiteSimCharacter"/> に正規化する。</summary>
internal static class RestSiteSimCharacterStore
{
    public static RestSiteSimCharacter Clamp(int value) =>
        (RestSiteSimCharacter)Math.Clamp(value, 0, (int)RestSiteSimCharacter.HypnosisCreator);
}
