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

    private const string PatchCardPortraitReplacement = "CardPortraitReplacementPatch";
    private const string PatchCardPortraitPathReplacement = "CardPortraitPathReplacementPatch";
    private const string PatchAncientCardStyle = "AncientCardStylePatch";

    public static void Initialize()
    {
        string platform = OS.GetName();
        Logger.Info($"[RegentFemPortraits] Initializing mod on platform: {platform}");

        Harmony harmony = new(ModId);

        try
        {
            if (platform == "Linux")
            {
                Logger.Info("[RegentFemPortraits] Linux platform detected - applying selective patching strategy");
                ApplyLinuxCompatiblePatches(harmony);
            }
            else
            {
                Logger.Info($"[RegentFemPortraits] Non-Linux platform ({platform}) detected - applying full patching");
                ApplyFullPatches(harmony);
            }

            Logger.Info("[RegentFemPortraits] All patches applied successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"[RegentFemPortraits] Failed to apply patches: {ex.Message}");
            Logger.Error($"[RegentFemPortraits] Stack trace: {ex.StackTrace}");
        }

        if (BaseLibIntegration.IsBaseLibLoaded)
        {
            Logger.Info("[RegentFemPortraits] BaseLib detected, registering config");
            try
            {
                BaseLibIntegration.RegisterBaseLibConfig();
                Logger.Info("[RegentFemPortraits] BaseLib config registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"[RegentFemPortraits] Failed to register BaseLib config: {ex.Message}");
            }
        }
        else
        {
            Logger.Info("[RegentFemPortraits] BaseLib not detected, skipping BaseLib integration");
        }

        Logger.Info("[RegentFemPortraits] Mod initialization completed");
    }

    private static void ApplyFullPatches(Harmony harmony)
    {
        Logger.Info("[RegentFemPortraits] Starting full patch application (all patches)");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Logger.Info($"[RegentFemPortraits] Successfully registered all patches: {PatchCardPortraitReplacement}, {PatchCardPortraitPathReplacement}, {PatchAncientCardStyle}");
    }

    private static void ApplyLinuxCompatiblePatches(Harmony harmony)
    {
        Logger.Info("[RegentFemPortraits] Starting Linux-compatible patch application");

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

            bool shouldSkip = false;
            string patchName = type.Name;

            if (type == typeof(CardPortraitReplacementPatch))
            {
                patchName = PatchCardPortraitReplacement;
            }
            else if (type == typeof(CardPortraitPathReplacementPatch))
            {
                patchName = PatchCardPortraitPathReplacement;
            }
            else if (type == typeof(AncientCardStylePatch))
            {
                patchName = PatchAncientCardStyle;
                shouldSkip = true;
            }

            if (shouldSkip)
            {
                Logger.Info($"[RegentFemPortraits] SKIPPING patch: {patchName} (not compatible with Linux Mono runtime)");
                skippedCount++;
                continue;
            }

            try
            {
                harmony.CreateClassProcessor(type).Patch();
                Logger.Info($"[RegentFemPortraits] Registered patch: {patchName}");
                registeredCount++;
            }
            catch (Exception ex)
            {
                Logger.Error($"[RegentFemPortraits] Failed to register patch {patchName}: {ex.Message}");
            }
        }

        Logger.Info($"[RegentFemPortraits] Patch application complete - Registered: {registeredCount}, Skipped: {skippedCount}");
    }
}
