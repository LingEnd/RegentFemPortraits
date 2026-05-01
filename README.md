# 尖塔储君卡图娘化

为《杀戮尖塔2》(Slay the Spire 2) 中储君(Regent)角色替换精美二次元风格卡图的模组。

## 关于这个模组

本模组将储君角色的卡牌立绘替换为精美的二次元风格插画。除了卡图替换，模组还应用了轻量化的纹理重渲染/平滑处理，有助于减少卡牌在缩放、悬停或在大预览视图中打开时的锯齿边缘。

目前已替换 **51 张卡牌**：
- 3 张储君基础牌
- 43 张储君普通/稀有牌
- 5 张棺者卡牌

## 重要说明

1. 本模组是早期预览版本。如果您遇到任何问题，请提交 issue 或联系 @绫末LingEnd。
2. 对于模组存档相关问题，请查看相关指南或安装 save unification mod。
3. 本模组不影响在线游戏玩法。这是一个本地模组，您的在线队友不需要安装它。

## 依赖要求

本模组需要安装 **BaseLib 3.1.0** 或更高版本作为前置依赖。

- **BaseLib**: 提供模组配置框架和本地化支持

## 配置选项

模组提供以下可配置选项（通过游戏内设置菜单访问）：

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| **启用抗锯齿过滤** | 布尔值 | 启用 | 启用后对卡牌立绘应用纹理过滤，减少缩小预览时的锯齿现象 |
| **过滤模式** | 枚举 | 线性+各向异性 | 选择纹理过滤模式，影响清晰度和边缘平滑度 |
| **纹理质量** | 滑块(0-2) | 1 | 控制纹理压缩级别，较高质量占用更多显存 |

### 过滤模式说明

- **最近邻**: 锐利，保留像素风格，可能有明显锯齿
- **双线性**: 平滑显示，边缘略有模糊
- **最近邻+Mipmap**: 平衡清晰度和性能
- **线性+Mipmap**: 平滑边缘，减少锯齿
- **最近邻+各向异性**: 高清晰度，斜视角保持清晰
- **线性+各向异性**: 最平滑的过滤效果，推荐用于缩小预览

## 致谢

### Credits

- **卡图绘制**: [失地骑士Hatk](https://space.bilibili.com/3546631035161405)
- **模组编程**: [绫末LingEnd](https://space.bilibili.com/187863463)

### Special Thanks

- @GhoulEmperor
- @失地骑士Hatk

## 画廊预览

所有卡图统一显示尺寸：120 × 160 像素

### 储君基础牌

| 防御 | 崇拜 | 陨星 |
|------|------|------|
| <img src="RegentFemPortraits/card_portraits/regent/DefendRegent.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Venerate.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/FallingStar.png" width="120" height="160"> |

### 储君普通/稀有牌

| 下去！ | 冲锋！！ | 群星之子 | 群星斗篷 |
|--------|----------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/Begone.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Charge.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/ChildOfTheStars.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/CloakOfStars.png" width="120" height="160"> |

| 征服者 | 汇流 | 宇宙冷漠 | 抉择，抉择 |
|--------|------|----------|------------|
| <img src="RegentFemPortraits/card_portraits/regent/Conqueror.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Convergence.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/CosmicIndifference.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/DecisionsDecisions.png" width="120" height="160"> |

| 辉光 | 天际钻头 | 王者之踢 | 王者之拳 |
|------|----------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/Glow.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/HeavenlyDrill.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/KinglyKick.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/KinglyPunch.png" width="120" height="160"> |

| 如此甚好 | 王之凝视 | 中子护盾 | 星星点点 |
|----------|----------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/MakeItSo.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/MonarchsGaze.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/NeutronAegis.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Patter.png" width="120" height="160"> |

| 招架 | 预言 | 类星体 | 辐射 |
|------|------|--------|------|
| <img src="RegentFemPortraits/card_portraits/regent/Parry.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Prophesize.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Quasar.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Radiate.png" width="120" height="160"> |

| 淬炼刀刃 | 剑圣 | 铸剑者 | 暴政 |
|----------|------|--------|------|
| <img src="RegentFemPortraits/card_portraits/regent/RefineBlade.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/SwordSage.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/TheSmith.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Tyranny.png" width="120" height="160"> |

| 虚空形态 | 战火铸就 | 伽马爆破 | 流光溢彩 |
|----------|----------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/VoidForm.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/WroughtInWar.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/GammaBlast.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Glitterstream.png" width="120" height="160"> |

| 传承之锤 | 创世之柱 | 独白 | 武器库 |
|----------|----------|------|--------|
| <img src="RegentFemPortraits/card_portraits/regent/HeirloomHammer.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/PillarOfCreation.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Monologue.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Arsenal.png" width="120" height="160"> |

| 锻打成型 | 大爆炸 | 新生之喜 | 天穹之力 |
|----------|--------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/BeatIntoShape.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/BigBang.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/BundleOfJoy.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/CelestialMight.png" width="120" height="160"> |

| 收集光辉 | 决胜一击 | 君权自授 | 胜券在王 |
|----------|----------|----------|----------|
| <img src="RegentFemPortraits/card_portraits/regent/GatherLight.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/KnockoutBlow.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/ManifestAuthority.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/RoyalGamble.png" width="120" height="160"> |

| 明耀打击 | 地形改造 | 星尘 |
|----------|----------|------|
| <img src="RegentFemPortraits/card_portraits/regent/ShiningStrike.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Terraforming.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Stardust.png" width="120" height="160"> |

### 棺者卡牌

| 挽歌 | 恐惧 | 重构 | 灵魂风暴 | 收割 |
|------|------|------|----------|------|
| <img src="RegentFemPortraits/card_portraits/regent/Dirge.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Fear.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Transfigure.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/SoulStorm.png" width="120" height="160"> | <img src="RegentFemPortraits/card_portraits/regent/Reap.png" width="120" height="160"> |

---

**版本**: v0.6.0
**作者**: [失地骑士HATK](https://space.bilibili.com/3546631035161405) & [绫末LingEnd](https://space.bilibili.com/187863463)
**游戏**: Slay the Spire 2 (杀戮尖塔2)
**依赖**: BaseLib 3.1.0+
