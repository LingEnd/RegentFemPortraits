using System;
using System.Collections.Generic;
using System.Reflection;

using Godot;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace RegentFemPortraits.RegentFemPortraitsCode;

public static class LinuxCardPatcher
{
    private const int DeferFrames = 30;
    private const int PeriodicScanIntervalFrames = 120;

    private static bool _initialized;
    private static bool _scanComplete;
    private static int _frameCount;

    private static SceneTree? _sceneTree;
    private static readonly Dictionary<string, Texture2D> TextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<ulong> PatchedCardInstanceIds = new();

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        MainFile.Logger.Info("[LinuxPatcher] Initializing Harmony-free card patching system");

        _sceneTree = Engine.GetMainLoop() as SceneTree;
        if (_sceneTree == null)
        {
            MainFile.Logger.Error("[LinuxPatcher] Failed to get SceneTree");
            return;
        }

        _sceneTree.ProcessFrame += OnProcessFrame;
        MainFile.Logger.Info($"[LinuxPatcher] Registered ProcessFrame handler, will activate after {DeferFrames} frames");
    }

    private static void OnProcessFrame()
    {
        _frameCount++;

        if (_frameCount == DeferFrames)
        {
            MainFile.Logger.Info($"[LinuxPatcher] Deferred startup triggered at frame {DeferFrames}");
            Activate();
        }

        if (!_scanComplete)
        {
            return;
        }

        if (_frameCount % PeriodicScanIntervalFrames == 0)
        {
            ScanAndPatchCards();
        }
    }

    private static void Activate()
    {
        MainFile.Logger.Info("[LinuxPatcher] Activating - scanning existing cards and subscribing to NodeAdded");

        if (_sceneTree == null)
        {
            return;
        }

        _sceneTree.NodeAdded += OnNodeAdded;
        ScanExistingCards(_sceneTree.Root);
        _scanComplete = true;

        MainFile.Logger.Info("[LinuxPatcher] Activation complete");
    }

    private static void ScanExistingCards(Node root)
    {
        MainFile.Logger.Info("[LinuxPatcher] Performing initial full scan for existing card nodes");

        int patchedCount = 0;
        ScanNodeRecursive(root, ref patchedCount);

        MainFile.Logger.Info($"[LinuxPatcher] Initial scan complete - patched {patchedCount} existing card nodes");
    }

    private static void ScanNodeRecursive(Node node, ref int count)
    {
        if (node is NCard nCard)
        {
            if (TryPatchCard(nCard))
            {
                count++;
            }
        }
        else if (node is NTinyCard nTinyCard)
        {
            if (TryPatchTinyCard(nTinyCard))
            {
                count++;
            }
        }

        foreach (Node child in node.GetChildren())
        {
            ScanNodeRecursive(child, ref count);
        }
    }

    private static void ScanAndPatchCards()
    {
        if (_sceneTree?.Root == null)
        {
            return;
        }

        int patchedCount = 0;
        ScanNodeRecursive(_sceneTree.Root, ref patchedCount);

        if (patchedCount > 0)
        {
            MainFile.Logger.Debug($"[LinuxPatcher] Periodic scan found {patchedCount} new/unpatched cards");
        }
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is NCard nCard)
        {
            TryPatchCardDeferred(nCard);
        }
        else if (node is NTinyCard nTinyCard)
        {
            TryPatchTinyCardDeferred(nTinyCard);
        }
    }

    private static void TryPatchCardDeferred(NCard card)
    {
        ulong nodeId = card.GetInstanceId();
        if (_sceneTree != null)
        {
            SceneTreeTimer timer = _sceneTree.CreateTimer(0.1f);
            timer.Timeout += () =>
            {
                if (GodotObject.IsInstanceIdValid(nodeId))
                {
                    TryPatchCard(card);
                }
            };
        }
    }

    private static void TryPatchTinyCardDeferred(NTinyCard card)
    {
        ulong nodeId = card.GetInstanceId();
        if (_sceneTree != null)
        {
            SceneTreeTimer timer = _sceneTree.CreateTimer(0.1f);
            timer.Timeout += () =>
            {
                if (GodotObject.IsInstanceIdValid(nodeId))
                {
                    TryPatchTinyCard(card);
                }
            };
        }
    }

    private static bool TryPatchCard(NCard card)
    {
        try
        {
            if (PatchedCardInstanceIds.Contains(card.GetInstanceId()))
            {
                return false;
            }

            CardModel? model = card.Model;
            if (model == null)
            {
                return false;
            }

            string cardTypeName = model.GetType().Name;

            if (!TryGetReplacementPath(cardTypeName, out string? replacementPath) || replacementPath == null)
            {
                return false;
            }

            Texture2D? replacementTexture = LoadTexture(replacementPath, cardTypeName);
            if (replacementTexture == null)
            {
                return false;
            }

            // Override the model's internal portrait field via reflection
            // This ensures even Reload() picks up our replacement
            bool reflectionSuccess = OverrideModelPortrait(model, replacementTexture);

            // Directly set TextureRect nodes for immediate visual effect
            SetNodeTexture(card, "%Portrait", replacementTexture);
            SetNodeTexture(card, "%AncientPortrait", replacementTexture);

            // Apply smooth texture filtering
            ApplySmoothFilter(card);

            PatchedCardInstanceIds.Add(card.GetInstanceId());
            MainFile.Logger.Info($"[LinuxPatcher] Patched card: {cardTypeName} -> {replacementPath} (reflection: {reflectionSuccess})");

            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[LinuxPatcher] Error patching card: {ex.Message}");
            return false;
        }
    }

    private static bool TryPatchTinyCard(NTinyCard card)
    {
        try
        {
            if (PatchedCardInstanceIds.Contains(card.GetInstanceId()))
            {
                return false;
            }

            CardModel? model = GetCardModelFromNode(card);
            if (model == null)
            {
                return false;
            }

            string cardTypeName = model.GetType().Name;

            if (!TryGetReplacementPath(cardTypeName, out string? replacementPath) || replacementPath == null)
            {
                return false;
            }

            Texture2D? replacementTexture = LoadTexture(replacementPath, cardTypeName);
            if (replacementTexture == null)
            {
                return false;
            }

            bool reflectionSuccess = OverrideModelPortrait(model, replacementTexture);

            TextureRect? portrait = card.GetNodeOrNull<TextureRect>("%Portrait");
            if (portrait != null)
            {
                portrait.Texture = replacementTexture;
            }

            PatchedCardInstanceIds.Add(card.GetInstanceId());
            MainFile.Logger.Info($"[LinuxPatcher] Patched tiny card: {cardTypeName} (reflection: {reflectionSuccess})");

            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[LinuxPatcher] Error patching tiny card: {ex.Message}");
            return false;
        }
    }

    private static CardModel? GetCardModelFromNode(Node node)
    {
        try
        {
            PropertyInfo? modelProp = node.GetType().GetProperty("Model",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (modelProp != null)
            {
                return modelProp.GetValue(node) as CardModel;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool OverrideModelPortrait(CardModel model, Texture2D replacement)
    {
        try
        {
            Type modelType = model.GetType();

            // Try auto-property backing field: <Portrait>k__BackingField
            FieldInfo? field = modelType.GetField("<Portrait>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null && typeof(Texture2D).IsAssignableFrom(field.FieldType))
            {
                field.SetValue(model, replacement);
                MainFile.Logger.Debug("[LinuxPatcher] Set <Portrait>k__BackingField via reflection");
                return true;
            }

            // Try common naming conventions
            foreach (string fieldName in new[] { "_portrait", "portrait", "m_portrait", "_cachedPortrait" })
            {
                field = modelType.GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (field != null && typeof(Texture2D).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(model, replacement);
                    MainFile.Logger.Debug($"[LinuxPatcher] Set {fieldName} via reflection");
                    return true;
                }
            }

            MainFile.Logger.Warn($"[LinuxPatcher] Could not find Portrait backing field on CardModel type: {modelType.FullName}");
            return false;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[LinuxPatcher] Failed to override model portrait via reflection: {ex.Message}");
            return false;
        }
    }

    private static void SetNodeTexture(Node parent, string nodePath, Texture2D texture)
    {
        TextureRect? rect = parent.GetNodeOrNull<TextureRect>(nodePath);
        if (rect != null)
        {
            rect.Texture = texture;
        }
    }

    private static void ApplySmoothFilter(Node node)
    {
        foreach (string nodePath in new[] { "%Portrait", "%AncientPortrait" })
        {
            CanvasItem? item = node.GetNodeOrNull<CanvasItem>(nodePath);
            if (item != null)
            {
                item.TextureFilter = (CanvasItem.TextureFilterEnum)5;
            }
        }

        CanvasItem? canvasGroup = node.GetNodeOrNull<CanvasItem>("%PortraitCanvasGroup");
        if (canvasGroup != null)
        {
            ApplyFilterRecursive(canvasGroup);
        }
    }

    private static void ApplyFilterRecursive(Node node)
    {
        if (node is CanvasItem canvasItem)
        {
            canvasItem.TextureFilter = (CanvasItem.TextureFilterEnum)5;
        }

        foreach (Node child in node.GetChildren())
        {
            ApplyFilterRecursive(child);
        }
    }

    private static bool TryGetReplacementPath(string cardTypeName, out string? path)
    {
        if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out path) && path != null)
        {
            return true;
        }

        if (CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out path) && path != null)
        {
            return true;
        }

        path = null;
        return false;
    }

    private static Texture2D? LoadTexture(string path, string cardTypeName)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (TextureCache.TryGetValue(path, out Texture2D? cached))
        {
            return cached;
        }

        if (!ResourceLoader.Exists(path))
        {
            MainFile.Logger.Warn($"[LinuxPatcher] Texture not found: {path} (for {cardTypeName})");
            return null;
        }

        Texture2D? source = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (source == null)
        {
            MainFile.Logger.Error($"[LinuxPatcher] Failed to load texture: {path} (for {cardTypeName})");
            return null;
        }

        AtlasTexture pseudoAtlas = new AtlasTexture
        {
            Atlas = source,
            Region = new Rect2(0, 0, source.GetWidth(), source.GetHeight())
        };

        TextureCache[path] = pseudoAtlas;
        MainFile.Logger.Debug($"[LinuxPatcher] Loaded and cached texture: {path} ({source.GetWidth()}x{source.GetHeight()})");

        return pseudoAtlas;
    }
}
