const path = require('path');
let PptxGenJS;
try {
  PptxGenJS = require('pptxgenjs');
} catch {
  PptxGenJS = require('C:/Users/wukaidong/AppData/Roaming/npm/node_modules/pptxgenjs');
}

const RAW_DIR = 'e:/1FairyOnTheTree/The-realm-of-resonance/Docs/ppt_v2/assets';
const PREP_DIR = 'e:/1FairyOnTheTree/The-realm-of-resonance/Docs/ppt_v2/prepared';
const OUT_FILE = 'e:/1FairyOnTheTree/The-realm-of-resonance/Docs/共鸣之境_项目汇报.pptx';

// 新瑞士声学大字报配色系统（严格与方向 1 一致，零圆角卡片）
const C = {
  bg: 'F7F6F2',       // 暖微灰纸白
  white: 'FFFFFF',
  ink: '0D1017',      // 浓墨黑
  rule: '111318',     // 主标尺黑线
  sub: '4A5260',      // 副墨灰
  muted: '6B7280',    // 辅助说明
  grid: 'D6D8DE',     // 辅助细灰线
  blue: '1A4BFF',     // 电光钴蓝
  coral: 'E8451E',    // 熔岩朱红
  teal: '0F766E',     // 深青验证
  subtle: 'EEEDE8',   // 极淡暖灰条
};

const F_ZH = 'Microsoft YaHei';
const F_NUM = 'Bahnschrift';

const pres = new PptxGenJS();
pres.layout = 'LAYOUT_WIDE'; // 13.333 x 7.5 in
pres.author = '吴凯东 · 杨世杰 · 周欣琦';
pres.company = '福建师范大学协和学院 · 信息技术系 · 数字媒体技术';
pres.subject = '基于 Unity 的联机游戏《共鸣之境》项目设计汇报';
pres.title = '《共鸣之境》项目设计汇报';

function rawImg(name) { return path.join(RAW_DIR, name); }
function prepImg(name) { return path.join(PREP_DIR, name); }

function addMasthead(slide, section, pageNum) {
  slide.background = { color: C.bg };
  slide.addText(section, {
    x: 0.55, y: 0.24, w: 6.20, h: 0.26,
    fontFace: F_ZH, fontSize: 10.5, bold: true, color: C.blue, margin: 0, valign: 'middle',
  });
  slide.addText('共鸣之境 · 联机第一人称射击游戏', {
    x: 7.00, y: 0.24, w: 5.78, h: 0.26,
    fontFace: F_ZH, fontSize: 10.5, color: C.sub, align: 'right', margin: 0, valign: 'middle',
  });
  slide.addShape(pres.ShapeType.line, {
    x: 0.55, y: 0.56, w: 12.23, h: 0,
    line: { color: C.rule, width: 1.25 },
  });
  // 底部极简页码（把空间全部留给主体）
  slide.addText(`${String(pageNum).padStart(2, '0')} / 15`, {
    x: 11.78, y: 7.12, w: 1.00, h: 0.24,
    fontFace: F_NUM, fontSize: 9.5, bold: true, color: C.ink, align: 'right', margin: 0,
  });
}

function addHLine(slide, x, y, w, color = C.grid, width = 0.75) {
  slide.addShape(pres.ShapeType.line, { x, y, w, h: 0, line: { color, width } });
}

function addVLine(slide, x, y, h, color = C.grid, width = 0.75) {
  slide.addShape(pres.ShapeType.line, { x, y, w: 0, h, line: { color, width } });
}

function addFramedPhoto(slide, filePath, x, y, w, h) {
  slide.addImage({ path: filePath, x, y, w, h });
  slide.addShape(pres.ShapeType.rect, {
    x, y, w, h,
    fill: { type: 'none' },
    line: { color: C.rule, width: 1.25 },
  });
}

