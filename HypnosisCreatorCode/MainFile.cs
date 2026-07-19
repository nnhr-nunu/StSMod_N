using BaseLib.Config;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Modding;

namespace HypnosisCreator.HypnosisCreatorCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "HypnosisCreator"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        ModConfigRegistry.Register(ModId, new HypnosisCreatorConfig());
        VisualTuner.ApplyAll();
        FetishCardText.Register();
        UpgradeDescriptionHooks.Register();

        ApplyHarmonyPatches();
    }

    /// <summary>
    /// Harmony.PatchAll と同等だが、1クラス失敗で後続が全滅しない。
    /// godot.log 例: GetCurrentAnimationLength の float/double 不一致で Count パッチ未適用。
    /// </summary>
    private static void ApplyHarmonyPatches()
    {
        Harmony harmony = new(ModId);
        var ok = 0;
        var fail = 0;

        foreach (var type in AccessTools.GetTypesFromAssembly(typeof(MainFile).Assembly))
        {
            try
            {
                var patched = harmony.CreateClassProcessor(type).Patch();
                if (patched != null && patched.Any())
                    ok++;
            }
            catch (Exception ex)
            {
                fail++;
                Logger.Warn($"Harmony patch failed for {type.FullName}: {ex.Message}");
            }
        }

        Logger.Info($"Harmony patches applied: {ok} classes ok, {fail} failed");
    }
}
