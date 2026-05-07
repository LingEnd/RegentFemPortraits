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
        Logger.Info($"[RegentFemPortraits] Initializing mod on platform: {platform}");

        if (platform == "Linux")
        {
            Logger.Info("[RegentFemPortraits] Linux detected - Harmony is incompatible with this Mono runtime, using alternative approach");
            InitializeLinux();
        }
        else
        {
            Logger.Info($"[RegentFemPortraits] Non-Linux platform ({platform}), using Harmony patches");
            InitializeWindows();
        }

        InitializeBaseLib();

        Logger.Info("[RegentFemPortraits] Mod initialization completed");
    }

    private static void InitializeWindows()
    {
        Harmony harmony = new(ModId);

        try
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.Info("[RegentFemPortraits] All Harmony patches registered successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"[RegentFemPortraits] Failed to apply Harmony patches: {ex.Message}");
            Logger.Error($"[RegentFemPortraits] Stack trace: {ex.StackTrace}");
        }
    }

    private static void InitializeLinux()
    {
        Logger.Info("[RegentFemPortraits] Linux: skipping all Harmony patches (MonoMod dynamic code gen incompatible)");
        Logger.Info("[RegentFemPortraits] Linux: activating SceneTree-based card interception instead");

        try
        {
            LinuxCardPatcher.Initialize();
            Logger.Info("[RegentFemPortraits] Linux: SceneTree patcher initialized, deferred startup scheduled");
        }
        catch (Exception ex)
        {
            Logger.Error($"[RegentFemPortraits] Linux: failed to initialize scene tree patcher: {ex.Message}");
            Logger.Error($"[RegentFemPortraits] Stack trace: {ex.StackTrace}");
        }
    }

    private static void InitializeBaseLib()
    {
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
    }
}
