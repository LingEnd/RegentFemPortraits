using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace RegentFemPortraits.RegentFemPortraitsCode;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class AncientCardStylePatch
{
    static void Postfix(NCard __instance)
    {
        try
        {
            if (__instance?.Model == null)
            {
                MainFile.Logger.Debug("[AncientStyle] Skipping - instance or model is null");
                return;
            }

            string cardTypeName = __instance.Model.GetType().Name;
            MainFile.Logger.Debug($"[AncientStyle] Processing card: {cardTypeName}");

            if (!CardReplacementConfig.IsAncientCard(cardTypeName))
            {
                MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Not an ancient card, skipping");
                return;
            }

            MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Is ancient card");

            if (!ConfigHelper.IsAncientStyleEnabled(cardTypeName))
            {
                MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Ancient style not enabled in config, skipping");
                return;
            }

            MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Ancient style is enabled, applying");
            ApplyAncientStyle(__instance, cardTypeName);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[AncientStyle] Unexpected error in Reload postfix: {ex.Message}");
            MainFile.Logger.Error($"[AncientStyle] Stack trace: {ex.StackTrace}");
        }
    }

    private static void ApplyAncientStyle(NCard card, string cardTypeName)
    {
        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Attempting to apply ancient style");

        TextureRect? portrait = SafeGetNode<TextureRect>(card, "%Portrait");
        TextureRect? ancientPortrait = SafeGetNode<TextureRect>(card, "%AncientPortrait");
        TextureRect? ancientTextBg = SafeGetNode<TextureRect>(card, "%AncientTextBg");
        CanvasGroup? portraitCanvasGroup = SafeGetNode<CanvasGroup>(card, "%PortraitCanvasGroup");

        if (portrait == null)
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Failed to get %Portrait node");
            return;
        }
        if (ancientPortrait == null)
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Failed to get %AncientPortrait node");
            return;
        }
        if (ancientTextBg == null)
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Failed to get %AncientTextBg node");
            return;
        }
        if (portraitCanvasGroup == null)
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Failed to get %PortraitCanvasGroup node");
            return;
        }

        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: All required nodes found successfully");

        AncientReplacementEntry? ancientConfig = CardReplacementConfig.GetAncientConfig(cardTypeName);
        if (ancientConfig == null)
        {
            MainFile.Logger.Warn($"[AncientStyle] {cardTypeName}: No ancient config found");
            return;
        }

        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Ancient config found, applying portrait texture");

        if (card.Model?.Portrait != null)
        {
            ancientPortrait.Texture = card.Model.Portrait;
            MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Portrait texture applied to ancient portrait node");
        }
        else
        {
            MainFile.Logger.Warn($"[AncientStyle] {cardTypeName}: Model portrait is null, cannot apply");
        }

        portrait.Visible = false;
        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Hidden original portrait");

        SafeSetNodeVisible(card, "%PortraitBorder", false);
        SafeSetNodeVisible(card, "%Frame", false);

        ancientPortrait.Visible = true;
        SafeSetNodeVisible(card, "%AncientBorder", true);
        ancientTextBg.Visible = true;
        SafeSetNodeVisible(card, "%AncientBanner", true);
        SafeSetNodeVisible(card, "%TitleBanner", false);

        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Visibility toggled for ancient style nodes");

        ApplyCanvasGroupMaskMaterial(portraitCanvasGroup);

        ancientPortrait.ExpandMode = (TextureRect.ExpandModeEnum)1;
        ancientPortrait.StretchMode = (TextureRect.StretchModeEnum)5;
        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Set expand and stretch mode");

        object? model = card.Model;
        if (model != null)
        {
            var typeProperty = model.GetType().GetProperty("Type");
            if (typeProperty != null)
            {
                var typeValue = typeProperty.GetValue(model);
                if (typeValue != null)
                {
                    CardType cardType = (CardType)typeValue;
                    MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Detected card type: {cardType}");
                    ApplyAncientTextBg(cardType, ancientTextBg, cardTypeName);
                }
                else
                {
                    MainFile.Logger.Warn($"[AncientStyle] {cardTypeName}: Type property value is null");
                }
            }
            else
            {
                MainFile.Logger.Warn($"[AncientStyle] {cardTypeName}: Type property not found on model");
            }
        }
        else
        {
            MainFile.Logger.Warn($"[AncientStyle] {cardTypeName}: Model is null when trying to get card type");
        }

        MainFile.Logger.Info($"[AncientStyle] {cardTypeName}: Ancient style applied successfully");
    }

    private static void ApplyAncientTextBg(CardType cardType, TextureRect ancientTextBg, string cardTypeName)
    {
        if (ancientTextBg == null)
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: ancientTextBg is null in ApplyAncientTextBg");
            return;
        }

        string cardTypeStr = cardType switch
        {
            CardType.None => "skill",
            CardType.Status => "skill",
            CardType.Curse => "skill",
            CardType.Quest => "skill",
            CardType.Attack => "attack",
            CardType.Skill => "skill",
            CardType.Power => "power",
            _ => "skill"
        };

        string textBgPath = $"res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_{cardTypeStr}.tres";
        MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Attempting to load text bg: {textBgPath}");

        if (ResourceLoader.Exists(textBgPath))
        {
            Texture2D? textBgTexture = ResourceLoader.Load<Texture2D>(textBgPath, null, ResourceLoader.CacheMode.Ignore);
            if (textBgTexture != null)
            {
                ancientTextBg.Texture = textBgTexture;
                MainFile.Logger.Debug($"[AncientStyle] {cardTypeName}: Applied {cardTypeStr} text bg successfully");
            }
            else
            {
                MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Failed to load texture: {textBgPath}");
            }
        }
        else
        {
            MainFile.Logger.Error($"[AncientStyle] {cardTypeName}: Texture not found: {textBgPath}");
        }
    }

    private static void ApplyCanvasGroupMaskMaterial(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
        {
            MainFile.Logger.Warn("[AncientStyle] ApplyCanvasGroupMaskMaterial: canvasGroup is null");
            return;
        }

        try
        {
            const string materialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";
            MainFile.Logger.Debug($"[AncientStyle] Attempting to load mask material: {materialPath}");

            Material? maskMaterial = GD.Load<Material>(materialPath);

            if (maskMaterial != null)
            {
                canvasGroup.Material = maskMaterial;
                MainFile.Logger.Debug("[AncientStyle] Mask material applied successfully");
            }
            else
            {
                MainFile.Logger.Warn("[AncientStyle] Mask material is null after loading");
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[AncientStyle] Material error: {ex.Message}");
        }
    }

    private static T? SafeGetNode<T>(Node node, NodePath path) where T : class
    {
        try
        {
            if (!node.HasNode(path))
            {
                MainFile.Logger.Warn($"[AncientStyle] Node not found: {path}");
                return null;
            }
            return node.GetNode<T>(path);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[AncientStyle] Failed to get node {path}: {ex.Message}");
            return null;
        }
    }

    private static void SafeSetNodeVisible(Node node, string nodeName, bool visible)
    {
        try
        {
            if (!node.HasNode(nodeName))
            {
                MainFile.Logger.Warn($"[AncientStyle] Node not found for visibility toggle: {nodeName}");
                return;
            }
            Node? targetNode = node.GetNode(nodeName);
            if (targetNode is Control control)
            {
                control.Visible = visible;
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[AncientStyle] Failed to set visibility for {nodeName}: {ex.Message}");
        }
    }
}
