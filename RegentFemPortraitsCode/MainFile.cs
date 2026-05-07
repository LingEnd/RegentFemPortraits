using System;
using System.Reflection;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

using LogType = MegaCrit.Sts2.Core.Logging.LogType;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace RegentFemPortraits.RegentFemPortraitsCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "RegentFemPortraits";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        string platform = OS.GetName();
        Logger.Info($"Initializing mod on platform: {platform}");

        Harmony harmony = new(ModId);

        try
        {
            if (platform == "Linux")
            {
                Logger.Info("Linux platform detected - applying fault-tolerant patching strategy");
                ApplyFaultTolerantPatches(harmony);
            }
            else
            {
                Logger.Info($"Non-Linux platform ({platform}) detected - applying full patching");
                ApplyFullPatches(harmony);
            }

            Logger.Info("All patches applied (some may have been skipped due to compatibility issues");
        }
        catch (Exception ex)
        {
            Logger.Error($"Critical failure in patch application: {ex.Message}");
            Logger.Error($"Stack trace: {ex.StackTrace}");
        }

        if (BaseLibIntegration.IsBaseLibLoaded)
        {
            Logger.Info("BaseLib detected, registering config");
            try
            {
                BaseLibIntegration.RegisterBaseLibConfig();
                Logger.Info("BaseLib config registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to register BaseLib config: {ex.Message}");
            }
        }
        else
        {
            Logger.Info("BaseLib not detected, skipping BaseLib integration");
        }

        Logger.Info("Mod initialization completed");
    }

    private static void ApplyFullPatches(Harmony harmony)
    {
        Logger.Info("Starting full patch application (all patches)");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Logger.Info("All patches registered successfully");
    }

    private static void ApplyFaultTolerantPatches(Harmony harmony)
    {
        Logger.Info("Starting fault-tolerant patch application");

        Assembly assembly = Assembly.GetExecutingAssembly();
        int registeredCount = 0;
        int skippedCount = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (type.Namespace != "RegentFemPortraits.RegentFemPortraitsCode")
            {
                continue;
            }

            object[] harmonyPatchAttributes = type.GetCustomAttributes(typeof(HarmonyPatch), false);
            if (harmonyPatchAttributes.Length == 0)
            {
                continue;
            }

            string patchName = type.Name;

            try
            {
                Logger.Info($"Attempting to register patch: {patchName}");
                harmony.CreateClassProcessor(type).Patch();
                Logger.Info($"Successfully registered patch: {patchName}");
                registeredCount++;
            }
            catch (Exception ex)
            {
                Logger.Info($"SKIPPING patch: {patchName} (failed to patch)");
                Logger.Debug($"Patch failure details: {ex.Message}");
                skippedCount++;
            }
        }

        Logger.Info($"Patch application complete - Registered: {registeredCount}, Skipped: {skippedCount}");
    }
}