// =========================================================================
// SLIDE 1: 封面（1:1 复刻方向 1 左右 52%/48% 刀切全出血版式）
// =========================================================================
{
  const s = pres.addSlide();
  s.background = { color: C.bg };

  // 左侧 52% 刊头
  s.addText('UNITY 2022.3 · PHOTON PUN 2 · URP 14', {
    x: 0.55, y: 0.38, w: 4.10, h: 0.26,
    fontFace: F_NUM, fontSize: 10.5, bold: true, color: C.blue, margin: 0,
  });
  s.addText('项目设计汇报 · 2026.11', {
    x: 4.65, y: 0.38, w: 1.95, h: 0.26,
    fontFace: F_ZH, fontSize: 10.5, color: C.sub, align: 'right', margin: 0,
  });
  addHLine(s, 0.55, 0.74, 6.05, C.rule, 1.25);

  // 红色短横 + 超大主标题
  s.addShape(pres.ShapeType.rect, {
    x: 0.55, y: 1.48, w: 0.62, h: 0.055,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('共鸣之境', {
    x: 0.48, y: 1.66, w: 6.20, h: 1.18,
    fontFace: F_ZH, fontSize: 66, bold: true, color: C.ink, margin: 0,
  });
  s.addText('THE REALM OF RESONANCE', {
    x: 0.55, y: 2.88, w: 6.05, h: 0.32,
    fontFace: F_NUM, fontSize: 13.5, bold: true, color: C.sub, charSpacing: 4, margin: 0,
  });
  s.addText([
    { text: '基于', options: { color: C.ink } },
    { text: '全局共鸣机制', options: { color: C.blue, bold: true } },
    { text: '的联机第一人称射击游戏——\n让每一次交火、设施争夺与夺旗，实时改写战场物理规则。', options: { color: C.ink } },
  ], {
    x: 0.55, y: 3.42, w: 5.95, h: 1.10,
    fontFace: F_ZH, fontSize: 15.5, margin: 0, lineSpacingMultiple: 1.4,
  });

  // 底部署名网格（无框瑞士标尺）
  addHLine(s, 0.55, 5.02, 6.05, C.rule, 1.25);
  s.addText('院校与专业', {
    x: 0.55, y: 5.20, w: 2.80, h: 0.24,
    fontFace: F_ZH, fontSize: 10.5, bold: true, color: C.muted, margin: 0,
  });
  s.addText('福建师范大学协和学院\n信息技术系 · 数字媒体技术', {
    x: 0.55, y: 5.46, w: 3.20, h: 0.56,
    fontFace: F_ZH, fontSize: 12.5, bold: true, color: C.ink, margin: 0, lineSpacingMultiple: 1.25,
  });
  s.addText('指导教师', {
    x: 3.95, y: 5.20, w: 2.65, h: 0.24,
    fontFace: F_ZH, fontSize: 10.5, bold: true, color: C.muted, margin: 0,
  });
  s.addText('黄取治\n2026 年 11 月', {
    x: 3.95, y: 5.46, w: 2.65, h: 0.56,
    fontFace: F_ZH, fontSize: 12.5, bold: true, color: C.ink, margin: 0, lineSpacingMultiple: 1.25,
  });

  addHLine(s, 0.55, 6.18, 6.05, C.grid, 0.75);
  s.addText('项目成员与职责分工', {
    x: 0.55, y: 6.32, w: 6.05, h: 0.24,
    fontFace: F_ZH, fontSize: 10.5, bold: true, color: C.muted, margin: 0,
  });
  s.addText('吴凯东（程序框架设计） · 杨世杰（业务逻辑实现） · 周欣琦（美术、策划、测试）', {
    x: 0.55, y: 6.60, w: 6.15, h: 0.36,
    fontFace: F_ZH, fontSize: 11.2, bold: true, color: C.ink, margin: 0,
  });

  // 右侧 48% 全出血地图 (6.39 x 7.50) + 底部黑底红线图注
  s.addImage({ path: prepImg('cover_map_bleed.png'), x: 6.94, y: 0, w: 6.393, h: 7.50 });
  addVLine(s, 6.94, 0, 7.50, C.rule, 1.5);
  s.addShape(pres.ShapeType.rect, {
    x: 6.94, y: 6.84, w: 6.393, h: 0.66,
    fill: { color: C.ink }, line: { type: 'none' },
  });
  s.addShape(pres.ShapeType.rect, {
    x: 6.94, y: 6.82, w: 6.393, h: 0.03,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('「小镇」对战地图全景 · NavMesh 与 30 处掩体网络', {
    x: 7.18, y: 6.86, w: 4.40, h: 0.62,
    fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.bg, valign: 'middle', margin: 0,
  });
  s.addText('310 脚本 / 44,039 行', {
    x: 11.58, y: 6.86, w: 1.52, h: 0.62,
    fontFace: F_NUM, fontSize: 11, color: '9CA3AF', align: 'right', valign: 'middle', margin: 0,
  });
}

// =========================================================================
// SLIDE 2: 汇报脉络与工程全貌（左瑞士目录 + 右 4 层巨数标尺）
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '00 · 汇报概览', 2);

  s.addText('一条全局共鸣中枢，串起联机 FPS 全链路工程', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 左侧 58%：5 条 1px 横线分隔的无框目录
  const toc = [
    ['01', '项目背景与玩法构成', '破解中后期固定卡点僵局，构建三模式、四兵种与八武器全表驱动体系'],
    ['02', '全局共鸣核心机制', '房主权威结算共鸣值，跨阈值同步触发重力、火海、空投、AI 与陨石剧变'],
    ['03', '关键工程架构实现', '三层程序集解耦、Photon 四通道同步、Heun 弹道、AI 状态机与 URP 双相机'],
    ['04', '云端联机与性能实测', '接入 Photon 云端实网测试六大核心场景，360 帧采样实现零 Gen0 GC'],
    ['05', '团队分工与演进方向', '三人职责边界、契约与配置并行协作，以及后续网络与专用服演进规划'],
  ];
  toc.forEach(([num, title, desc], i) => {
    const y = 1.58 + i * 1.06;
    addHLine(s, 0.55, y, 7.20, i === 0 ? C.rule : C.grid, i === 0 ? 1.25 : 0.75);
    s.addText(num, {
      x: 0.55, y: y + 0.16, w: 0.72, h: 0.68,
      fontFace: F_NUM, fontSize: 28, bold: true, color: i === 1 ? C.coral : C.blue, margin: 0, valign: 'top',
    });
    s.addText(title, {
      x: 1.38, y: y + 0.16, w: 6.35, h: 0.34,
      fontFace: F_ZH, fontSize: 16.5, bold: true, color: C.ink, margin: 0,
    });
    s.addText(desc, {
      x: 1.38, y: y + 0.54, w: 6.35, h: 0.38,
      fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0,
    });
  });
  addHLine(s, 0.55, 6.88, 7.20, C.rule, 1.25);

  // 右侧 38%：1.5pt 竖黑线分隔 + 4 层巨数标尺
  addVLine(s, 8.15, 1.58, 5.30, C.rule, 1.5);
  const kpis = [
    ['310 / 44,039', '一方脚本文件与 C# 总行数', '三层程序集架构 · 85 个预制体 / 31 份配置', C.ink],
    ['8 · 4 · 3', '武器 · 兵种 · 对局模式', '统一走表格与配置资产驱动，改数免改代码', C.blue],
    ['28 / 13', 'RPC 方法 · 全局广播事件码', '四通道联机同步 · 支持房主离线无缝接管', C.teal],
    ['67.7 FPS', '46 处火海并发实测平均帧率', '360 帧连续采样期内 Gen0 GC 增量 = 0 次', C.coral],
  ];
  kpis.forEach(([val, t, sub, col], i) => {
    const ky = 1.58 + i * 1.34;
    if (i > 0) addHLine(s, 8.48, ky - 0.08, 4.30, C.grid, 0.75);
    s.addText(val, {
      x: 8.48, y: ky + 0.02, w: 4.30, h: 0.50,
      fontFace: F_NUM, fontSize: 34, bold: true, color: col, margin: 0,
    });
    s.addText(t, {
      x: 8.48, y: ky + 0.54, w: 4.30, h: 0.28,
      fontFace: F_ZH, fontSize: 13, bold: true, color: C.ink, margin: 0,
    });
    s.addText(sub, {
      x: 8.48, y: ky + 0.84, w: 4.30, h: 0.30,
      fontFace: F_ZH, fontSize: 11, color: C.sub, margin: 0,
    });
  });
}

// =========================================================================
// SLIDE 3: 项目背景与破局思路（左粗线因果宣言 + 右大厅实机）
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '01 · 项目背景与玩法设计', 3);

  s.addText([
    { text: '打破中后期固定卡点僵局，让射击', options: { color: C.ink } },
    { text: '实时改写战场', options: { color: C.coral } },
  ], {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, margin: 0,
  });

  // 左 43%：痛点 -> 红底锚块 -> 破局
  addHLine(s, 0.55, 1.56, 5.40, C.rule, 1.5);
  s.addText('01 / 传统痛点', {
    x: 0.55, y: 1.70, w: 5.40, h: 0.26,
    fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.muted, margin: 0,
  });
  s.addText('联机 FPS 中后期交火点固化', {
    x: 0.55, y: 2.02, w: 5.40, h: 0.38,
    fontFace: F_ZH, fontSize: 18, bold: true, color: C.ink, margin: 0,
  });
  s.addText('对局进入中后期，双方玩家熟悉地图掩体与架枪点位，攻防节奏趋于停滞，缺乏迫使双方转点的动态变量。', {
    x: 0.55, y: 2.48, w: 5.40, h: 0.90,
    fontFace: F_ZH, fontSize: 13, color: C.sub, margin: 0, lineSpacingMultiple: 1.4,
  });

  // 熔岩红锚块
  s.addShape(pres.ShapeType.rect, {
    x: 0.55, y: 3.68, w: 5.40, h: 0.62,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('破局核心：全图共享的全局共鸣值', {
    x: 0.76, y: 3.68, w: 5.00, h: 0.62,
    fontFace: F_ZH, fontSize: 14.5, bold: true, color: C.white, valign: 'middle', margin: 0,
  });

  addHLine(s, 0.55, 4.62, 5.40, C.blue, 1.5);
  s.addText('02 / 动态破局', {
    x: 0.55, y: 4.76, w: 5.40, h: 0.26,
    fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.blue, margin: 0,
  });
  s.addText('玩家行为实时重构战场物理条件', {
    x: 0.55, y: 5.08, w: 5.40, h: 0.38,
    fontFace: F_ZH, fontSize: 18, bold: true, color: C.ink, margin: 0,
  });
  s.addText('命中敌方、误伤友军、击打环境设施与夺旗实时改变全局共鸣；越过阈值同步触发重力调制、地面火海、空投、中立 AI 与陨石打击。', {
    x: 0.55, y: 5.54, w: 5.40, h: 1.15,
    fontFace: F_ZH, fontSize: 13, color: C.sub, margin: 0, lineSpacingMultiple: 1.4,
  });

  // 右 53%：大厅 16:9 实机截图 (6.45 x 3.63) + 3 列无框特性带
  addFramedPhoto(s, rawImg('lobby_ui.png'), 6.32, 1.56, 6.45, 3.63);
  s.addText('实机大厅与建房界面：自选地图、对局模式、目标阈值、单局时长与 AI / 共鸣开关', {
    x: 6.32, y: 5.26, w: 6.45, h: 0.28,
    fontFace: F_ZH, fontSize: 11, color: C.muted, margin: 0,
  });

  addHLine(s, 6.32, 5.66, 6.45, C.rule, 1.25);
  const feats = [
    ['2–16 人建房', '自选地图、模式、目标得分阈值与单局时长'],
    ['云端 + 离线双轨', '外网直连 Photon 云端；无外网切换离线模式调试'],
    ['智能 AI 补位', '团队与混战模式自动填充机器人并支持真人顶替'],
  ];
  feats.forEach(([t, d], i) => {
    const fx = 6.32 + i * 2.18;
    if (i > 0) addVLine(s, fx - 0.10, 5.80, 1.06, C.grid, 0.75);
    s.addText(t, {
      x: fx, y: 5.80, w: 2.04, h: 0.30,
      fontFace: F_ZH, fontSize: 13, bold: true, color: C.blue, margin: 0,
    });
    s.addText(d, {
      x: fx, y: 6.14, w: 2.04, h: 0.72,
      fontFace: F_ZH, fontSize: 11, color: C.sub, margin: 0, lineSpacingMultiple: 1.3,
    });
  });
}

// =========================================================================
// SLIDE 4: 玩法构成与八武器表驱动体系
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '01 · 项目背景与玩法设计', 4);

  s.addText('三模式、四兵种与八武器全表驱动，支撑多节奏博弈', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 上部双栏无框概览
  addHLine(s, 0.55, 1.44, 12.23, C.rule, 1.25);
  addVLine(s, 6.66, 1.54, 0.88, C.grid, 1);
  s.addText('三大模式 · 蓝红双阵营 + 第三阵营中立 AI', {
    x: 0.55, y: 1.54, w: 5.90, h: 0.30,
    fontFace: F_ZH, fontSize: 13.5, bold: true, color: C.blue, margin: 0,
  });
  s.addText('团队竞赛 / 自由混战 / 夺旗模式，高共鸣下刷出第三阵营机器人搅局', {
    x: 0.55, y: 1.88, w: 5.90, h: 0.50,
    fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0,
  });
  s.addText('四大兵种 · 主 / 副 / 近战 / 战术四槽位自由配装', {
    x: 6.92, y: 1.54, w: 5.86, h: 0.30,
    fontFace: F_ZH, fontSize: 13.5, bold: true, color: C.coral, margin: 0,
  });
  s.addText('突击兵 / 侦察兵 / 支援兵 / 工程兵预设，支持战前与局内快速换装', {
    x: 6.92, y: 1.88, w: 5.86, h: 0.50,
    fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0,
  });
  addHLine(s, 0.55, 2.48, 12.23, C.rule, 1.25);

  // 下左：瑞士横线 8 武器配置表（无竖线，清爽可读）
  const wRows = [
    ['武器名称', '类型', '伤害', '射速间隔', '换弹', '射程', '精准', '负重'].map((t) => ({
      text: t,
      options: { bold: true, color: C.bg, fill: { color: C.ink }, align: 'center', fontSize: 11 },
    })),
    ['自动步枪', '连发步枪', '25', '0.09 s', '2.0 s', '700', '1', '2.0'],
    ['战术手枪', '手枪', '25', '0.25 s', '1.5 s', '300', '3', '1.95'],
    ['霰弹枪', '霰弹枪', '50', '1.25 s', '6.0 s', '100', '1', '2.2'],
    ['破片手雷', '投掷物', '80', '2.00 s', '2.0 s', '125', '1', '1.7'],
    ['狙击步枪', '狙击枪', '100', '1.80 s', '3.0 s', '700', '5', '2.2'],
    ['近战匕首', '近战', '60', '0.70 s', '1.0 s', '2', '1', '1.6'],
    ['燃烧瓶', '燃烧投掷', '20', '1.00 s', '1.2 s', '300', '2', '0.0'],
    ['榴弹发射器', '榴弹炮', '75', '2.00 s', '3.0 s', '100', '1', '2.5'],
  ].map((row, rIdx) => {
    if (rIdx === 0) return row;
    return row.map((cell, cIdx) => {
      let col = cIdx === 0 ? C.ink : C.sub;
      let isBold = cIdx === 0;
      if (rIdx === 5 && cIdx === 2) { col = C.coral; isBold = true; } // 100 伤害
      if (rIdx === 1 && cIdx === 3) { col = C.blue; isBold = true; }  // 0.09s
      return {
        text: cell,
        options: {
          fill: { color: rIdx % 2 === 1 ? C.white : C.bg },
          color: col,
          bold: isBold,
          align: cIdx <= 1 ? 'left' : 'center',
          fontSize: 11,
        },
      };
    });
  });

  s.addTable(wRows, {
    x: 0.55, y: 2.68, w: 7.05, h: 4.02,
    colW: [1.38, 1.05, 0.72, 0.90, 0.72, 0.76, 0.72, 0.80],
    rowH: [0.42, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45],
    border: [
      { pt: 1.2, color: C.rule },
      { pt: 0, color: C.bg },
      { pt: 0.75, color: C.grid },
      { pt: 0, color: C.bg },
    ],
    fontFace: F_ZH,
    valign: 'middle',
    margin: [2, 6, 2, 6],
  });
  s.addText('注：全部 8 把武器统一由武器配置表驱动，另预留点射连发武器扩展槽位。', {
    x: 0.55, y: 6.76, w: 7.05, h: 0.26,
    fontFace: F_ZH, fontSize: 10.5, color: C.muted, margin: 0,
  });

  // 下右：装备界面实机图 (1200x740 => w=4.90, h=3.02)
  addFramedPhoto(s, rawImg('loadout_ui.png'), 7.88, 2.68, 4.90, 3.02);
  addHLine(s, 7.88, 5.86, 4.90, C.rule, 1);
  s.addText('数据驱动 · 增改武器免改核心脚本', {
    x: 7.88, y: 5.96, w: 4.90, h: 0.30,
    fontFace: F_ZH, fontSize: 13, bold: true, color: C.ink, margin: 0,
  });
  s.addText('兵种四槽位引导与武器数值完全资产化；燃烧瓶复用投掷框架并在落地后生成持续燃烧伤害区域。', {
    x: 7.88, y: 6.28, w: 4.90, h: 0.62,
    fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.32,
  });
}

// =========================================================================
// SLIDE 5: 核心创新：全局共鸣跃迁机制（1:1 还原方向 1 第二页）
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '02 · 核心机制与战场演化', 5);

  // 上左 40%
  s.addText([
    { text: '每一次交火，\n都在改写', options: { color: C.ink } },
    { text: '物理规则', options: { color: C.coral } },
  ], {
    x: 0.55, y: 0.78, w: 4.95, h: 1.10,
    fontFace: F_ZH, fontSize: 27, bold: true, margin: 0, lineSpacingMultiple: 1.18,
  });
  s.addText('房主端实时汇总对局行为并写入房间共鸣属性。一旦跨过临界阈值，五类环境响应在所有客户端同步爆发，强制打破阵地僵局。', {
    x: 0.55, y: 2.02, w: 4.75, h: 1.10,
    fontFace: F_ZH, fontSize: 13.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.45,
  });

  addHLine(s, 0.55, 3.44, 4.75, C.grid, 0.75);
  const wItems = [
    ['命中敌方 / 误伤友方', '+1 / −1', C.blue, C.coral],
    ['共鸣增幅器 / 抑制器', '+2 / −2', C.blue, C.coral],
    ['夺旗 / 交旗 / 归还', '+10 · +20 / −5', C.blue, C.coral],
    ['设施主动触发', '陨石 50×1~3', C.ink, C.coral],
  ];
  wItems.forEach(([lbl, val, c1, c2], i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const wx = 0.55 + col * 2.45;
    const wy = 3.60 + row * 0.82;
    s.addText(lbl, {
      x: wx, y: wy, w: 2.35, h: 0.24,
      fontFace: F_ZH, fontSize: 11, bold: true, color: C.muted, margin: 0,
    });
    s.addText(val, {
      x: wx, y: wy + 0.28, w: 2.35, h: 0.36,
      fontFace: F_NUM, fontSize: 17, bold: true, color: i === 3 ? C.coral : C.blue, margin: 0,
    });
  });

  // 上右 58%：2x 高精瑞士跃迁图 (1160x640 => w=7.25, h=4.00)
  s.addImage({ path: prepImg('diag_resonance.png'), x: 5.52, y: 0.86, w: 7.25, h: 4.00 });

  // 下方：2pt 贯通黑线 + 4 列无框标尺巨数带
  addHLine(s, 0.55, 5.14, 12.23, C.rule, 1.75);
  const bottomKpis = [
    ['高共鸣并发实测帧率', '67.7 FPS', '46 处火海 + 多 AI 并发交火', C.blue],
    ['地面火海同步节点', '46 处', '越过 150 点燃 · 降至 100 熄灭', C.coral],
    ['跳跃初速度调制', '11.0 m/s', '由初始 5.00 随共鸣 1000 跃升', C.ink],
    ['360 帧内存回收增量', '0 次 GC', '预热对象池 + 集中帧调度器', C.ink],
  ];
  bottomKpis.forEach(([lbl, num, sub, col], i) => {
    const kx = 0.55 + i * 3.10;
    if (i > 0) addVLine(s, kx - 0.16, 5.26, 1.66, C.grid, 0.75);
    s.addText(lbl, {
      x: kx, y: 5.30, w: 2.86, h: 0.26,
      fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.sub, margin: 0,
    });
    s.addText(num, {
      x: kx, y: 5.62, w: 2.86, h: 0.74,
      fontFace: F_NUM, fontSize: 38, bold: true, color: col, margin: 0,
    });
    s.addText(sub, {
      x: kx, y: 6.44, w: 2.86, h: 0.36,
      fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0,
    });
  });
}

