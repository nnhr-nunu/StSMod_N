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

        // どれか1つが例外を投げても Harmony パッチ適用まで届くよう、個別に保護する
        // （ここで落ちると ApplyHarmonyPatches が走らず、mod の挙動が全滅する）。
        SafeInit(nameof(ModConfigRegistry), () => ModConfigRegistry.Register(ModId, new HypnosisCreatorConfig()));
        SafeInit(nameof(VisualTuner), VisualTuner.ApplyAll);
        SafeInit(nameof(FetishCardText), FetishCardText.Register);
        SafeInit(nameof(CountCardText), CountCardText.Register);
        SafeInit(nameof(FetishDoomPreview), FetishDoomPreview.Register);
        SafeInit(nameof(UpgradeDescriptionHooks), UpgradeDescriptionHooks.Register);
        SafeInit(nameof(KeywordLineLayout), KeywordLineLayout.Register);
        SafeInit(nameof(CognitiveCharacterChoiceText), CognitiveCharacterChoiceText.Register);
        SafeInit(nameof(CombatSuffixDescriptionHooks), CombatSuffixDescriptionHooks.Register);
        SafeInit(nameof(Sensitivity3000Description), Sensitivity3000Description.Register);

        ApplyHarmonyPatches();
        SafeInit(nameof(ModUpdateGuard), ModUpdateGuard.Start);
        SafeInit(nameof(HeartRelicUi), HeartRelicUi.Start);
        // INetAction 登録は ModManager.Initialize 完了後（ActionTypesInitPatch）。
    }

    private static void SafeInit(string name, Action init)
    {
        try
        {
            init();
        }
        catch (Exception ex)
        {
            Logger.Warn($"{name} initialization failed: {ex}");
        }
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

        var getMePatches = harmony.GetPatchedMethods()
            .Count(m => m.DeclaringType?.FullName == "MegaCrit.Sts2.Core.Context.LocalContext" && m.Name == "GetMe");
        if (getMePatches == 0)
            Logger.Warn("EnemyAttackIntentPerspectivePatch: LocalContext.GetMe was not patched (mirroring MP desync risk).");
        else
            Logger.Info($"EnemyAttackIntentPerspectivePatch: patched {getMePatches} LocalContext.GetMe overload(s).");
    }
}
