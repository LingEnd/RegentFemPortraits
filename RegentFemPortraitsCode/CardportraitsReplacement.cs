using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace RegentFemPortraits.RegentFemPortraitsCode;

/// <summary>
/// 拦截 <see cref="CardModel.Portrait"/> 的读取过程，将指定卡牌的原始立绘替换为模组内的二次元卡图。
/// 这里还会优先生成并缓存带有 Mipmap 的纹理，为预览卡片时的平滑显示提供更合适的资源数据，
/// 从而配合 <c>CardFilterUtility.cs</c> 一起减轻卡牌在缩放或放大预览时出现的边缘锯齿。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class CardPortraitReplacementPatch
{
    private const string ModPortraitRoot = "res://RegentFemPortraits/card_portraits/";

    private static readonly Dictionary<Type, string> Replacements = new()
    {
        // 储君基础牌
        { typeof(DefendRegent), "res://RegentFemPortraits/card_portraits/regent/DefendRegent.png" }, // 防御
        { typeof(Venerate), "res://RegentFemPortraits/card_portraits/regent/Venerate.png" }, // 崇拜
        { typeof(FallingStar), "res://RegentFemPortraits/card_portraits/regent/FallingStar.png" }, // 陨星

        // 储君普通牌
        { typeof(Begone), "res://RegentFemPortraits/card_portraits/regent/Begone.png" }, // 下去！
        { typeof(Charge), "res://RegentFemPortraits/card_portraits/regent/Charge.png" }, // 冲锋！！
        { typeof(ChildOfTheStars), "res://RegentFemPortraits/card_portraits/regent/ChildOfTheStars.png" }, // 群星之子
        { typeof(CloakOfStars), "res://RegentFemPortraits/card_portraits/regent/CloakOfStars.png" }, // 群星斗篷
        { typeof(Conqueror), "res://RegentFemPortraits/card_portraits/regent/Conqueror.png" }, // 征服者
        { typeof(Convergence), "res://RegentFemPortraits/card_portraits/regent/Convergence.png" }, // 汇流
        { typeof(CosmicIndifference), "res://RegentFemPortraits/card_portraits/regent/CosmicIndifference.png" }, // 宇宙冷漠
        { typeof(DecisionsDecisions), "res://RegentFemPortraits/card_portraits/regent/DecisionsDecisions.png" }, // 抉择，抉择
        { typeof(Glow), "res://RegentFemPortraits/card_portraits/regent/Glow.png" }, // 辉光
        { typeof(HeavenlyDrill), "res://RegentFemPortraits/card_portraits/regent/HeavenlyDrill.png" }, // 天际钻头
        { typeof(KinglyKick), "res://RegentFemPortraits/card_portraits/regent/KinglyKick.png" }, // 王者之踢
        { typeof(KinglyPunch), "res://RegentFemPortraits/card_portraits/regent/KinglyPunch.png" }, // 王者之拳
        { typeof(MakeItSo), "res://RegentFemPortraits/card_portraits/regent/MakeItSo.png" }, // 如此甚好
        { typeof(MonarchsGaze), "res://RegentFemPortraits/card_portraits/regent/MonarchsGaze.png" }, // 王之凝视
        { typeof(NeutronAegis), "res://RegentFemPortraits/card_portraits/regent/NeutronAegis.png" }, // 中子护盾
        { typeof(Patter), "res://RegentFemPortraits/card_portraits/regent/Patter.png" }, // 星星点点
        { typeof(Parry), "res://RegentFemPortraits/card_portraits/regent/Parry.png" }, // 招架
        { typeof(Prophesize), "res://RegentFemPortraits/card_portraits/regent/Prophesize.png" }, // 预言
        { typeof(Quasar), "res://RegentFemPortraits/card_portraits/regent/Quasar.png" }, // 类星体
        { typeof(Radiate), "res://RegentFemPortraits/card_portraits/regent/Radiate.png" }, // 辐射
        { typeof(RefineBlade), "res://RegentFemPortraits/card_portraits/regent/RefineBlade.png" }, // 淬炼刀刃
        { typeof(SwordSage), "res://RegentFemPortraits/card_portraits/regent/SwordSage.png" }, // 剑圣
        { typeof(TheSmith), "res://RegentFemPortraits/card_portraits/regent/TheSmith.png" }, // 铸剑者
        { typeof(Tyranny), "res://RegentFemPortraits/card_portraits/regent/Tyranny.png" }, // 暴政
        { typeof(VoidForm), "res://RegentFemPortraits/card_portraits/regent/VoidForm.png" }, // 虚空形态
        { typeof(WroughtInWar), "res://RegentFemPortraits/card_portraits/regent/WroughtInWar.png" }, // 战火铸就
        { typeof(GammaBlast), "res://RegentFemPortraits/card_portraits/regent/GammaBlast.png" }, // 伽马爆破
        { typeof(Glitterstream), "res://RegentFemPortraits/card_portraits/regent/Glitterstream.png" }, // 流光溢彩
        { typeof(HeirloomHammer), "res://RegentFemPortraits/card_portraits/regent/HeirloomHammer.png" }, // 创世之柱
        { typeof(PillarOfCreation), "res://RegentFemPortraits/card_portraits/regent/PillarOfCreation.png" }, // 传承之锤
        { typeof(Monologue), "res://RegentFemPortraits/card_portraits/regent/Monologue.png" }, // 独白
        { typeof(Arsenal), "res://RegentFemPortraits/card_portraits/regent/Arsenal.png" }, // 武器库
        { typeof(BeatIntoShape), "res://RegentFemPortraits/card_portraits/regent/BeatIntoShape.png" }, // 锻打成型
        { typeof(BigBang), "res://RegentFemPortraits/card_portraits/regent/BigBang.png" }, // 大爆炸
        { typeof(BundleOfJoy), "res://RegentFemPortraits/card_portraits/regent/BundleOfJoy.png" }, // 新生之喜
        { typeof(CelestialMight), "res://RegentFemPortraits/card_portraits/regent/CelestialMight.png" }, // 天穹之力
        { typeof(GatherLight), "res://RegentFemPortraits/card_portraits/regent/GatherLight.png" }, // 收集光辉
        { typeof(KnockoutBlow), "res://RegentFemPortraits/card_portraits/regent/KnockoutBlow.png" }, // 决胜一击
        { typeof(ManifestAuthority), "res://RegentFemPortraits/card_portraits/regent/ManifestAuthority.png" }, // 君权自授
        { typeof(RoyalGamble), "res://RegentFemPortraits/card_portraits/regent/RoyalGamble.png" }, // 胜券在王
        { typeof(ShiningStrike), "res://RegentFemPortraits/card_portraits/regent/ShiningStrike.png" }, // 明耀打击
        { typeof(Terraforming), "res://RegentFemPortraits/card_portraits/regent/Terraforming.png" }, // 地形改造
        { typeof(Stardust), "res://RegentFemPortraits/card_portraits/regent/Stardust.png" }, // 星尘

        // 棺者普通牌
        { typeof(Dirge), "res://RegentFemPortraits/card_portraits/regent/Dirge.png" }, // 挽歌
        { typeof(Transfigure), "res://RegentFemPortraits/card_portraits/regent/Transfigure.png" }, // 重构
        { typeof(Fear), "res://RegentFemPortraits/card_portraits/regent/Fear.png" }, // 恐惧
        { typeof(SoulStorm), "res://RegentFemPortraits/card_portraits/regent/SoulStorm.png" }, // 灵魂风暴
        { typeof(Reap), "res://RegentFemPortraits/card_portraits/regent/Reap.png" }, // 收割
    };

    // 缓存已经生成过 Mipmap 的替换贴图，避免每次读取卡图时重复解码和重复生成。
    private static readonly Dictionary<string, Texture2D> ReplacementTextureCache = new(StringComparer.OrdinalIgnoreCase);

    // 记录本模组运行时创建出的贴图实例，供过滤补丁快速判断当前纹理是否来自本模组。
    private static readonly HashSet<ulong> ReplacementTextureIds = new();

    /// <summary>
    /// 在游戏请求卡牌立绘时，将其替换为本模组对应的图片资源。
    /// </summary>
    static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        if (!TryGetReplacementPath(__instance, out string path))
            return;

        Texture2D? portrait = LoadReplacementTexture(path);
        if (portrait != null)
        {
            __result = portrait;
        }
        else
        {
            MainFile.Logger.Error($"Failed to load replacement portrait for {__instance.GetType().Name} at {path}");
        }
    }

    /// <summary>
    /// 判断某张纹理是否属于本模组的替换卡图。
    /// </summary>
    public static bool IsModPortraitTexture(Texture2D? texture)
    {
        if (texture == null)
        {
            return false;
        }

        if (ReplacementTextureIds.Contains(texture.GetInstanceId()))
        {
            return true;
        }

        string resourcePath = texture.ResourcePath ?? string.Empty;
        return resourcePath.StartsWith(ModPortraitRoot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断某张卡牌当前使用的立绘是否来自本模组。
    /// </summary>
    public static bool IsModPortraitModel(CardModel? model)
    {
        return IsModPortraitTexture(model?.Portrait);
    }

    /// <summary>
    /// 加载并缓存替换立绘。
    /// 直接使用原始纹理以保留完整的 Mipmap 链，避免 AtlasTexture 包装导致 Mipmap 映射错误。
    /// </summary>
    private static Texture2D? LoadReplacementTexture(string path)
    {
        if (ReplacementTextureCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        Texture2D sourceTexture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (sourceTexture == null)
        {
            return null;
        }

        int width = sourceTexture.GetWidth();
        int height = sourceTexture.GetHeight();
        
        MainFile.Logger.Info($"[Portrait] Loaded texture: {path}, Size: {width}x{height}");
        
        // 检查纹理尺寸是否是 2 的幂次方
        if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
        {
            MainFile.Logger.Warn($"[Portrait] WARNING: Texture size {width}x{height} is not power of two! Mipmap quality may be degraded. Consider using 256x256, 512x512, or 1024x1024.");
        }

        ReplacementTextureCache[path] = sourceTexture;
        ReplacementTextureIds.Add(sourceTexture.GetInstanceId());
        return sourceTexture;
    }
    
    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    /// <summary>
    /// 根据卡牌运行时类型查找对应的替换图片路径。
    /// </summary>
    internal static bool TryGetReplacementPath(CardModel model, out string path)
    {
        if (Replacements.TryGetValue(model.GetType(), out string? value) && !string.IsNullOrEmpty(value))
        {
            path = value;
            return true;
        }

        path = string.Empty;
        return false;
    }

    // 删除了手动的 CreateMipmappedTexture，让Godot底层原生处理Mipmap
}

/// <summary>
/// 拦截 <see cref="CardModel.PortraitPath"/> 的读取过程，将底层依赖的路径也替换为模组内的独立卡图路径。
/// 《杀戮尖塔2》内部除了直接读取 <c>Portrait</c> 纹理，部分 UI 和状态更新也可能依赖于路径进行校验或异步加载。
/// 若不替换此路径，卡图可能会被原版 UI 或 Shader 冲刷回游戏最初自带的源图。
/// </summary>
[HarmonyPatch(typeof(CardModel), "PortraitPath", MethodType.Getter)]
public static class CardPortraitPathReplacementPatch
{
    static void Postfix(CardModel __instance, ref string __result)
    {
        if (CardPortraitReplacementPatch.TryGetReplacementPath(__instance, out string path))
        {
            __result = path;
        }
    }
}