// =========================================================================
// SLIDE 6: 共鸣实机对照与零代码调参
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '02 · 核心机制与战场演化', 6);

  s.addText('从常态架枪到火海爆发：实机视觉反馈与可视化调参', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 上左：常态 (5.95 x 3.35) + 纯黑图注条
  addFramedPhoto(s, rawImg('fps_gun.png'), 0.55, 1.44, 5.95, 3.35);
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 4.79, w: 5.95, h: 0.42, fill: { color: C.ink }, line: { type: 'none' } });
  s.addText('常态 · 共鸣值 -100  |  标准跳跃初速 5.00，常规掩体架枪对抗', {
    x: 0.72, y: 4.79, w: 5.65, h: 0.42,
    fontFace: F_ZH, fontSize: 11, bold: true, color: C.bg, valign: 'middle', margin: 0,
  });

  // 上右：高共鸣 (5.95 x 3.35) + 熔岩红图注条
  addFramedPhoto(s, rawImg('resonance_fire.png'), 6.83, 1.44, 5.95, 3.35);
  s.addShape(pres.ShapeType.rect, { x: 6.83, y: 4.79, w: 5.95, h: 0.42, fill: { color: C.coral }, line: { type: 'none' } });
  s.addText('高共鸣 · 共鸣值 350  |  越过阈值 150，46 处地面火海同步激活', {
    x: 7.00, y: 4.79, w: 5.65, h: 0.42,
    fontFace: F_ZH, fontSize: 11, bold: true, color: C.white, valign: 'middle', margin: 0,
  });

  // 下方无框三列：左配置中心实景 (1200x740 => h=1.42, w=2.30) + 右三段要点
  addHLine(s, 0.55, 5.38, 12.23, C.rule, 1.5);
  addFramedPhoto(s, rawImg('config_center.png'), 0.55, 5.54, 2.30, 1.42);

  const cfgCols = [
    ['可视化配置中心', '在 Unity 编辑器内集中调节共鸣权重、重力曲线、火海阈值 150 与陨石参数，所见即所得。', C.blue],
    ['全端闭环同步回收', '实测共鸣升至 350 时 46 处火焰全量点燃，攻击抑制器压回 100 后所有火点彻底销毁。', C.coral],
    ['低侵入独立解耦', '共鸣管理与五类环境设施独立收敛于 19 个脚本（仅 1,084 行），扩展新异常不耦合主干。', C.ink],
  ];
  cfgCols.forEach(([t, d, col], i) => {
    const cx = 3.12 + i * 3.26;
    if (i > 0) addVLine(s, cx - 0.16, 5.54, 1.42, C.grid, 0.75);
    s.addText(t, {
      x: cx, y: 5.54, w: 3.00, h: 0.30,
      fontFace: F_ZH, fontSize: 13.5, bold: true, color: col, margin: 0,
    });
    s.addText(d, {
      x: cx, y: 5.90, w: 3.00, h: 1.02,
      fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.35,
    });
  });
}

