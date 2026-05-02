using Godot;

namespace RegentFemPortraits.RegentFemPortraitsCode;

/// <summary>
/// 模组基础配置类。
/// </summary>
public static class ModConfig
{
    public const string ModId = "RegentFemPortraits";

    /// <summary>
    /// 获取纹理过滤模式。
    /// 固定使用线性各向异性过滤，提供最佳的抗锯齿效果。
    /// </summary>
    public static CanvasItem.TextureFilterEnum GetGodotFilterMode()
    {
        return CanvasItem.TextureFilterEnum.LinearWithMipmapsAnisotropic;
    }
}