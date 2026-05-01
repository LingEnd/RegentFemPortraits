using Godot;
using BaseLib.Config;

namespace RegentFemPortraits.RegentFemPortraitsCode;

public enum FilterMode
{
    Nearest,
    Linear,
    NearestMipmap,
    LinearMipmap,
    NearestAnisotropic,
    LinearAnisotropic
}

[ConfigHoverTipsByDefault]
public class ModConfig : SimpleModConfig
{
    [ConfigHoverTip]
    public static bool EnableAntialiasingFilter { get; set; } = true;

    [ConfigHoverTip]
    public static FilterMode SelectedFilterMode { get; set; } = FilterMode.LinearAnisotropic;

    [ConfigSlider(0, 2, 1)]
    [ConfigHoverTip]
    public static int TextureQuality { get; set; } = 1;

    public static string GetTextureQualityDescription()
    {
        return TextureQuality switch
        {
            0 => "低质量",
            1 => "标准",
            2 => "高质量",
            _ => "标准"
        };
    }

    public static string GetFilterModeDescription()
    {
        return SelectedFilterMode switch
        {
            FilterMode.Nearest => "最近邻 (锐利，保留像素风格，可能有锯齿)",
            FilterMode.Linear => "双线性 (平滑，略有模糊)",
            FilterMode.NearestMipmap => "最近邻+Mipmap (平衡清晰度)",
            FilterMode.LinearMipmap => "线性+Mipmap (平滑边缘，减少锯齿)",
            FilterMode.NearestAnisotropic => "最近邻+各向异性 (高清晰度，斜视角保持清晰)",
            FilterMode.LinearAnisotropic => "线性+各向异性 (最平滑，推荐用于缩小预览)",
            _ => "未知"
        };
    }

    public static CanvasItem.TextureFilterEnum GetGodotFilterMode()
    {
        if (!EnableAntialiasingFilter)
        {
            return CanvasItem.TextureFilterEnum.Nearest;
        }

        return SelectedFilterMode switch
        {
            FilterMode.Nearest => CanvasItem.TextureFilterEnum.Nearest,
            FilterMode.Linear => CanvasItem.TextureFilterEnum.Linear,
            FilterMode.NearestMipmap => (CanvasItem.TextureFilterEnum)2,
            FilterMode.LinearMipmap => (CanvasItem.TextureFilterEnum)3,
            FilterMode.NearestAnisotropic => (CanvasItem.TextureFilterEnum)4,
            FilterMode.LinearAnisotropic => (CanvasItem.TextureFilterEnum)5,
            _ => (CanvasItem.TextureFilterEnum)5
        };
    }

    public static string GetAntialiasingDescription()
    {
        return EnableAntialiasingFilter ? "已启用" : "已禁用";
    }
}
