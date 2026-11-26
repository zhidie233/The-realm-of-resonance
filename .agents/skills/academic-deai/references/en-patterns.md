# 英文学术论文 AI 痕迹字典

基于真实 AIGC 检测报告中命中的英文模式整理。每个模式包含：识别特征、修复策略、改写示例。

---

## 1. 元描述开头（Meta-Description Openers）⭐ 头号杀手

### 识别特征
- `This chapter provides an overview of X, focusing on...`
- `This section examines / explores / investigates / discusses...`
- `The analysis proceeds from X to Y`
- `This study therefore poses two key research questions`
- `It aims to show how...`
- `In what follows, we discuss...`
- `The following analysis examines...`
- `This chapter investigates how...`

### 修复策略
**默认：直接删除**。如果该段后面就是内容本身，元描述句完全冗余。如果担心读者迷路，把元描述里的具体信息融到下一句正文里。

### 改写示例

❌ 原文（100% AI）：
> This chapter provides an overview of communicative translation theory, focusing on its origins, basic principles, and practical applications. It aims to show how this approach facilitates effective communication between source and target audiences.

✅ 改写：
> Communicative translation theory grew out of mid-twentieth-century dissatisfaction with strict linguistic equivalence. Its development—through Nida's dynamic equivalence (1964) and Newmark's later refinements (1981, 1988)—rests on a single premise: a translation should produce, on its new readers, an effect close to that the original produced on its own.

(把宣告改成已经在论证；加了具体的引用锚点)

---

## 2. 三件套节奏（Rule of Three）⭐ 高频

### 识别特征
- 三个独立短句平行排列：`It does A. It does B. It does C.`
- 三个形容词并列：`concise, authoritative, and reliable`
- 三个动词并列：`identify, describe, and evaluate`
- 三个名词并列：`its origins, basic principles, and practical applications`

### 修复策略
- **删一个**（通常中间那个信息量最弱），剩下两个不对称展开
- 或者**改成长短句**：把三个合成 1 个长句 + 1 个短句
- 或者**改变句法位置**：让三个元素出现在不同的语法位置

### 改写示例

❌ 原文：
> It stays faithful to the source text's meaning and structure. It uses a measured, restrained register. It expects readers to move toward the text's cultural world.

✅ 改写：
> It stays close to the source text's meaning and structure, using a register that is measured rather than colloquial—an approach that pushes the reader, not the translator, across the cultural distance.

---

## 3. 对仗式对比（Parallel Contrast）⭐ 高频

### 识别特征
- `Where X actively..., Y tends to preserve...`
- `Lyell's version..., by contrast, the Yangs...`
- `While X emphasizes A, Y prioritizes B`
- `On the one hand... on the other hand...`

### 修复策略
- **拆成两个独立段落或不同长度的句子**
- **加入具体例证**打断对称（例如插入一个引用 + 页码 + 原文片段）
- 用 `Lyell does this differently—when he encountered [具体例子], he...` 这种带具体的句式

### 改写示例

❌ 原文：
> Where Lyell actively intervenes to bridge cultural gaps through explanatory notes and idiomatic rephrasing, the Yangs tend to preserve the original's syntactic contours and cultural references with minimal mediation.

✅ 改写：
> Lyell intervenes constantly. His footnote on 古久先生 (p. 12), for instance, explains both the Confucian satirical reference and the literal meaning "Mr. Ancient." The Yangs make a different bargain. They keep the syntax and the cultural references intact, and they leave the reader to either follow up or accept the opacity.

(对仗变成两段不等长描述+具体例子)

---

## 4. AI 高频词汇（AI Buzzwords）

### Tier 1：见到就替换

| AI 词 | 替换建议 |
|-------|---------|
| significance | importance（具体说 to whom）|
| holds a unique place | 具体说"位于 X 的 Y 位置"|
| fundamental shift | change / break |
| particularly important | matters（删评价词，直接陈述）|
| especially relevant | 改写为"is relevant to X because Y"|
| prioritizes | puts X first / chooses X over Y |
| embodies | shows / represents |
| underscores / highlights | shows / makes clear |
| navigate | handle / move through |
| leverage | use |
| seamless | 删除或 smooth |
| groundbreaking | new / first |
| robust | strong / consistent |
| nuanced | 具体说哪里 subtle |
| comprehensive | 删除或 full |
| profound | deep / large |
| pivotal | central / key |
| facilitates | helps / lets / enables |

### Tier 2：学术拖泥带水

| AI 短语 | 替换 |
|---------|------|
| It is important to note that | 删除 |
| It should be emphasized that | 删除 |
| It is worth mentioning that | 删除 |
| In order to | To |
| At its core | 删除或具体化 |
| With respect to | On / For |
| In terms of | 多数可删 |
| A wide range of | many / several / 具体数字 |
| Plays a vital / crucial role | 删评价，说具体作用 |
| Aims to | 直接写动作（"The study examines..." 而非 "The study aims to examine..."）|

### Tier 3：例外——保留
- **学科术语**：foreignization, domestication, skopos, register, parataxis, hypotaxis
- **引用作者原话**
- **已经定义过的理论概念**

---

## 5. 空洞过渡词（Empty Transitions）

### 识别特征
- `From the perspective of X`
- `In practice`
- `Overall`
- `Firstly... Secondly... Finally...`
- `Moreover`（在学术写作里多数情况下可删）
- `Furthermore`
- `In conclusion`（结论段开头不需要这个词）

