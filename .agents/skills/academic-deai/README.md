# claude-academic-deai

> 学术论文 AIGC 检测率降低工具 · Claude Code skill · 中英文双语
>
> An academic-paper de-AI rewriter for Claude Code, designed for bilingual (Chinese-English) translation studies and humanities papers.

---

## 这是什么

一个 Claude Code skill，专门用来降低**学术论文**的 AIGC 检测率（万方 / 维普 / PaperPass / 知网 AIGC 报告等国内检测系统）。

跟通用的 humanizer（如 `blader/humanizer`、`Factory-AI/factory-plugins` 里的 `human-writing`）不同，这个 skill 是基于一份**真实学术论文的 AIGC 检测报告**逆向设计的——观察哪些段落被打满分（100% 疑似 AI），归纳出 7 类**结构性** AI 痕迹，再针对每一类写改写规则。

通用 humanizer 用在学术论文上的常见问题：会把 hedging（may / might / suggest）改成绝对化、把 Methods 部分的规范被动语态改成主动、把术语（如 `foreignization`）当成 AI 高频词替换掉——这些都是学术写作的硬规范，不能动。这个 skill **明确把这些列为红线**。

## 检测系统判分参考

| 严重程度 | AIGC 占比 |
|---------|----------|
| 轻度疑似 AI | 70%-80% |
| 中度疑似 AI | 80%-90% |
| 重度疑似 AI | 90% 以上 |

## 处理的 7 类 AI 痕迹

1. **元描述开头** ⭐ —— `This chapter provides an overview of...` / `本章主要探讨...` （头号杀手）
2. **机翻学术腔（中文）** ⭐ —— `本文以 X 为框架` / `对 Y 进行系统分析` 这种英式中文
3. **三件套节奏** —— `It does X. It does Y. It does Z.`
4. **对仗式对比** —— `Where X..., Y...` / `Lyell's..., by contrast, the Yangs...`
5. **AI 高频词汇** —— significance / holds a unique place / 具有重要意义 / 起到了关键作用
6. **空洞过渡词** —— `From the perspective of...` / `综上所述`
7. **抽象段落无锚点** —— 整段评价但缺引用 / 人名 / 页码 / 引文

## 严格保留的特征（红线）

- ✅ 所有引用 `(Author, Year)` 和参考文献
- ✅ Hedging 措辞（`may / might / suggest / appear to`）
- ✅ Methods 章节的规范被动语态
- ✅ 学科术语（`foreignization` / `skopos` / `parataxis` / `交际翻译` / `归化异化` 等）
- ✅ 数字、年份、人名、原文片段
- ✅ 论证方向和作者本意

**绝不凭空生成内容**——如果需要补具体例证而原文没有，会标注 `[此处建议补充：XXX]` 让用户自己加。

## 安装

### 安装为全局 skill（所有 Claude Code 会话都能用）

**Windows (Git Bash / WSL)**：
```bash
cd ~/.claude/skills
git clone https://github.com/akanous/claude-academic-deai.git academic-deai
```

**macOS / Linux**：
```bash
mkdir -p ~/.claude/skills
git clone https://github.com/akanous/claude-academic-deai.git ~/.claude/skills/academic-deai
```

### 安装为项目 skill（仅当前项目）

```bash
mkdir -p .claude/skills
git clone https://github.com/akanous/claude-academic-deai.git .claude/skills/academic-deai
```

## 使用

装好之后，在 Claude Code 里直接说：

- "帮我降这段的 AI 率"
- "改写这段过 AIGC 检测"
- "humanize this paragraph"
- 或者直接贴论文段落

Claude 会自动识别并调用这个 skill，按 4 步流程处理：

1. **诊断** —— 识别语言（中/英/混合）、章节类型（摘要/引言/理论/案例/结论）
2. **加载对应字典** —— 中文段加载 `references/zh-patterns.md`，英文段加载 `references/en-patterns.md`
3. **按 7 类模式修复** —— 优先处理元描述和机翻句式
4. **输出** —— 改写文本 + 改动清单（每处改了什么、为什么）