// =========================================================================
// SLIDE 7: 工程总体架构与技术基座
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '03 · 关键工程架构实现', 7);

  s.addText('三层程序集解耦九大模块，驾驭 4.4 万行工程代码', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 左 60%：直角三层程序集蓝图（无圆角）
  // 1. Editor
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 1.48, w: 7.45, h: 0.86, fill: { color: C.white }, line: { color: C.rule, width: 1.25 } });
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 1.48, w: 2.15, h: 0.86, fill: { color: C.ink }, line: { type: 'none' } });
  s.addText('编辑器工具层\n21 文件 / 5,287 行', {
    x: 0.55, y: 1.48, w: 2.15, h: 0.86,
    fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.bg, align: 'center', valign: 'middle', margin: 0,
  });
  s.addText('15 个自定义 Inspector 检视面板 + 2 个工具窗口（可视化配置中心），支撑无代码调参', {
    x: 2.86, y: 1.48, w: 4.98, h: 0.86,
    fontFace: F_ZH, fontSize: 11.5, color: C.sub, valign: 'middle', margin: 0,
  });

  // 2. Runtime (9 modules)
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 2.48, w: 7.45, h: 3.44, fill: { color: C.white }, line: { color: C.blue, width: 1.5 } });
  s.addText('核心运行时层 · 257 文件 / 36,686 行（九大高内聚业务模块）', {
    x: 0.74, y: 2.58, w: 7.05, h: 0.30,
    fontFace: F_ZH, fontSize: 12.5, bold: true, color: C.blue, margin: 0,
  });

  const mods = [
    ['核心框架', '72 / 5,500', '配置根 / 对象池 / 调度'],
    ['界面系统', '53 / 6,364', '大厅 / 房间 / HUD'],
    ['世界关卡', '25 / 2,597', '场景 / 音频 / 交互门梯'],
    ['角色控制', '23 / 4,925', '第一人称 / 相机 / 骨骼'],
    ['核心战斗', '22 / 5,544', '八武器 / 弹道 / 爆炸'],
    ['共鸣异常', '19 / 1,084', '共鸣中枢 / 五类剧变'],
    ['联机网络', '19 / 4,797', '大厅 / 准备厅 / 四通道'],
    ['AI 机器人', '12 / 3,938', '七态 FSM / 掩体 / 寻路'],
    ['对局模式', '12 / 1,937', '三模式 / 计时 / 播报'],
  ];
  mods.forEach(([mName, mStat, mDesc], i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    const mx = 0.74 + col * 2.40;
    const my = 2.98 + row * 0.94;
    const isHot = mName === '共鸣异常';
    s.addShape(pres.ShapeType.rect, {
      x: mx, y: my, w: 2.26, h: 0.82,
      fill: { color: isHot ? 'FEECE8' : C.bg },
      line: { color: isHot ? C.coral : C.grid, width: isHot ? 1.5 : 0.75 },
    });
    s.addText(mName, {
      x: mx + 0.10, y: my + 0.08, w: 1.10, h: 0.26,
      fontFace: F_ZH, fontSize: 12, bold: true, color: isHot ? C.coral : C.ink, margin: 0,
    });
    s.addText(mStat, {
      x: mx + 1.15, y: my + 0.08, w: 1.02, h: 0.26,
      fontFace: F_NUM, fontSize: 9.5, color: C.muted, align: 'right', margin: 0,
    });
    s.addText(mDesc, {
      x: mx + 0.10, y: my + 0.42, w: 2.06, h: 0.30,
      fontFace: F_ZH, fontSize: 10.5, color: C.sub, margin: 0,
    });
  });

  // 3. Foundation
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 6.06, w: 7.45, h: 0.84, fill: { color: C.white }, line: { color: C.rule, width: 1.25 } });
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 6.06, w: 2.15, h: 0.84, fill: { color: C.teal }, line: { type: 'none' } });
  s.addText('基础公共层\n32 文件 / 2,066 行', {
    x: 0.55, y: 6.06, w: 2.15, h: 0.84,
    fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.white, align: 'center', valign: 'middle', margin: 0,
  });
  s.addText('枚举定义、自定义特性、全局事件总线、补间动画与通用底层基础设施', {
    x: 2.86, y: 6.06, w: 4.98, h: 0.84,
    fontFace: F_ZH, fontSize: 11.5, color: C.sub, valign: 'middle', margin: 0,
  });

  // 右 37%：瑞士技术账单 + 配置驱动说明
  addVLine(s, 8.26, 1.48, 5.42, C.rule, 1.5);
  s.addText('核心技术栈', {
    x: 8.54, y: 1.48, w: 4.24, h: 0.32,
    fontFace: F_ZH, fontSize: 15, bold: true, color: C.ink, margin: 0,
  });
  const stack = [
    ['引擎版本', 'Unity 2022.3.62f3c1'],
    ['渲染管线', 'URP 14.0.12（6 档画质）'],
    ['联机框架', 'Photon PUN 2（v2.45）'],
    ['资源加载', 'Addressables 1.22.3'],
    ['AI 寻路', 'AI Navigation 1.1.7'],
    ['图形接口', 'Direct3D 11 + TextMeshPro'],
  ];
  stack.forEach(([k, v], i) => {
    const sy = 1.92 + i * 0.42;
    addHLine(s, 8.54, sy + 0.38, 4.24, C.grid, 0.75);
    s.addText(k, {
      x: 8.54, y: sy, w: 1.25, h: 0.34,
      fontFace: F_ZH, fontSize: 11.5, bold: true, color: C.muted, valign: 'middle', margin: 0,
    });
    s.addText(v, {
      x: 9.85, y: sy, w: 2.93, h: 0.34,
      fontFace: F_NUM, fontSize: 12, bold: true, color: C.ink, align: 'right', valign: 'middle', margin: 0,
    });
  });

  s.addText('全局配置与运行调度', {
    x: 8.54, y: 4.70, w: 4.24, h: 0.32,
    fontFace: F_ZH, fontSize: 15, bold: true, color: C.coral, margin: 0,
  });
  s.addText('通过 Addressables 单例加载全局配置根资产，统一驱动对局规则、武器数值、地图清单、阵营配色与共鸣阈值配置，局内由运行时服务与总控协同分发。', {
    x: 8.54, y: 5.10, w: 4.24, h: 1.75,
    fontFace: F_ZH, fontSize: 12.2, color: C.sub, margin: 0, lineSpacingMultiple: 1.42,
  });
}