### 修复策略
**默认删除**。如果需要逻辑过渡，用具体的对比/承接词代替：
- `But Newmark's 1988 revision changes this:...`
- `This pattern breaks down in chapter 3, where...`

---

## 6. 抽象段落无具体锚点

### 诊断
一段全是评价/描述但缺少：
- 引用 `(author, year)`
- 具体页码 `(p. 23)`
- 直接引文 `"..."`
- 数字 / 实例 / 人名

### 修复策略
**每段强制加 1 个具体锚点**。如果原文里真的没有，标注：

> `[此处建议补充：可引用 Newmark (1988) p.40-41 的具体定义]`

**绝不凭空编造引用或例子**。

---

## 7. 平衡句尾摘要 / 评价

### 识别特征
段落末尾出现：
- `These insights inform...`
- `This finding suggests...`
- `Such an approach demonstrates...`
- `In sum...`
- `Taken together...`

### 修复策略
- 把摘要句删掉，让前面的具体描述自己说话
- 或把摘要点改成下一段开头作为承接

---

## 8. 隐性微对仗（Implicit Micro-Parallelism）⭐ v2 实测新发现

### 识别特征
比"Where X..., Y..." 更隐蔽，但仍被国内 AIGC 检测器命中：

- `The X looks at A. The Y looks at B.`（两个短句结构平行）
- `Lyell intervenes; the Yangs preserve.`（分号连接的双动词对比）
- `His version explains; theirs retains.`
- `The comparison runs on two levels. The macro level looks at... The micro level looks at...`
- `What matters is X. What this looks like in practice: Y.`

### 为什么是新模式
在 v1 实测中，即使把显性 `Where X..., Y...` 全部拆开，剩下的隐性微对仗仍占改写后 6 个残留命中段的主要原因。检测器对**"两个长度接近的短句、相同句法位置、相似动词"**的组合很敏感。

### 修复策略
- **打破长度对称**：让两个对比句一长一短
- **插入第三句**：在两个对比句之间塞一个"侧写"句子打断节奏
- **改变动词位置**：让对比的动词出现在主句不同位置（一个做主动词，另一个做分词或从句中的动词）

### 改写示例

❌ "The comparison runs on two levels. The macro level looks at each translator's purpose, paratext, and narrative register. The micro level looks at how culture-loaded items, metaphor, and Chinese parataxis get handled sentence by sentence."

✅ "The comparison works on two levels. At the macro level, what matters is each translator's purpose and the paratext built around the text. The micro level is where the real differences show up—in how culture-loaded items, metaphor, and Chinese parataxis get carried sentence by sentence."

(第二句和第三句不再以 "The X level looks at" 起头；第三句长度明显大于第二句)

---

## 9. 抽象框架词（Abstract Framing Phrases）⭐ v2 实测新发现

### 识别特征
- `the comparison runs on...`
- `the analysis turns on...`
- `the framework rests on...`
- `the question hinges on...`
- `the matter comes down to...`
- `the issue centers on...`

这类短语本身没有特定含义，只是把抽象论点包装得"学术感"——典型 AI 文风。

### 修复策略
- 替换成具体动词 + 直接陈述
- 比如 `The analysis turns on X` → `The analysis depends on X` 还是 AI 腔；最好改成 `If X is wrong, the analysis falls apart`

---

## 10. 隐蔽元描述（Stealth Meta-Description）⭐ v2 实测新发现

第 1 类元描述（"This chapter provides..."）已是常识，但下面这些隐蔽变体同样命中：

### 识别特征
- `What matters is X` （在评论性段落开头）
- `What this looks like in practice: ...`
- `On this view, ...`
- `On this criterion, ...`
- `From the communicative standpoint, ...`
- `The pattern holds when...`
- `The voice-level divergence points to something larger.`

### 修复策略
- 在评论段落里直接说观点，不要先架个 "What matters is..." 的框
- `On this view, X is Y` → 直接 `X is Y`
- `The pattern holds when we turn to metaphor` → `Metaphor works the same way`

---

## 11. 章节首句"主题宣告"

### 识别特征
章节开头一句话说本章是讲什么的，跟标题重复：
- 标题 `2.1 Origins of Communicative Translation`
- 首句 `This subsection traces the origins of communicative translation theory...`

### 修复策略
**删掉首句**，直接从第二句开始。标题已经说明了内容范围。

---

## 总体改写检查清单

改写完一段后自检：
- [ ] 元描述开头是否已删？（含隐蔽变体 "What matters is..." / "On this view..."）
- [ ] 三件套节奏是否打散？
- [ ] 对仗对比是否拆开 + 加了具体例子？
- [ ] **隐性微对仗** 是否打破？（"The X looks at... The Y looks at..." 这种）
- [ ] **抽象框架词** 是否替换？（"the comparison runs on", "the question hinges on"）
- [ ] Tier 1 / Tier 2 AI 词是否替换？
- [ ] 空洞过渡词是否删除？
- [ ] 本段是否有至少 1 个具体锚点（引用/人名/年份/页码/引文）？
- [ ] 句长是否有变化（不是全部 15-25 词的中等句）？
- [ ] hedging 是否保留？
- [ ] 引用是否完整未动？
