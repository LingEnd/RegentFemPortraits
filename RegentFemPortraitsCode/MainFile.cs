using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using BaseLib.Config;
using System.Reflection;

namespace RegentFemPortraits.RegentFemPortraitsCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "RegentFemPortraits";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new ModConfig());

        Harmony harmony = new(ModId);

        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Logger.Info($"RegentFemPortraits has been initialized! Antialiasing: {ModConfig.GetAntialiasingDescription()}, Quality: {ModConfig.GetTextureQualityDescription()}");
    }
}