// =========================================================================
// SLIDE 8: Photon PUN 2 四通道网络同步
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '03 · 关键工程架构实现', 8);

  s.addText('按频次与语义拆分四条同步通道，兼顾带宽与断线容错', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  const channels = [
    ['01', 'IPunObservable', '连续状态流同步', '角色与机器人位姿、旋转与动作流序列化传输，客户端插值平滑消除抖动。', '高频帧同步', C.blue],
    ['02', '[PunRPC]', '瞬时动作指令（28 个）', '承担开火表现、换弹动画、受击扣血及机器人开火同步等离散动作调用。', '点对点 / 广播', C.coral],
    ['03', 'RaiseEvent', '无视图全局广播（13 码）', '涵盖准备、初同步、开局、聊天、击杀、夺旗、空投、门体及倒计时，无需挂载网络视图。', '轻量化事件', C.teal],
    ['04', 'Room Properties', '房间共享权威态', '托管全局共鸣值等房间公开状态，确保中途入场与属性回调端最终一致。', '属性回调一致', C.ink],
  ];
  channels.forEach(([num, tech, sub, desc, foot, col], i) => {
    const cx = 0.55 + i * 3.12;
    if (i > 0) addVLine(s, cx - 0.16, 1.56, 3.24, C.grid, 0.75);
    addHLine(s, cx, 1.56, 2.86, col, 2.5);
    s.addText(num, {
      x: cx, y: 1.72, w: 2.86, h: 0.34,
      fontFace: F_NUM, fontSize: 20, bold: true, color: col, margin: 0,
    });
    s.addText(tech, {
      x: cx, y: 2.10, w: 2.86, h: 0.38,
      fontFace: F_NUM, fontSize: 17.5, bold: true, color: C.ink, margin: 0,
    });
    s.addText(sub, {
      x: cx, y: 2.48, w: 2.86, h: 0.28,
      fontFace: F_ZH, fontSize: 12.5, bold: true, color: col, margin: 0,
    });
    s.addText(desc, {
      x: cx, y: 2.86, w: 2.86, h: 1.48,
      fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0, lineSpacingMultiple: 1.38,
    });
    s.addText(foot, {
      x: cx, y: 4.48, w: 2.86, h: 0.26,
      fontFace: F_ZH, fontSize: 11, bold: true, color: C.muted, margin: 0,
    });
  });

  // 下半全宽深墨黑容错带
  s.addShape(pres.ShapeType.rect, {
    x: 0.55, y: 5.06, w: 12.23, h: 1.84,
    fill: { color: C.ink }, line: { type: 'none' },
  });
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 5.06, w: 6.11, h: 0.05, fill: { color: C.blue }, line: { type: 'none' } });
  s.addShape(pres.ShapeType.rect, { x: 6.66, y: 5.06, w: 6.12, h: 0.05, fill: { color: C.coral }, line: { type: 'none' } });

  s.addText('房主迁移无缝接管（OnMasterClientSwitched）', {
    x: 0.86, y: 5.28, w: 5.50, h: 0.34,
    fontFace: F_ZH, fontSize: 14.5, bold: true, color: C.white, margin: 0,
  });
  s.addText('当房主中途意外退房时，系统在回调中自动将对局倒计时、共鸣中枢结算及场内全部 AI 机器人控制权移交新房主，保证联机对局零中断。', {
    x: 0.86, y: 5.70, w: 5.50, h: 1.00,
    fontFace: F_ZH, fontSize: 12, color: 'CBD5E1', margin: 0, lineSpacingMultiple: 1.38,
  });

  addVLine(s, 6.66, 5.30, 1.36, '334155', 1);
  s.addText('实网协议适配与离线调试分支', {
    x: 6.98, y: 5.28, w: 5.50, h: 0.34,
    fontFace: F_ZH, fontSize: 14.5, bold: true, color: C.white, margin: 0,
  });
  s.addText('采用 WebSocketSecure 协议稳定接入 Photon Cloud 云端节点，实测大厅 RTT 235–280 ms、局内 246–262 ms；无外网时支持 OfflineMode 本地单机调试。', {
    x: 6.98, y: 5.70, w: 5.50, h: 1.00,
    fontFace: F_ZH, fontSize: 12, color: 'CBD5E1', margin: 0, lineSpacingMultiple: 1.38,
  });
}

// =========================================================================
// SLIDE 9: 核心战斗：二阶 Heun 弹道与零 GC 复用
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '03 · 关键工程架构实现', 9);

  s.addText('无刚体二阶 Heun 弹道叠加预热对象池，精准判定且零 GC', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 上半：2x 瑞士工业水平流程 (1760x280 => w=12.23, h=1.95)
  s.addImage({ path: prepImg('diag_heun.png'), x: 0.55, y: 1.44, w: 12.23, h: 1.95 });

  // 下左 53%：2x2 无卡片细线十字矩阵
  s.addText('四路命中分流即时结算', {
    x: 0.55, y: 3.58, w: 6.50, h: 0.34,
    fontFace: F_ZH, fontSize: 15, bold: true, color: C.ink, margin: 0,
  });
  addHLine(s, 0.55, 3.98, 6.50, C.rule, 1.25);
  addHLine(s, 0.55, 5.42, 6.50, C.grid, 0.75);
  addVLine(s, 3.80, 4.06, 2.75, C.grid, 0.75);

  const branches = [
    ['01 / 玩家骨骼受击', '按头 / 胸 / 臂 / 腿部位倍率扣血，衔接布娃娃物理、死亡观战与 5 秒出生保护。', C.blue],
    ['02 / AI 机器人目标', '直接扣血并唤醒受击警戒状态，立即触发 AI 反击或向掩体转移。', C.ink],
    ['03 / 共鸣设施交互', '击打增幅器、抑制器或陨石设施，立即驱动全局共鸣值与环境变更。', C.coral],
    ['04 / 场景碰撞贴花', '在命中点贴出弹孔贴花后立刻回收子弹对象至池内，消除运行时 GC。', C.teal],
  ];
  branches.forEach(([t, d, col], i) => {
    const colIdx = i % 2;
    const rowIdx = Math.floor(i / 2);
    const bx = 0.55 + colIdx * 3.42;
    const by = 4.10 + rowIdx * 1.44;
    s.addText(t, {
      x: bx, y: by, w: 3.08, h: 0.28,
      fontFace: F_ZH, fontSize: 13, bold: true, color: col, margin: 0,
    });
    s.addText(d, {
      x: bx, y: by + 0.32, w: 3.08, h: 0.92,
      fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.35,
    });
  });

  // 下右 43%：1.5pt 左黑线引导的多元武器与零 GC
  addVLine(s, 7.36, 3.58, 3.32, C.rule, 1.5);
  s.addText('多元武器形态与零 GC 保障', {
    x: 7.62, y: 3.58, w: 5.16, h: 0.34,
    fontFace: F_ZH, fontSize: 15, bold: true, color: C.ink, margin: 0,
  });
  const mechs = [
    ['近战匕首', '基于 SphereCast 体积扫描判伤，贴身近战稳定。', C.blue],
    ['手雷与燃烧瓶', '抛物线物理投掷，燃烧瓶落地生成持续燃烧区。', C.ink],
    ['榴弹发射器', '范围爆炸叠加障碍物遮挡射线校验，防隔墙误伤。', C.ink],
    ['帧控与零 GC 实测', '统一配置出生保护 5s、复活 7s 与掉枪；配合集中帧调度器实现连续 20 发开火 Gen0 GC = 0。', C.coral],
  ];
  mechs.forEach(([t, d, col], i) => {
    const my = 4.04 + i * 0.72;
    s.addText(t, {
      x: 7.62, y: my, w: 5.16, h: 0.26,
      fontFace: F_ZH, fontSize: 12.5, bold: true, color: col, margin: 0,
    });
    s.addText(d, {
      x: 7.62, y: my + 0.26, w: 5.16, h: 0.42,
      fontFace: F_ZH, fontSize: 11.2, color: C.sub, margin: 0, lineSpacingMultiple: 1.25,
    });
  });
}

