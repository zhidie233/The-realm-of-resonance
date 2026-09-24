# TMP 字体本地生成说明（SourceHanSans SDF）

> 本文档面向所有协作者。仓库里**不含** `SourceHanSans SDF.asset` 字体资源文件（每个 129MB，超过 GitHub 100MB 限制），需要你在本地用 TextMesh Pro 的 Font Asset Creator 生成。源字体 `.otf` 已在仓库里，生成一次约 5-10 分钟。

## 背景

项目使用思源黑体（Source Han Sans）作为中文字体，共三个字重：

| 字重 | 源字体文件（在仓库里） | 需生成的 SDF 资源（本地生成） |
|------|----------------------|------------------------------|
| Regular（常规） | `Assets/TextMesh Pro/Fonts/SourceHanSans-Regular.otf` | `SourceHanSans-Regular SDF.asset` |
| Normal（中等） | `Assets/TextMesh Pro/Fonts/SourceHanSans-Normal.otf` | `SourceHanSans-Normal SDF.asset` |
| Heavy（粗） | `Assets/TextMesh Pro/Fonts/SourceHanSans-Heavy.otf` | `SourceHanSans-Heavy SDF.asset` |

`SDF.asset` 是 TextMesh Pro 的字体图集资源（含字形纹理 + 字距表等），属于**构建产物**而非源码，所以不进版本控制。谁修改了字体生成参数（字号、Atlas 分辨率、Padding 等），所有人都要重新生成，硬塞 Git 反而频繁冲突。

## 生成步骤

### 1. 打开 Font Asset Creator

Unity 编辑器菜单：**Window → TextMeshPro → Font Asset Creator**

### 2. 逐个生成三个字重

对每个字重，按下表设置参数后点 **Generate Font Atlas** → 等待生成完成 → **Save**，保存到 `Assets/TextMesh Pro/Fonts/` 下，文件名必须和上表"需生成的 SDF 资源"列一致。

| 参数 | 推荐值 | 说明 |
|------|--------|------|
| Source Font File | 选对应的 `.otf` | Regular→Regular.otf，以此类推 |
| Sampling Point Size | Auto Sizing（或 48-64） | 中文字符多，建议 Auto 让 TMP 自行计算 |
| Padding | 5-9 | 字形间距，太小会糊，太大浪费图集 |
| Packing Method | Fast / Optimum | Optimum 更紧凑但慢 |
| Atlas Resolution | 2048×2048 或 4096×4096 | 中文常用字多，建议至少 2048，字符不全则升到 4096 |
| Character Set | Custom Range 或 Chars from File | 见下方"字符集" |
| Render Mode | SDFAA 或 SDF32 | TMP 常用 SDFAA（抗锯齿） |

> ⚠️ **如果第一次生成后发现某些中文字显示为方框/缺失**，说明图集分辨率不够或字符集没覆盖到，调高 Atlas Resolution 或补全字符集后重新生成。

### 3. 字符集（Character Set）建议

中文字体图集很大，建议用 **Custom Range** 包含常用区间，或用一个包含所需汉字的文本文件作为 **Chars from File** 输入：

- ASCII + 常用中文（GB2312 一级字 + 标点）
- 如有游戏专用文案，把所有会出现在 UI 里的汉字整理成一个 `.txt` 喂给 TMP

如果不确定字符集范围，先用 **Custom Range**：`32-126, 12288-40869`（覆盖 ASCII + CJK 统一汉字基本区），生成后检查图集是否填满。

## 常见问题

**Q：我生成完了，但 Unity 报 "Font Asset ... is missing"？**
A：确认生成的 `.asset` 文件名和 `.meta` 路径与项目引用一致。TMP 的字体引用存在 `TMP_Settings.defaultFontAsset` 等设置里，一般文件名对上就自动认到。

**Q：三个字重我都要生成吗？**
A：是的。如果项目里某些字重暂时没用到，可以先只生成 Regular，后续用到再补。但建议一次生成齐全，避免运行时缺字体。

**Q：生成参数和项目负责人不一样怎么办？**
A：以项目负责人维护的参数为准。理想情况下应该把 Font Asset Creator 的设置截图或导出预设同步给团队。若显示效果（字间距、清晰度）和预期不符，找项目负责人确认具体参数。

## 给项目负责人

请在此记录最终采用的生成参数，供团队对齐（删除占位符填实际值）：

- Sampling Point Size：__TODO__
- Padding：__TODO__
- Atlas Resolution：__TODO__
- Packing Method：__TODO__
- Character Set / 字符来源：__TODO__
- Render Mode：__TODO__
