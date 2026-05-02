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
/// 支持普通替换和先古样式替换两种模式。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class CardPortraitReplacementPatch
{
    private const string ModPortraitRoot = "res://RegentFemPortraits/card_portraits/";

    private static readonly Dictionary<string, Texture2D> ReplacementTextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<ulong> ReplacementTextureIds = new();

    static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        string cardTypeName = __instance.GetType().Name;
        bool hasReplacement = false;

        if (CardReplacementConfig.IsAncientCard(cardTypeName))
        {
            if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
            {
                if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? ancientPath))
                {
                    Texture2D? portrait = LoadReplacementTexture(ancientPath);
                    if (portrait != null)
                    {
                        __result = portrait;
                        hasReplacement = true;
                    }
                }
            }
            else
            {
                if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? fallbackPath))
                {
                    Texture2D? portrait = LoadReplacementTexture(fallbackPath);
                    if (portrait != null)
                    {
                        __result = portrait;
                        hasReplacement = true;
                    }
                }
            }
        }

        if (!hasReplacement && CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath))
        {
            Texture2D? portrait = LoadReplacementTexture(normalPath);
            if (portrait != null)
            {
                __result = portrait;
            }
        }
    }

    /// <summary>
    /// 检查纹理是否为模组卡图。
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
    /// 检查卡牌模型是否为模组卡图。
    /// </summary>
    public static bool IsModPortraitModel(CardModel? model)
    {
        return IsModPortraitTexture(model?.Portrait);
    }

    private static Texture2D? LoadReplacementTexture(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (ReplacementTextureCache.TryGetValue(path, out Texture2D? cached))
        {
            return cached;
        }

        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        Texture2D? sourceTexture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (sourceTexture == null)
        {
            return null;
        }

        AtlasTexture pseudoAtlas = new AtlasTexture
        {
            Atlas = sourceTexture,
            Region = new Rect2(0, 0, sourceTexture.GetWidth(), sourceTexture.GetHeight())
        };

        ReplacementTextureCache[path] = pseudoAtlas;
        ReplacementTextureIds.Add(pseudoAtlas.GetInstanceId());

        return pseudoAtlas;
    }

    internal static bool TryGetReplacementPath(string cardTypeName, out string? path)
    {
        if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out path))
        {
            return true;
        }

        if (CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out path))
        {
            return true;
        }

        path = string.Empty;
        return false;
    }
}

/// <summary>
/// 拦截 <see cref="CardModel.PortraitPath"/> 的读取过程，将底层依赖的路径也替换为模组内的独立卡图路径。
/// </summary>
[HarmonyPatch(typeof(CardModel), "PortraitPath", MethodType.Getter)]
public static class CardPortraitPathReplacementPatch
{
    static void Postfix(CardModel __instance, ref string __result)
    {
        string cardTypeName = __instance.GetType().Name;

        if (CardReplacementConfig.IsAncientCard(cardTypeName))
        {
            if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
            {
                if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? path) && path != null)
                {
                    __result = path;
                    return;
                }
            }
            else
            {
                if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? path) && path != null)
                {
                    __result = path;
                    return;
                }
            }
        }
        else if (CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath) && normalPath != null)
        {
            __result = normalPath;
        }
    }
}