// =========================================================================
// SLIDE 10: 机器人 AI：七态状态机与掩体调度
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '03 · 关键工程架构实现', 10);

  s.addText('房主权威七态有限状态机，联动 30 处掩体实现拟真对抗', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 左 47%：三大行为模式 + 七态 FSM 图 + 掩体与分工
  s.addText('三大行为模式', {
    x: 0.55, y: 1.48, w: 1.40, h: 0.38,
    fontFace: F_ZH, fontSize: 12.5, bold: true, color: C.ink, valign: 'middle', margin: 0,
  });
  const aiModes = [
    ['激进型', C.coral, C.white],
    ['被动型', C.subtle, C.ink],
    ['保护型', C.blue, C.white],
  ];
  aiModes.forEach(([m, bg, tc], i) => {
    const mx = 2.05 + i * 1.42;
    s.addShape(pres.ShapeType.rect, { x: mx, y: 1.48, w: 1.28, h: 0.38, fill: { color: bg }, line: { color: C.rule, width: 0.75 } });
    s.addText(m, {
      x: mx, y: 1.48, w: 1.28, h: 0.38,
      fontFace: F_ZH, fontSize: 11.5, bold: true, color: tc, align: 'center', valign: 'middle', margin: 0,
    });
  });

  // 2x 七态 FSM 图 (880x420 => w=5.75, h=2.74)
  s.addImage({ path: prepImg('diag_fsm.png'), x: 0.55, y: 2.02, w: 5.75, h: 2.74 });

  addHLine(s, 0.55, 4.94, 5.75, C.rule, 1.25);
  s.addText('掩体占用与冷却配对调度', {
    x: 0.55, y: 5.04, w: 5.75, h: 0.28,
    fontFace: F_ZH, fontSize: 13, bold: true, color: C.blue, margin: 0,
  });
  s.addText('30 处掩体节点采用「占用 + 冷却」成对机制，避免多名机器人扎堆抢点，提升分散交火真实感。', {
    x: 0.55, y: 5.34, w: 5.75, h: 0.56,
    fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.3,
  });

  addHLine(s, 0.55, 5.96, 5.75, C.grid, 0.75);
  s.addText('房主权威寻路与全端动作同步', {
    x: 0.55, y: 6.06, w: 5.75, h: 0.28,
    fontFace: F_ZH, fontSize: 13, bold: true, color: C.coral, margin: 0,
  });
  s.addText('仅房主端执行 NavMesh 寻路与状态决策；其余端同步位姿插值并通过 [PunRPC] 同步开火与换弹。', {
    x: 0.55, y: 6.36, w: 5.75, h: 0.56,
    fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.3,
  });

  // 右 50%：精裁地图 NavMesh (1100x840 => w=6.18, h=4.72)
  addFramedPhoto(s, prepImg('ai_map_crop.png'), 6.60, 1.48, 6.18, 4.72);
  s.addShape(pres.ShapeType.rect, { x: 6.60, y: 6.20, w: 6.18, h: 0.72, fill: { color: C.ink }, line: { type: 'none' } });
  s.addText('「小镇」烘焙 NavMesh 可行走网格（蓝）与 30 处掩体转移拓扑（红）\n支持蓝/红阵营补位及第三阵营中立机器人并发交火', {
    x: 6.80, y: 6.22, w: 5.80, h: 0.68,
    fontFace: F_ZH, fontSize: 11, color: C.bg, valign: 'middle', margin: 0, lineSpacingMultiple: 1.28,
  });
}

// =========================================================================
// SLIDE 11: URP 双相机防穿墙与后处理
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '03 · 关键工程架构实现', 11);

  s.addText('URP Base + Overlay 双相机栈，根治第一人称持枪穿墙', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 左 53%：持枪实机画面 (6.45 x 3.63) + 黑底说明 + 底部总结条
  addFramedPhoto(s, rawImg('fps_gun.png'), 0.55, 1.48, 6.45, 3.63);
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 5.11, w: 6.45, h: 0.62, fill: { color: C.ink }, line: { type: 'none' } });
  s.addText('近墙持枪实测：主相机（深度 1 · Base）渲染地形天空 + 武器相机（深度 2 · Overlay）叠加枪模', {
    x: 0.74, y: 5.11, w: 6.08, h: 0.62,
    fontFace: F_ZH, fontSize: 11, bold: true, color: C.bg, valign: 'middle', margin: 0,
  });
  s.addText('实测彻底解决贴近墙面与掩体时第一人称枪械模型被场景裁剪穿模的问题，同时保留完整阴影与 HUD 叠加。', {
    x: 0.55, y: 5.88, w: 6.45, h: 0.90,
    fontFace: F_ZH, fontSize: 12.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.4,
  });

  // 右 43%：3 段无框瑞士大号编号解析
  addVLine(s, 7.30, 1.48, 5.42, C.rule, 1.5);
  const rItems = [
    ['01', '双相机栈与前景防穿墙着色器', '双阵营玩家预制体均将武器 Overlay 相机挂载至主相机 Camera Stack，配合自研前景着色器确保第一人称持枪深度优先且光照协调。', C.blue],
    ['02', '屏幕深度水体与 6 档画质分级', '河道水体基于屏幕坐标与深度图差值计算水深，叠双层反向滚动法线呈现水波；6 档画质分别绑定性能、平衡与高画质 URP 管线资产。', C.teal],
    ['03', '动态后处理与反射开关控制', '局内运行时按需开关 Bloom、色差与动态模糊，并通过反射读取 URP 渲染器特征列表实现 SSAO 实时启停。', C.coral],
  ];
  rItems.forEach(([num, t, d, col], i) => {
    const ry = 1.48 + i * 1.84;
    if (i > 0) addHLine(s, 7.58, ry - 0.14, 5.20, C.grid, 0.75);
    s.addText(num, {
      x: 7.58, y: ry, w: 0.65, h: 0.44,
      fontFace: F_NUM, fontSize: 26, bold: true, color: col, margin: 0,
    });
    s.addText(t, {
      x: 8.28, y: ry + 0.04, w: 4.50, h: 0.34,
      fontFace: F_ZH, fontSize: 15, bold: true, color: C.ink, margin: 0,
    });
    s.addText(d, {
      x: 8.28, y: ry + 0.46, w: 4.50, h: 1.15,
      fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0, lineSpacingMultiple: 1.38,
    });
  });
}

