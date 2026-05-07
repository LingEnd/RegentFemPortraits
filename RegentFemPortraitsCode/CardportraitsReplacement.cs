using System;
using System.Collections.Generic;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace RegentFemPortraits.RegentFemPortraitsCode;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class CardPortraitReplacementPatch
{
    private const string ModPortraitRoot = "res://RegentFemPortraits/card_portraits/";

    private static readonly Dictionary<string, Texture2D> ReplacementTextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<ulong> ReplacementTextureIds = new();

    static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        string cardTypeName = __instance.GetType().Name;

        try
        {
            bool hasReplacement = false;

            if (CardReplacementConfig.IsAncientCard(cardTypeName))
            {
                if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
                {
                    MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Ancient card with ancient style enabled, looking for ancient portrait");
                    if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? ancientPath))
                    {
                        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Found ancient path: {ancientPath}");
                        Texture2D? portrait = LoadReplacementTexture(ancientPath, cardTypeName, "ancient");
                        if (portrait != null)
                        {
                            __result = portrait;
                            hasReplacement = true;
                            MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Successfully loaded ancient portrait texture");
                        }
                        else
                        {
                            MainFile.Logger.Warn($"[CardPortrait] {cardTypeName}: Failed to load ancient portrait texture from path: {ancientPath}");
                        }
                    }
                    else
                    {
                        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: No ancient portrait configured");
                    }
                }
                else
                {
                    MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Ancient card with fallback style, looking for fallback portrait");
                    if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? fallbackPath))
                    {
                        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Found fallback path: {fallbackPath}");
                        Texture2D? portrait = LoadReplacementTexture(fallbackPath, cardTypeName, "fallback");
                        if (portrait != null)
                        {
                            __result = portrait;
                            hasReplacement = true;
                            MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Successfully loaded fallback portrait texture");
                        }
                        else
                        {
                            MainFile.Logger.Warn($"[CardPortrait] {cardTypeName}: Failed to load fallback portrait texture from path: {fallbackPath}");
                        }
                    }
                    else
                    {
                        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: No fallback portrait configured");
                    }
                }
            }

            if (!hasReplacement && CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath))
            {
                MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: No ancient/fallback replacement, looking for normal portrait");
                MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Found normal path: {normalPath}");
                Texture2D? portrait = LoadReplacementTexture(normalPath, cardTypeName, "normal");
                if (portrait != null)
                {
                    __result = portrait;
                    MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: Successfully loaded normal portrait texture");
                }
                else
                {
                    MainFile.Logger.Warn($"[CardPortrait] {cardTypeName}: Failed to load normal portrait texture from path: {normalPath}");
                }
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[CardPortrait] {cardTypeName}: Unexpected error in portrait replacement: {ex.Message}");
            MainFile.Logger.Error($"[CardPortrait] {cardTypeName}: Stack trace: {ex.StackTrace}");
        }
    }

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

    public static bool IsModPortraitModel(CardModel? model)
    {
        return IsModPortraitTexture(model?.Portrait);
    }

    private static Texture2D? LoadReplacementTexture(string? path, string cardTypeName, string portraitType)
    {
        if (string.IsNullOrEmpty(path))
        {
            MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: {portraitType} path is null or empty, skipping");
            return null;
        }

        if (ReplacementTextureCache.TryGetValue(path, out Texture2D? cached))
        {
            MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: {portraitType} texture cache HIT for path: {path}");
            return cached;
        }

        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: {portraitType} texture cache MISS, attempting to load: {path}");

        if (!ResourceLoader.Exists(path))
        {
            MainFile.Logger.Warn($"[CardPortrait] {cardTypeName}: {portraitType} resource does not exist at path: {path}");
            return null;
        }

        Texture2D? sourceTexture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (sourceTexture == null)
        {
            MainFile.Logger.Error($"[CardPortrait] {cardTypeName}: {portraitType} failed to load texture from path: {path}");
            return null;
        }

        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: {portraitType} texture loaded successfully, size: {sourceTexture.GetWidth()}x{sourceTexture.GetHeight()}");

        AtlasTexture pseudoAtlas = new AtlasTexture
        {
            Atlas = sourceTexture,
            Region = new Rect2(0, 0, sourceTexture.GetWidth(), sourceTexture.GetHeight())
        };

        ReplacementTextureCache[path] = pseudoAtlas;
        ReplacementTextureIds.Add(pseudoAtlas.GetInstanceId());

        MainFile.Logger.Debug($"[CardPortrait] {cardTypeName}: {portraitType} texture cached with ID: {pseudoAtlas.GetInstanceId()}");

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

[HarmonyPatch(typeof(CardModel), "PortraitPath", MethodType.Getter)]
public static class CardPortraitPathReplacementPatch
{
    static void Postfix(CardModel __instance, ref string __result)
    {
        string cardTypeName = __instance.GetType().Name;

        try
        {
            if (CardReplacementConfig.IsAncientCard(cardTypeName))
            {
                if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
                {
                    if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? path) && path != null)
                    {
                        MainFile.Logger.Debug($"[CardPortraitPath] {cardTypeName}: Ancient style enabled, replacing path with: {path}");
                        __result = path;
                        return;
                    }
                }
                else
                {
                    if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? path) && path != null)
                    {
                        MainFile.Logger.Debug($"[CardPortraitPath] {cardTypeName}: Fallback style, replacing path with: {path}");
                        __result = path;
                        return;
                    }
                }
            }
            else if (CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath) && normalPath != null)
            {
                MainFile.Logger.Debug($"[CardPortraitPath] {cardTypeName}: Normal replacement, replacing path with: {normalPath}");
                __result = normalPath;
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[CardPortraitPath] {cardTypeName}: Unexpected error in path replacement: {ex.Message}");
            MainFile.Logger.Error($"[CardPortraitPath] {cardTypeName}: Stack trace: {ex.StackTrace}");
        }
    }
}