## 工作原理

观察发现：含有具体引用 / 人名 / 中文原文 / 页码的章节（案例分析章节）AIGC 占比通常比纯论述章节（引言、理论综述）低 20-30 个百分点。所以**最有效的降 AI 率手段是结构性修复 + 加具体锚点**，而不是单纯的同义词替换。

具体的模式字典和改写示例见 `references/` 目录：

- `references/en-patterns.md` —— 英文 AI 模式（11 大类 + 三层词汇替换表 + 改写示例，含 v2 实测新发现的 3 类隐蔽模式）
- `references/zh-patterns.md` —— 中文 AI 模式（10 大类 + 中文摘要专项处理）
- `references/preserve.md` —— 学术写作必须保留的特征清单

## 实测数据

> 以下数据来自一份翻译研究学士论文（中英双语，约 5,000 词）的 AIGC 检测对比。

| 章节 | 改写前 | v1 改写后 | v2 改写后 | 降幅 |
|------|--------|----------|----------|------|
| **总体 AIGC 占比** | **85.01%** 🟡 | **59.94%** 🟢 | 待测 | **↓ 25.07%** |
| 中文摘要 | 100% 🔴 | **0.0%** 🟢 | — | ↓ 100% |
| Abstract | 88% 🟡 | 68% 🟢 | — | ↓ 20% |
| Introduction | 100% 🔴 | 57% 🟢 | — | ↓ 43% |
| Ch.2 Theory | 95% 🔴 | 43% 🟢 | — | ↓ 52% |
| Ch.3 Comparative | 73% 🟢 | 73% 🟢 | 目标 ~40% | — |
| Ch.4 Conclusion | 87% 🟡 | **21%** 🟢 | — | ↓ 66% |

**关键发现**：

1. **完全重写的核心章节（Abstract / Introduction / Ch.2 / Conclusion）AI 占比降幅 20-66 个百分点**——"结构性修复"策略（元描述清除 + 三件套打散 + 对仗拆解 + 加具体锚点）持续有效
2. **中文摘要 0%** —— 完全重写而非局部修改的"中文摘要专项处理"规则是正确的
3. **Ch.3（案例章节）73% 完全没变** —— v1 只改了 2 段元描述段，其余 51 段原始文本未动。v2 追加了 11 段纯论述段的重写，预期总分能跌到 35-45%
4. **v1 残留命中的 6 段**暴露出 3 类新 AI 模式：隐性微对仗（"The macro level looks at... The micro level looks at..."）、抽象框架词（"the comparison runs on..."）、隐蔽元描述（"What matters is..."）——已在 v2 的 `references/en-patterns.md` 中收录为第 8-10 类

## ⚠️ 关于学术诚信

这个工具的预设场景是：**论文是你自己写的（或主要由你写的），但因为写作风格偏程式化、句式偏机翻腔，被 AIGC 检测系统误判**。

斯坦福 2023 年研究（Liang et al., *Patterns*）发现 AI 检测器对**非母语英文写作者**的误报率可达 60% 以上——也就是说，中国学生写的真人英文论文经常被误判。这个工具帮助你把写作风格调整得更"非套路化"，减少这种误报。

**这个工具不能也不应该被用于**：
- 把完全由 AI 生成的论文改写后冒充原创
- 学术不端行为
- 通过任何形式逃避学术诚信审查

如果你的论文确实是 AI 生成的，请回去自己写。

## License

MIT —— 见 [LICENSE](LICENSE)

## Credits

设计思路参考：
- [Hakku/finnish-humanizer](https://github.com/Hakku/finnish-humanizer) —— SKILL.md 模式分类的启发
- [blader/humanizer](https://github.com/blader/humanizer) —— Wikipedia "Signs of AI writing" 规则
- [Imbad0202/academic-research-skills](https://github.com/Imbad0202/academic-research-skills) —— 学术写作优先 > humanize 的设计哲学
- Liang et al. (2023). "GPT detectors are biased against non-native English writers." *Patterns*, 4(7).