// =========================================================================
// SLIDE 12: Photon 云端多端联机实测
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '04 · 系统测试与性能分析', 12);

  s.addText('接入 Photon 云端实网测试，六大核心联机场景全部通过', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 顶部 4 列无框环境标尺
  addHLine(s, 0.55, 1.44, 12.23, C.rule, 1.25);
  const envCols = [
    ['实网接入节点', 'Photon Cloud（WSS）', C.ink],
    ['局内稳定延迟', 'RTT 246–262 ms', C.ink],
    ['测试平台画质', 'Direct3D 11 · Ultra', C.ink],
    ['核心场景验证', '6 / 6 全部通过', C.blue],
  ];
  envCols.forEach(([k, v, col], i) => {
    const ex = 0.55 + i * 3.10;
    if (i > 0) addVLine(s, ex - 0.16, 1.54, 0.68, C.grid, 0.75);
    s.addText(k, {
      x: ex, y: 1.54, w: 2.86, h: 0.24,
      fontFace: F_ZH, fontSize: 10.5, bold: true, color: C.muted, margin: 0,
    });
    s.addText(v, {
      x: ex, y: 1.80, w: 2.86, h: 0.38,
      fontFace: F_NUM, fontSize: 16.5, bold: true, color: col, margin: 0,
    });
  });

  // 6 行瑞士横线验证表（编号 01~06，无 TC-）
  const tcRows = [
    ['编号', '验证场景', '实测操作条件', '实测观测结果', '结论'].map((t) => ({
      text: t,
      options: { bold: true, color: C.bg, fill: { color: C.ink }, align: 'center', fontSize: 11.5 },
    })),
    ['01', '大厅建房与切图', '主菜单创建 6 人小镇团队房并开局', '大厅 RTT 235–280 ms，自动分队并载入小镇地图', '通过'],
    ['02', '共鸣重力全端同步', '触发共鸣接口推高数值至 160 与 1000', '房间属性全端一致；跳跃初速由 5.00 升至 5.96 与 11.00', '通过'],
    ['03', '地面火海同步启停', '越过阈值 150 至 350，再压低至 100', '46 处火点全开无色块遮挡；降至 100 后全部销毁', '通过'],
    ['04', '战区空投与中立 AI', '共鸣依次跨越步长 200 与 1000', '触发 2 次空投事件；刷出第三阵营中立 AI（5→6 名）', '通过'],
    ['05', 'AI 状态切换与寻路', '小镇 30 处掩体环境下观测 5–6 名 AI', '搜寻与警戒状态实时巡检切换并正常交火击杀', '通过'],
    ['06', '对象池连续射击', '主武器自动步枪连续触发 20 次单发', '20 次弹道推进与射线判定正常，Gen0 GC 增量 = 0', '通过'],
  ].map((row, rIdx) => {
    if (rIdx === 0) return row;
    return row.map((cell, cIdx) => ({
      text: cell,
      options: {
        fill: { color: rIdx % 2 === 1 ? C.white : C.bg },
        color: cIdx === 4 ? C.teal : (cIdx <= 1 ? C.ink : C.sub),
        bold: cIdx === 0 || cIdx === 4,
        align: cIdx === 0 || cIdx === 4 ? 'center' : 'left',
        fontSize: 11,
      },
    }));
  });

  s.addTable(tcRows, {
    x: 0.55, y: 2.38, w: 12.23, h: 4.52,
    colW: [0.78, 1.95, 3.85, 4.75, 0.90],
    rowH: [0.46, 0.67, 0.67, 0.67, 0.67, 0.67, 0.67],
    border: [
      { pt: 1.25, color: C.rule },
      { pt: 0, color: C.bg },
      { pt: 0.75, color: C.grid },
      { pt: 0, color: C.bg },
    ],
    fontFace: F_ZH,
    valign: 'middle',
    margin: [4, 8, 4, 8],
  });
}

// =========================================================================
// SLIDE 13: URP 与高共鸣并发性能评测
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '04 · 系统测试与性能分析', 13);

  s.addText('46 处火海并发稳居 67.7 FPS，360 帧采样实现零 Gen0 GC', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  // 左上：原生可编辑柱状图（白底无圆角，1px 细黑框）
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 1.44, w: 7.15, h: 3.32, fill: { color: C.white }, line: { color: C.rule, width: 1 } });
  s.addChart(pres.ChartType.bar, [
    {
      name: '平均帧率 (FPS)',
      labels: ['待机全特效', '高共鸣全特效', '高共鸣关后处理'],
      values: [87.7, 67.7, 55.6],
    },
    {
      name: '1% 低帧率 (FPS)',
      labels: ['待机全特效', '高共鸣全特效', '高共鸣关后处理'],
      values: [69.4, 39.7, 37.8],
    },
  ], {
    x: 0.68, y: 1.52, w: 6.90, h: 3.16,
    barDir: 'col',
    barGrouping: 'clustered',
    chartColors: [C.blue, C.coral],
    showLegend: true,
    legendPos: 't',
    legendFontFace: F_ZH,
    legendFontSize: 10.5,
    showValue: true,
    dataLabelFormatCode: '0.0',
    dataLabelFontFace: F_NUM,
    dataLabelFontSize: 10,
    dataLabelColor: C.ink,
    catAxisLabelFontFace: F_ZH,
    catAxisLabelFontSize: 10.5,
    catAxisLabelColor: C.ink,
    valAxisLabelFontFace: F_NUM,
    valAxisLabelFontSize: 9.5,
    valAxisLabelColor: C.muted,
    valAxisMaxVal: 100,
    valGridLine: { color: C.grid, size: 0.5 },
    catGridLine: { style: 'none' },
  });

  // 左下：4 行瑞士横线核心对照表（无 W1/W2/W3）
  const perfRows = [
    ['性能指标', '待机全特效', '高共鸣全特效', '高共鸣关后处理'].map((t) => ({
      text: t,
      options: { bold: true, color: C.bg, fill: { color: C.ink }, align: 'center', fontSize: 10.5 },
    })),
    ['平均帧耗时', '11.40 ms', '14.77 ms', '18.00 ms'],
    ['Draw Call / Batches', '218.0 / 218.0', '236.5 / 227.4', '199.4 / 191.5'],
    ['已分配总内存', '729.4 MB', '743.8 MB', '747.4 MB'],
    ['Gen0 GC 增量', '0 次', '0 次', '0 次'],
  ].map((row, rIdx) => {
    if (rIdx === 0) return row;
    return row.map((cell, cIdx) => ({
      text: cell,
      options: {
        fill: { color: rIdx % 2 === 1 ? C.white : C.bg },
        color: rIdx === 4 && cIdx > 0 ? C.teal : (cIdx === 0 ? C.ink : C.sub),
        bold: cIdx === 0 || rIdx === 4,
        align: cIdx === 0 ? 'left' : 'center',
        fontSize: 10.5,
      },
    }));
  });
  s.addTable(perfRows, {
    x: 0.55, y: 4.90, w: 7.15, h: 1.98,
    colW: [1.95, 1.65, 1.75, 1.80],
    rowH: [0.38, 0.40, 0.40, 0.40, 0.40],
    border: [
      { pt: 1.25, color: C.rule },
      { pt: 0, color: C.bg },
      { pt: 0.75, color: C.grid },
      { pt: 0, color: C.bg },
    ],
    fontFace: F_ZH,
    valign: 'middle',
    margin: [2, 6, 2, 6],
  });

  // 右 38%：1.5pt 竖黑线分隔 + 3 段无框分析
  addVLine(s, 7.98, 1.44, 5.44, C.rule, 1.5);
  const perfNotes = [
    ['高负载流畅达标', '从待机切入 46 处火海与 AI 连射交火状态，Draw Call 仅从 218.0 升至 236.5，平均帧耗时增加 3.37 ms，平均帧率稳居 67.7 FPS。', C.blue],
    ['零 GC 内存平稳', '3 组共 360 帧采样期内 Gen0 垃圾回收增量全部保持 0 次，已分配总内存仅静态增长 14.4 MB，证明对象池与集中调度有效。', C.teal],
    ['关后处理工况复盘', '关闭后处理使 Draw Call 降 15.7%、三角面数降 23.7%；该次联机采样恰好遇到 AI 密集寻路重算拉低主线程，排除 AI 波动对照采样中帧率由 62.3 升至 67.0 FPS。', C.coral],
  ];
  perfNotes.forEach(([t, d, col], i) => {
    const ny = 1.46 + i * 1.84;
    if (i > 0) addHLine(s, 8.26, ny - 0.14, 4.52, C.grid, 0.75);
    s.addText(t, {
      x: 8.26, y: ny, w: 4.52, h: 0.34,
      fontFace: F_ZH, fontSize: 15, bold: true, color: col, margin: 0,
    });
    s.addText(d, {
      x: 8.26, y: ny + 0.42, w: 4.52, h: 1.24,
      fontFace: F_ZH, fontSize: 12, color: C.sub, margin: 0, lineSpacingMultiple: 1.38,
    });
  });
}

