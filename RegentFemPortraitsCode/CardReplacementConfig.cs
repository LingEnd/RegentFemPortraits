using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Godot;

using FileAccess = Godot.FileAccess;

namespace RegentFemPortraits.RegentFemPortraitsCode;

/// <summary>
/// 表示单个卡牌替换配置的条目。
/// </summary>
public class NormalReplacementEntry
{
    public string CardType { get; set; } = string.Empty;
    public string PortraitPath { get; set; } = string.Empty;
}

/// <summary>
/// 表示单个先古样式替换配置的条目。
/// </summary>
public class AncientReplacementEntry
{
    public string CardType { get; set; } = string.Empty;
    public string NormalPortrait { get; set; } = string.Empty;
    public string AncientPortrait { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
}

/// <summary>
/// 卡牌替换配置的根数据结构。
/// </summary>
public class CardReplacementData
{
    public List<NormalReplacementEntry> NormalReplacements { get; set; } = new();
    public List<AncientReplacementEntry> AncientReplacements { get; set; } = new();
}

/// <summary>
/// 卡牌替换配置管理器。
/// 负责从JSON文件加载和管理所有卡牌替换配置。
/// </summary>
public static class CardReplacementConfig
{
    private static CardReplacementData? _data;
    private static bool _loaded;
    private static bool _loadFailed;

    private const string ConfigPath = "res://RegentFemPortraits/card_replacements.json";

    /// <summary>
    /// 获取普通替换配置字典，键为卡牌类型名，值为纹理路径。
    /// </summary>
    public static Dictionary<string, string> NormalReplacements
    {
        get
        {
            EnsureLoaded();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (_data?.NormalReplacements != null)
            {
                foreach (var entry in _data.NormalReplacements)
                {
                    if (!string.IsNullOrEmpty(entry.CardType) && !string.IsNullOrEmpty(entry.PortraitPath))
                    {
                        dict[entry.CardType] = entry.PortraitPath;
                    }
                }
            }

            return dict;
        }
    }

    /// <summary>
    /// 获取先古替换配置列表。
    /// </summary>
    public static List<AncientReplacementEntry> AncientReplacements
    {
        get
        {
            EnsureLoaded();
            return _data?.AncientReplacements ?? new List<AncientReplacementEntry>();
        }
    }

    /// <summary>
    /// 获取所有配置了先古样式的卡牌类型名列表。
    /// </summary>
    public static List<string> AncientCardTypes
    {
        get
        {
            EnsureLoaded();
            return AncientReplacements
                .Where(a => !string.IsNullOrEmpty(a.CardType))
                .Select(a => a.CardType)
                .ToList();
        }
    }

    /// <summary>
    /// 根据卡牌类型名获取对应的先古配置。
    /// </summary>
    public static AncientReplacementEntry? GetAncientConfig(string cardType)
    {
        EnsureLoaded();
        return AncientReplacements.FirstOrDefault(a =>
            string.Equals(a.CardType, cardType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 检查指定的卡牌类型是否配置了先古替换。
    /// </summary>
    public static bool IsAncientCard(string cardType)
    {
        EnsureLoaded();
        return AncientReplacements.Any(a =>
            string.Equals(a.CardType, cardType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 检查指定的卡牌类型是否配置了普通替换。
    /// </summary>
    public static bool IsNormalReplacement(string cardType)
    {
        EnsureLoaded();
        return NormalReplacements.ContainsKey(cardType);
    }

    /// <summary>
    /// 获取指定卡牌类型对应的普通纹理路径。
    /// </summary>
    public static bool TryGetNormalPortrait(string cardType, out string? path)
    {
        return NormalReplacements.TryGetValue(cardType, out path);
    }

    /// <summary>
    /// 获取指定卡牌类型对应的先古纹理路径。
    /// </summary>
    public static bool TryGetAncientPortrait(string cardType, out string? path)
    {
        path = null;
        var config = GetAncientConfig(cardType);

        if (config != null && !string.IsNullOrEmpty(config.AncientPortrait))
        {
            path = config.AncientPortrait;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取指定卡牌类型对应的普通纹理路径（用于先古卡关闭时显示）。
    /// </summary>
    public static bool TryGetFallbackPortrait(string cardType, out string? path)
    {
        var config = GetAncientConfig(cardType);

        if (config != null && !string.IsNullOrEmpty(config.NormalPortrait))
        {
            path = config.NormalPortrait;
            return true;
        }

        return TryGetNormalPortrait(cardType, out path);
    }

    /// <summary>
    /// 确保配置已加载。
    /// </summary>
    private static void EnsureLoaded()
    {
        if (_loaded || _loadFailed)
        {
            return;
        }

        _loaded = true;

        if (!FileAccess.FileExists(ConfigPath))
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] Card replacement config not found at {ConfigPath}");
            throw new Exception($"Card replacement config not found: {ConfigPath}");
        }

        using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] Failed to open config file: {ConfigPath}");
            throw new Exception($"Failed to open config file: {ConfigPath}");
        }

        string jsonContent = file.GetAsText();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            _data = JsonSerializer.Deserialize<CardReplacementData>(jsonContent, options);
        }
        catch (JsonException jsonEx)
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] JSON parsing error: {jsonEx.Message}");
            throw new Exception($"JSON parsing error: {jsonEx.Message}");
        }

        if (_data == null)
        {
            _loadFailed = true;
            MainFile.Logger.Error("[Config] Failed to parse config file");
            throw new Exception("Failed to parse config file");
        }

        MainFile.Logger.Info($"[Config] Loaded: {NormalReplacements.Count} normal replacements, {AncientReplacements.Count} ancient replacements");
    }
}