// =========================================================================
// SLIDE 14: 团队成员分工与协作模式
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '05 · 团队分工与演进方向', 14);

  s.addText('三人职责边界清晰，契约与配置解耦驱动高效集成', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  const members = [
    [
      '吴凯东', '程序框架设计', C.ink,
      [
        ['三层程序集架构', '规划基础、运行时、编辑器三层划分与 Addressables 配置加载体系。'],
        ['联机同步基座', '封装 Photon PUN 2 四通道同步、网络事件协议及房主迁移接管机制。'],
        ['渲染与工具链', '实现 URP 双相机防穿墙、集中帧调度、预热对象池及 15 个编辑器扩展。'],
      ],
    ],
    [
      '杨世杰', '业务逻辑实现', C.blue,
      [
        ['对局与模式控制', '实现团队、混战、夺旗三模式控制器、准备厅及局内计分与复活流程。'],
        ['共鸣核心与响应', '开发房主端共鸣结算中枢，落地重力、火海、空投、中立 AI 与陨石五类剧变。'],
        ['战斗与 AI 系统', '实现二阶 Heun 弹道、八武器与骨骼受击，以及七态 AI 状态机与掩体调度。'],
      ],
    ],
    [
      '周欣琦', '美术、策划、测试', C.coral,
      [
        ['关卡与视觉表现', '搭建小镇对战地图与烘焙 NavMesh，制作水体/火海粒子及大厅 HUD 界面。'],
        ['数值与玩法策划', '主导四兵种八武器数值平衡，设计共鸣权重表与五类环境阈值梯度。'],
        ['联机与性能测试', '执行 Photon 云端六大场景验证与 360 帧性能采样，推进缺陷回归闭环。'],
      ],
    ],
  ];

  members.forEach(([name, role, col, items], i) => {
    const mx = 0.55 + i * 4.18;
    if (i > 0) addVLine(s, mx - 0.18, 1.48, 4.16, C.grid, 0.75);
    addHLine(s, mx, 1.48, 3.86, col, 3.5);
    s.addText(name, {
      x: mx, y: 1.66, w: 1.75, h: 0.44,
      fontFace: F_ZH, fontSize: 22, bold: true, color: C.ink, margin: 0,
    });
    s.addText(role, {
      x: mx + 1.80, y: 1.74, w: 2.06, h: 0.34,
      fontFace: F_ZH, fontSize: 12.5, bold: true, color: col, align: 'right', valign: 'middle', margin: 0,
    });
    addHLine(s, mx, 2.22, 3.86, C.grid, 0.75);

    items.forEach(([itTitle, itDesc], j) => {
      const iy = 2.40 + j * 1.12;
      s.addText(itTitle, {
        x: mx, y: iy, w: 3.86, h: 0.28,
        fontFace: F_ZH, fontSize: 13, bold: true, color: col, margin: 0,
      });
      s.addText(itDesc, {
        x: mx, y: iy + 0.32, w: 3.86, h: 0.70,
        fontFace: F_ZH, fontSize: 11.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.32,
      });
    });
  });

  // 底部 1.5pt 黑线协作链
  addHLine(s, 0.55, 5.86, 12.23, C.rule, 1.5);
  const collab = [
    ['01 / 接口契约先行', '网络事件码与配置根结构先定稿，程序并行开发零冲突'],
    ['02 / 配置零代码并行', '策划与美术直接调优 31 份资产与配置中心，免等脚本改动'],
    ['03 / 云端与离线双轨闭环', '外网 Photon 云端联机实测 + 离线单机快速回归，问题当日闭环'],
  ];
  collab.forEach(([ct, cd], i) => {
    const cx = 0.55 + i * 4.18;
    if (i > 0) addVLine(s, cx - 0.18, 6.00, 0.88, C.grid, 0.75);
    s.addText(ct, {
      x: cx, y: 6.00, w: 3.86, h: 0.30,
      fontFace: F_ZH, fontSize: 13, bold: true, color: C.ink, margin: 0,
    });
    s.addText(cd, {
      x: cx, y: 6.34, w: 3.86, h: 0.54,
      fontFace: F_ZH, fontSize: 11.2, color: C.sub, margin: 0, lineSpacingMultiple: 1.28,
    });
  });
}

// =========================================================================
// SLIDE 15: 项目总结与后续演进（上双栏无框 + 下全出血深墨黑致谢带）
// =========================================================================
{
  const s = pres.addSlide();
  addMasthead(s, '05 · 团队分工与演进方向', 15);

  s.addText('项目总结与后续演进方向', {
    x: 0.55, y: 0.72, w: 12.23, h: 0.58,
    fontFace: F_ZH, fontSize: 28, bold: true, color: C.ink, margin: 0,
  });

  addHLine(s, 0.55, 1.48, 12.23, C.rule, 1.25);
  addVLine(s, 6.66, 1.62, 2.75, C.grid, 1);

  // 左：项目总结
  s.addText('01 / 项目工程总结', {
    x: 0.55, y: 1.64, w: 5.85, h: 0.34,
    fontFace: F_ZH, fontSize: 16, bold: true, color: C.blue, margin: 0,
  });
  s.addText('• 完整交付可实网联机运行的 Unity URP + Photon PUN 2 多人 FPS 工程（310 脚本 / 44,039 行代码）。\n• 以「全局共鸣中枢」实现差异化玩法，五类战场环境剧变在多客户端一致运行。\n• 通过预热对象池、集中帧调度与数据驱动配置，兼顾高负载 67.7 FPS / 零 Gen0 GC 与团队高效协作。', {
    x: 0.55, y: 2.08, w: 5.85, h: 2.24,
    fontFace: F_ZH, fontSize: 12.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.48,
  });

  // 右：后续演进方向
  s.addText('02 / 后续演进方向', {
    x: 6.94, y: 1.64, w: 5.84, h: 0.34,
    fontFace: F_ZH, fontSize: 16, bold: true, color: C.coral, margin: 0,
  });
  s.addText('• 网络体验升级：引入客户端预测与服务端延迟补偿，降低高 RTT 环境下的命中判定延迟。\n• 内容与模式扩展：扩充多张主题对战地图，启用预留的点射连发武器及自定义共鸣规则。\n• 专用服务器演进：探索 Dedicated Server 替代房主权威，提升反作弊能力与大房间承载上限。', {
    x: 6.94, y: 2.08, w: 5.84, h: 2.24,
    fontFace: F_ZH, fontSize: 12.5, color: C.sub, margin: 0, lineSpacingMultiple: 1.48,
  });

  // 下方：全出血深墨黑致谢主带
  s.addShape(pres.ShapeType.rect, {
    x: 0, y: 4.62, w: 13.333, h: 2.88,
    fill: { color: C.ink }, line: { type: 'none' },
  });
  s.addShape(pres.ShapeType.rect, {
    x: 0, y: 4.62, w: 13.333, h: 0.06,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('感谢各位老师聆听 · 敬请批评指正', {
    x: 0.72, y: 5.04, w: 11.88, h: 0.74,
    fontFace: F_ZH, fontSize: 34, bold: true, color: C.white, margin: 0,
  });
  s.addText('《共鸣之境》项目组：吴凯东 · 杨世杰 · 周欣琦　　|　　指导教师：黄取治', {
    x: 0.72, y: 5.96, w: 11.88, h: 0.42,
    fontFace: F_ZH, fontSize: 14, color: 'CBD5E1', margin: 0,
  });
  s.addText('福建师范大学协和学院 · 信息技术系 · 数字媒体技术专业　·　2026 年 11 月', {
    x: 0.72, y: 6.50, w: 11.88, h: 0.34,
    fontFace: F_ZH, fontSize: 11.5, color: '9CA3AF', margin: 0,
  });
}

pres.writeFile({ fileName: OUT_FILE }).then(() => {
  console.log('Successfully generated:', OUT_FILE);
}).catch((err) => {
  console.error('Failed to generate PPTX:', err);
  process.exit(1);
});
