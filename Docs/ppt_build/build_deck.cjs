const path = require('path');
let PptxGenJS;
try {
  PptxGenJS = require('pptxgenjs');
} catch {
  PptxGenJS = require('C:/Users/wukaidong/AppData/Roaming/npm/node_modules/pptxgenjs');
}

const IMG_DIR = 'e:/1FairyOnTheTree/The-realm-of-resonance/Docs/论文/草稿/图片';
const OUT_FILE = 'e:/1FairyOnTheTree/The-realm-of-resonance/Docs/共鸣之境_实训结题答辩.pptx';

const C = {
  bg: 'F6F7FB',
  card: 'FFFFFF',
  border: 'DDE3EE',
  imgBorder: 'CBD5E1',
  subtle: 'EEF2F9',
  subtle2: 'F8FAFC',
  navy: '141E38',
  body: '334155',
  muted: '64748B',
  blue: '1D4ED8',
  blueBg: 'DBEAFE',
  coral: 'E85D3F',
  coralBg: 'FEECE8',
  teal: '0F766E',
  tealBg: 'CCFBF1',
  white: 'FFFFFF',
  slateLight: '94A3B8',
  slateMid: 'CBD5E1',
};

const FONT = 'Microsoft YaHei';
const CODE_FONT = 'Consolas';

const pres = new PptxGenJS();
pres.layout = 'LAYOUT_WIDE'; // 13.333 x 7.5 in
pres.author = '吴凯东 · 杨世杰 · 周欣琦';
pres.company = '福建师范大学协和学院 信息技术系 数字媒体技术';
pres.subject = '基于 Unity 的联机游戏《共鸣之境》实训结题答辩';
pres.title = '《共鸣之境》实训结题答辩';

function imgPath(name) {
  return path.join(IMG_DIR, name);
}

function addHeader(slide, section, title) {
  slide.background = { color: C.bg };
  slide.addText(section, {
    x: 0.55, y: 0.26, w: 12.23, h: 0.24,
    fontFace: FONT, fontSize: 10, bold: true, color: C.blue,
    margin: 0, valign: 'middle',
  });
  slide.addText(title, {
    x: 0.55, y: 0.50, w: 12.23, h: 0.48,
    fontFace: FONT, fontSize: 23, bold: true, color: C.navy,
    margin: 0, valign: 'middle',
  });
  slide.addShape(pres.ShapeType.line, {
    x: 0.55, y: 1.04, w: 12.23, h: 0,
    line: { color: C.border, width: 1 },
  });
}

function addFooter(slide, pageNum, total = 15) {
  slide.addShape(pres.ShapeType.line, {
    x: 0.55, y: 7.06, w: 12.23, h: 0,
    line: { color: C.border, width: 0.75 },
  });
  slide.addText('《共鸣之境》 · Unity 联机 FPS 实训结题答辩', {
    x: 0.55, y: 7.12, w: 8.5, h: 0.25,
    fontFace: FONT, fontSize: 9, color: C.muted, margin: 0, valign: 'middle',
  });
  slide.addText(`${String(pageNum).padStart(2, '0')} / ${total}`, {
    x: 11.4, y: 7.12, w: 1.38, h: 0.25,
    fontFace: CODE_FONT, fontSize: 9.5, bold: true, color: C.navy,
    align: 'right', margin: 0, valign: 'middle',
  });
}

function addCard(slide, x, y, w, h, opts = {}) {
  slide.addShape(pres.ShapeType.roundRect, {
    x, y, w, h,
    rectRadius: opts.radius ?? 0.04,
    fill: { color: opts.fill || C.card },
    line: { color: opts.line || C.border, width: opts.lineW ?? 1 },
  });
  if (opts.topBarColor) {
    slide.addShape(pres.ShapeType.rect, {
      x: x + 0.01, y: y + 0.01, w: w - 0.02, h: opts.topBarH || 0.06,
      fill: { color: opts.topBarColor },
      line: { type: 'none' },
    });
  }
}

function addBadge(slide, x, y, w, h, text, fillColor, textColor, fontSize = 9.5) {
  slide.addShape(pres.ShapeType.roundRect, {
    x, y, w, h,
    rectRadius: 0.12,
    fill: { color: fillColor },
    line: { type: 'none' },
  });
  slide.addText(text, {
    x, y, w, h,
    fontFace: FONT, fontSize, bold: true, color: textColor,
    align: 'center', valign: 'middle', margin: 0,
  });
}

function addFramedImage(slide, file, x, y, w, h) {
  slide.addShape(pres.ShapeType.rect, {
    x: x - 0.015, y: y - 0.015, w: w + 0.03, h: h + 0.03,
    fill: { color: C.white },
    line: { color: C.imgBorder, width: 1 },
  });
  slide.addImage({
    path: imgPath(file),
    x, y, w, h,
  });
}

// =========================================================================
// SLIDE 1: 封面（左右非对称编辑部版式）
// =========================================================================
{
  const s = pres.addSlide();
  s.background = { color: C.bg };

  // 左侧装饰条与标签
  s.addShape(pres.ShapeType.rect, {
    x: 0.60, y: 0.68, w: 0.90, h: 0.07,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('UNITY 联机第一人称射击游戏 · 实训结题答辩', {
    x: 0.60, y: 0.88, w: 5.95, h: 0.28,
    fontFace: FONT, fontSize: 11, bold: true, color: C.blue, margin: 0,
  });
  s.addText('《共鸣之境》', {
    x: 0.52, y: 1.28, w: 6.10, h: 0.90,
    fontFace: FONT, fontSize: 44, bold: true, color: C.navy, margin: 0,
  });
  s.addText('THE REALM OF RESONANCE', {
    x: 0.60, y: 2.18, w: 5.95, h: 0.30,
    fontFace: CODE_FONT, fontSize: 12.5, bold: true, color: C.muted,
    charSpacing: 3, margin: 0,
  });
  s.addText('全局共鸣中枢驱动的动态战场联机 FPS 工程实现', {
    x: 0.60, y: 2.60, w: 5.95, h: 0.40,
    fontFace: FONT, fontSize: 14, color: C.body, margin: 0,
  });

  s.addShape(pres.ShapeType.line, {
    x: 0.60, y: 3.18, w: 5.85, h: 0,
    line: { color: C.border, width: 1 },
  });

  // 署名信息卡
  addCard(s, 0.60, 3.40, 5.85, 3.42, { fill: C.white, line: C.border });
  const metaRows = [
    ['院校专业', '福建师范大学协和学院 · 信息技术系 · 数字媒体技术'],
    ['小组成员', '吴凯东（程序框架设计）\n杨世杰（业务逻辑实现）\n周欣琦（美术、策划、测试）'],
    ['指导教师', '黄取治'],
    ['汇报日期', '2026 年 11 月'],
  ];
  let my = 3.58;
  metaRows.forEach(([k, v], idx) => {
    const rowH = idx === 1 ? 0.96 : 0.42;
    s.addText(k, {
      x: 0.82, y: my, w: 1.05, h: rowH,
      fontFace: FONT, fontSize: 10.5, bold: true, color: C.muted,
      valign: 'top', margin: 0,
    });
    s.addText(v, {
      x: 1.92, y: my, w: 4.35, h: rowH,
      fontFace: FONT, fontSize: 11, bold: idx === 1 ? false : true, color: C.navy,
      valign: 'top', margin: 0, lineSpacingMultiple: 1.28,
    });
    my += rowH + 0.16;
    if (idx < metaRows.length - 1) {
      s.addShape(pres.ShapeType.line, {
        x: 0.82, y: my - 0.09, w: 5.42, h: 0,
        line: { color: C.subtle, width: 0.75 },
      });
    }
  });

  // 右侧主视觉图 (1281x716 => 5.95 / 1.789 = 3.326)
  addFramedImage(s, 'shot_4_2_ai_navmesh_cover.png', 6.78, 0.68, 5.95, 3.33);
  s.addShape(pres.ShapeType.rect, {
    x: 6.78, y: 4.08, w: 5.95, h: 0.44,
    fill: { color: C.navy }, line: { type: 'none' },
  });
  s.addText('OldQuarter 对战地图全景 · 烘焙 NavMesh 与 30 处 AI 掩体网络', {
    x: 6.94, y: 4.08, w: 5.65, h: 0.44,
    fontFace: FONT, fontSize: 10.5, color: C.white, valign: 'middle', margin: 0,
  });

  // 右下 3 格工程指标条
  const coverStats = [
    ['Unity 2022.3\nURP 14 渲染', '通用渲染管线与双相机栈'],
    ['Photon PUN 2\n四通道联机', '高频流同步与房主迁移'],
    ['310 个脚本\n44,039 行代码', '三程序集高内聚工程结构'],
  ];
  coverStats.forEach(([t, sub], i) => {
    const cx = 6.78 + i * 2.02;
    addCard(s, cx, 4.74, 1.91, 2.08, {
      fill: C.white, line: C.border,
      topBarColor: i === 1 ? C.coral : C.blue, topBarH: 0.05,
    });
    s.addText(t, {
      x: cx + 0.14, y: 4.98, w: 1.63, h: 0.72,
      fontFace: FONT, fontSize: 12, bold: true, color: C.navy,
      valign: 'top', margin: 0, lineSpacingMultiple: 1.25,
    });
    s.addText(sub, {
      x: cx + 0.14, y: 5.78, w: 1.63, h: 0.82,
      fontFace: FONT, fontSize: 10, color: C.body,
      valign: 'top', margin: 0, lineSpacingMultiple: 1.25,
    });
  });
}

// =========================================================================
// SLIDE 2: 汇报目录与工程全貌
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '00 / 汇报概览', '以全局「共鸣」驱动战场演化，完成联机 FPS 全链路工程');
  addFooter(s, 2);

  const toc = [
    ['01', '项目背景与玩法构成', '破解中后期僵局痛点，构建三模式、四兵种与八武器表驱动体系'],
    ['02', '全局共鸣核心机制', '房主权威结算 rr.resonance，跨阈值同步触发五类战场剧变'],
    ['03', '关键工程架构实现', '三程序集分层、PUN 2 四通道、Heun 弹道、AI 与 URP 双相机渲染'],
    ['04', '系统联机与性能实测', 'Photon 云端 6 项功能用例全过，3 工况 360 帧实测零 Gen0 GC'],
    ['05', '团队分工与总结展望', '三人职责边界、契约与配置解耦协作及后续演进方向'],
  ];
  toc.forEach(([num, title, desc], i) => {
    const y = 1.26 + i * 1.12;
    addCard(s, 0.55, y, 7.20, 0.96, { fill: C.white, line: C.border });
    addBadge(s, 0.75, y + 0.22, 0.62, 0.52, num, i === 1 ? C.coral : C.navy, C.white, 13);
    s.addText(title, {
      x: 1.54, y: y + 0.14, w: 6.00, h: 0.34,
      fontFace: FONT, fontSize: 14.5, bold: true, color: C.navy, margin: 0,
    });
    s.addText(desc, {
      x: 1.54, y: y + 0.50, w: 6.00, h: 0.34,
      fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0,
    });
  });

  // 右侧 2x2 关键指标卡
  const kpis = [
    ['310', '一方脚本文件', '44,039 行 C# 代码\nFoundation / Runtime / Editor', C.navy],
    ['8 · 4 · 3', '武器 · 兵种 · 模式', '85 个预制体与 31 个配置资产\n统一由 ProjectConfigRoot 统管', C.blue],
    ['28 · 13', 'RPC · 自定义事件码', '四通道联机同步架构\n支持房主离线迁移无缝接管', C.teal],
    ['67.7 FPS', '高共鸣并发平均帧率', '46 处火海与 AI 交火实测\n360 帧内 Gen0 GC 增量 = 0', C.coral],
  ];
  kpis.forEach(([val, lbl, sub, barColor], i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const x = 8.02 + col * 2.42;
    const y = 1.26 + row * 2.84;
    addCard(s, x, y, 2.28, 2.60, { fill: C.white, line: C.border, topBarColor: barColor, topBarH: 0.06 });
    s.addText(val, {
      x: x + 0.18, y: y + 0.26, w: 1.92, h: 0.58,
      fontFace: CODE_FONT, fontSize: 25, bold: true, color: barColor, margin: 0,
    });
    s.addText(lbl, {
      x: x + 0.18, y: y + 0.90, w: 1.92, h: 0.36,
      fontFace: FONT, fontSize: 12, bold: true, color: C.navy, margin: 0,
    });
    s.addText(sub, {
      x: x + 0.18, y: y + 1.36, w: 1.92, h: 1.06,
      fontFace: FONT, fontSize: 10, color: C.body, margin: 0, lineSpacingMultiple: 1.3,
    });
  });
}

// =========================================================================
// SLIDE 3: 项目背景与设计定位
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '01 / 项目背景与玩法设计', '破解联机 FPS 中后期交火点固化，让每次交火改写战场规则');
  addFooter(s, 3);

  // 左侧：痛点 -> 中枢 -> 破局
  addCard(s, 0.55, 1.26, 5.45, 2.02, { fill: C.white, line: C.border, topBarColor: C.muted, topBarH: 0.06 });
  addBadge(s, 0.78, 1.48, 1.25, 0.28, '行业痛点', C.subtle, C.navy, 9.5);
  s.addText('传统联机 FPS 中后期易陷入阵地僵局', {
    x: 0.78, y: 1.84, w: 5.00, h: 0.34,
    fontFace: FONT, fontSize: 14, bold: true, color: C.navy, margin: 0,
  });
  s.addText('对局进入中后期，双方玩家往往熟悉固定掩体与卡点位置，交火点位趋于固化，攻防节奏单一，缺乏突发变量打破平衡。', {
    x: 0.78, y: 2.24, w: 5.00, h: 0.88,
    fontFace: FONT, fontSize: 11, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
  });

  // 向下箭头 + 核心解法徽章
  s.addShape(pres.ShapeType.downArrow, {
    x: 3.05, y: 3.38, w: 0.45, h: 0.36,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  addBadge(s, 1.38, 3.82, 3.80, 0.42, '核心解法：全局共鸣值 rr.resonance', C.coralBg, C.coral, 11);
  s.addShape(pres.ShapeType.downArrow, {
    x: 3.05, y: 4.32, w: 0.45, h: 0.36,
    fill: { color: C.blue }, line: { type: 'none' },
  });

  addCard(s, 0.55, 4.76, 5.45, 2.08, { fill: C.white, line: C.border, topBarColor: C.blue, topBarH: 0.06 });
  addBadge(s, 0.78, 4.98, 1.25, 0.28, '动态破局', C.blueBg, C.blue, 9.5);
  s.addText('对局行为实时改写战场物理与环境条件', {
    x: 0.78, y: 5.34, w: 5.00, h: 0.34,
    fontFace: FONT, fontSize: 14, bold: true, color: C.navy, margin: 0,
  });
  s.addText('命中、误伤、击打共鸣设施与夺旗实时改变全局共鸣值；越过阈值即联动重力调制、地面火海、空投补给、第三阵营 AI 与陨石打击。', {
    x: 0.78, y: 5.74, w: 5.00, h: 0.92,
    fontFace: FONT, fontSize: 11, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
  });

  // 右侧：大厅截图 + 3 个特性卡 (1920x1080 => w=6.45, h=3.63)
  addFramedImage(s, 'shot_2_1_lobby_ui.png', 6.32, 1.26, 6.45, 3.63);
  s.addText('实机主菜单与建房界面：自选小镇/测试地图、模式、目标阈值与 AI/共鸣开关', {
    x: 6.32, y: 4.96, w: 6.45, h: 0.28,
    fontFace: FONT, fontSize: 9.5, color: C.muted, margin: 0,
  });

  const lobbyFeats = [
    ['2–16 人自定义房间', '建房自选对局模式、目标击杀/得分阈值与单局时长'],
    ['双轨联机模式', '外网直连 Photon 云端；无网自动切换 OfflineMode 调试'],
    ['智能 AI 补位', '团队与混战模式自动填充机器人，支持真人顶替入场'],
  ];
  lobbyFeats.forEach(([t, d], i) => {
    const x = 6.32 + i * 2.18;
    addCard(s, x, 5.34, 2.08, 1.50, { fill: C.white, line: C.border });
    s.addText(t, {
      x: x + 0.14, y: 5.48, w: 1.80, h: 0.34,
      fontFace: FONT, fontSize: 11.5, bold: true, color: C.blue, margin: 0,
    });
    s.addText(d, {
      x: x + 0.14, y: 5.88, w: 1.80, h: 0.84,
      fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.25,
    });
  });
}

// =========================================================================
// SLIDE 4: 玩法构成——三模式、四兵种与八武器体系
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '01 / 项目背景与玩法设计', '三模式、四兵种与八武器全表驱动，支撑多维度战术搭配');
  addFooter(s, 4);

  // 顶部两张概览条
  addCard(s, 0.55, 1.22, 6.00, 1.12, { fill: C.white, line: C.border, topBarColor: C.blue, topBarH: 0.05 });
  s.addText('三大对局模式与阵营格局', {
    x: 0.74, y: 1.36, w: 5.60, h: 0.28,
    fontFace: FONT, fontSize: 12.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('TeamMatch 团队竞赛  |  FreeMatch 自由混战  |  CaptureMatch 夺旗模式\n蓝队 / 红队双阵营对抗，高共鸣下引入第三阵营中立机器人搅局', {
    x: 0.74, y: 1.68, w: 5.60, h: 0.56,
    fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.28,
  });

  addCard(s, 6.78, 1.22, 6.00, 1.12, { fill: C.white, line: C.border, topBarColor: C.coral, topBarH: 0.05 });
  s.addText('四大兵种预设 · 四槽位自由配装', {
    x: 6.98, y: 1.36, w: 5.60, h: 0.28,
    fontFace: FONT, fontSize: 12.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('Vanguard 突击  |  Pathfinder 侦察  |  Guardian 支援  |  Artificer 工程\n每兵种标配主武器、副武器、近战与战术投掷四槽位，支持局前切换', {
    x: 6.98, y: 1.68, w: 5.60, h: 0.56,
    fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.28,
  });

  // 下左：8 武器配置表 (WeaponCatalog.asset)
  const wRows = [
    ['武器名称', '类型', '伤害', '间隔', '换弹', '射程', '精准', '负重'].map((t) => ({
      text: t,
      options: { bold: true, color: C.white, fill: { color: C.navy }, align: 'center', fontSize: 10 },
    })),
    ['Rifle', 'Machinegun', '25', '0.09s', '2.0s', '700', '1', '2.0'],
    ['Pistol', 'Pistol', '25', '0.25s', '1.5s', '300', '3', '1.95'],
    ['Shotgun', 'Shotgun', '50', '1.25s', '6.0s', '100', '1', '2.2'],
    ['Grenade', 'Grenade', '80', '2.0s', '2.0s', '125', '1', '1.7'],
    ['Sniper', 'Sniper', '100', '1.8s', '3.0s', '700', '5', '2.2'],
    ['Knife', 'Knife', '60', '0.7s', '1.0s', '2', '1', '1.6'],
    ['Molotov', 'Grenade', '20', '1.0s', '1.2s', '300', '2', '0.0'],
    ['Grenade Launcher', 'Launcher', '75', '2.0s', '3.0s', '100', '1', '2.5'],
  ].map((row, rIdx) => {
    if (rIdx === 0) return row;
    return row.map((cell, cIdx) => ({
      text: cell,
      options: {
        fill: { color: rIdx % 2 === 1 ? C.white : C.subtle2 },
        color: cIdx === 0 ? C.navy : C.body,
        bold: cIdx === 0,
        align: cIdx <= 1 ? 'left' : 'center',
        fontSize: 9.8,
      },
    }));
  });

  s.addTable(wRows, {
    x: 0.55, y: 2.54, w: 6.95, h: 4.18,
    colW: [1.65, 1.15, 0.62, 0.68, 0.68, 0.72, 0.68, 0.77],
    rowH: [0.42, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45, 0.45],
    border: { type: 'solid', color: C.border, pt: 0.75 },
    fontFace: FONT,
    valign: 'middle',
    margin: [2, 5, 2, 5],
  });
  s.addText('数据来源：Assets/Config/Modules/WeaponCatalog.asset（FirearmType 另预留 Burst 连发扩展枚举）', {
    x: 0.55, y: 6.72, w: 6.95, h: 0.24,
    fontFace: FONT, fontSize: 9, color: C.muted, margin: 0,
  });

  // 下右：装备配置界面截图 (1200x740 => w=5.05, h=3.11)
  addFramedImage(s, 'shot_2_2_loadout_ui.png', 7.73, 2.54, 5.05, 3.11);
  addCard(s, 7.73, 5.82, 5.05, 1.00, { fill: C.subtle, line: C.border });
  s.addText('统一武器表驱动设计', {
    x: 7.90, y: 5.92, w: 4.70, h: 0.28,
    fontFace: FONT, fontSize: 11, bold: true, color: C.navy, margin: 0,
  });
  s.addText('武器属性与四槽位引导全部配置化，Molotov 复用 Grenade 类型生成燃烧区域，新增武器无需改动战斗核心脚本。', {
    x: 7.90, y: 6.22, w: 4.70, h: 0.52,
    fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.25,
  });
}

// =========================================================================
// SLIDE 5: 核心创新——全局共鸣系统闭环机制
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '02 / 共鸣核心机制与环境响应', '房主权威结算单一共鸣中枢，跨阈值同步触发五类战场剧变');
  addFooter(s, 5);

  // 左栏：对局行为输入
  addCard(s, 0.55, 1.28, 3.25, 5.56, { fill: C.white, line: C.border, topBarColor: C.blue, topBarH: 0.06 });
  s.addText('对局行为输入（权重触发）', {
    x: 0.75, y: 1.48, w: 2.85, h: 0.34,
    fontFace: FONT, fontSize: 13.5, bold: true, color: C.navy, margin: 0,
  });

  const triggers = [
    ['战斗对抗权重', '命中敌方  +1\n误伤友方  -1', C.blueBg, C.blue],
    ['共鸣设施交互', '击打共鸣增幅器  +2\n击打共鸣抑制器  -2', C.coralBg, C.coral],
    ['夺旗模式博弈', '夺旗  +10  |  交旗  +20\n旗帜归还  -5', C.tealBg, C.teal],
  ];
  triggers.forEach(([t, d, bg, tc], i) => {
    const ty = 1.98 + i * 1.32;
    addCard(s, 0.75, ty, 2.85, 1.14, { fill: bg, line: bg });
    s.addText(t, {
      x: 0.90, y: ty + 0.12, w: 2.55, h: 0.28,
      fontFace: FONT, fontSize: 11.5, bold: true, color: tc, margin: 0,
    });
    s.addText(d, {
      x: 0.90, y: ty + 0.44, w: 2.55, h: 0.58,
      fontFace: FONT, fontSize: 11, bold: true, color: C.navy, margin: 0, lineSpacingMultiple: 1.3,
    });
  });
  s.addText('玩家每次交火与设施争夺，都在主动推高或压低整局战场烈度。', {
    x: 0.75, y: 6.02, w: 2.85, h: 0.64,
    fontFace: FONT, fontSize: 10, color: C.muted, margin: 0, lineSpacingMultiple: 1.3,
  });

  // 箭头 1 -> 2
  s.addShape(pres.ShapeType.rightArrow, {
    x: 3.92, y: 3.84, w: 0.42, h: 0.42,
    fill: { color: C.blue }, line: { type: 'none' },
  });

  // 中栏：权威同步中枢
  addCard(s, 4.46, 1.56, 2.95, 5.00, { fill: C.navy, line: C.navy });
  addBadge(s, 4.72, 1.80, 2.43, 0.34, 'MasterClient 权威中枢', C.coral, C.white, 10.5);
  s.addText('全局共鸣值', {
    x: 4.66, y: 2.34, w: 2.55, h: 0.36,
    fontFace: FONT, fontSize: 16, bold: true, color: C.white, align: 'center', margin: 0,
  });
  s.addText('rr.resonance', {
    x: 4.66, y: 2.74, w: 2.55, h: 0.38,
    fontFace: CODE_FONT, fontSize: 15, bold: true, color: C.coralBg, align: 'center', margin: 0,
  });

  const hubSteps = [
    '1. 房主端统一汇总权重增量',
    '2. 写入 Photon 房间自定义属性',
    '3. 全客户端属性回调实时接收',
    '4. ResonanceSuite.asset 驱动响应',
  ];
  hubSteps.forEach((step, i) => {
    const sy = 3.38 + i * 0.72;
    addCard(s, 4.68, sy, 2.51, 0.54, { fill: '1E2D50', line: '2E4270' });
    s.addText(step, {
      x: 4.78, y: sy, w: 2.31, h: 0.54,
      fontFace: FONT, fontSize: 10.2, color: C.white, valign: 'middle', margin: 0,
    });
  });

  // 箭头 2 -> 3
  s.addShape(pres.ShapeType.rightArrow, {
    x: 7.53, y: 3.84, w: 0.42, h: 0.42,
    fill: { color: C.coral }, line: { type: 'none' },
  });

  // 右栏：五类下游响应
  const responses = [
    ['GravityManager · 重力调制', '曲线映射本地角色跳跃初速度（实测 5.00 → 5.96 → 11.00）并移动重力设施', C.blue],
    ['GroundFireManager · 地面火海', '共鸣越过阈值 150 激活 46 处火点，伤害随共鸣递增，降至 100 即销毁', C.coral],
    ['SupplyManager · 战区空投', '每跨越步长 200 触发一次补给箱空投事件，重塑交火补给点', C.teal],
    ['RobotManager · 第三阵营 AI', '每跨越 1000 刷出中立机器人同时攻击红蓝双方，打破阵地僵局', C.navy],
    ['MeteoriteFacility · 陨石打击', '击中设施召唤陨石坠落（速度 20、基础伤害 50 × 1~3 倍率）', C.coral],
  ];
  responses.forEach(([title, desc, bar], i) => {
    const ry = 1.28 + i * 1.13;
    addCard(s, 8.08, ry, 4.70, 1.00, { fill: C.white, line: C.border });
    s.addShape(pres.ShapeType.rect, {
      x: 8.08, y: ry + 0.08, w: 0.08, h: 0.84,
      fill: { color: bar }, line: { type: 'none' },
    });
    s.addText(title, {
      x: 8.28, y: ry + 0.10, w: 4.35, h: 0.28,
      fontFace: FONT, fontSize: 11.8, bold: true, color: C.navy, margin: 0,
    });
    s.addText(desc, {
      x: 8.28, y: ry + 0.42, w: 4.35, h: 0.48,
      fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.22,
    });
  });
}

// =========================================================================
// SLIDE 6: 共鸣实机前后对比与配置中心
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '02 / 共鸣核心机制与环境响应', '从常态交火到火海爆发：实机视觉反馈与可视化共振调参');
  addFooter(s, 6);

  // 上左：常态 (5.95 x 3.35)
  addFramedImage(s, 'shot_3_1_urp_dual_camera.png', 0.55, 1.22, 5.95, 3.35);
  addBadge(s, 0.55, 4.62, 1.72, 0.28, '常态 · 共鸣值 -100', C.navy, C.white, 9.2);
  s.addText('标准跳跃初速 5.00，场内无地面火海，常规掩体架枪对抗', {
    x: 2.36, y: 4.62, w: 4.14, h: 0.28,
    fontFace: FONT, fontSize: 9.5, color: C.body, valign: 'middle', margin: 0,
  });

  // 上右：高共鸣 (5.95 x 3.35)
  addFramedImage(s, 'shot_4_1_resonance_effects.png', 6.83, 1.22, 5.95, 3.35);
  addBadge(s, 6.83, 4.62, 1.80, 0.28, '高共鸣 · 共鸣值 350', C.coral, C.white, 9.2);
  s.addText('越过阈值 150 激活 46 处火海，HUD 实时播报数值与出生保护', {
    x: 8.72, y: 4.62, w: 4.06, h: 0.28,
    fontFace: FONT, fontSize: 9.5, color: C.body, valign: 'middle', margin: 0,
  });

  // 下半区：配置中心复合卡
  addCard(s, 0.55, 5.00, 12.23, 1.88, { fill: C.white, line: C.border });
  // shot_4_3_config_center.png (1200x740 => h=1.58, w=2.56)
  addFramedImage(s, 'shot_4_3_config_center.png', 0.74, 5.15, 2.56, 1.58);

  const cfgCols = [
    ['ConfigCenterWindow 配置中心', '在 Unity 编辑器内集中调节共鸣权重、重力曲线、火海阈值 150 与陨石下落参数，所见即所得。'],
    ['全端闭环启停验证', '实测共鸣升至 160/350 时 46 处火焰全量激活；攻击抑制器压回 100 后，所有火点同步彻底销毁。'],
    ['异常模块低侵入解耦', '共鸣管理与五类设施集中于 Runtime/Anomaly/（19 文件 / 1,084 行），扩展新异常不耦合战斗主干。'],
  ];
  cfgCols.forEach(([t, d], i) => {
    const cx = 3.55 + i * 3.06;
    s.addText(t, {
      x: cx, y: 5.20, w: 2.88, h: 0.32,
      fontFace: FONT, fontSize: 11.5, bold: true, color: i === 1 ? C.coral : C.blue, margin: 0,
    });
    s.addText(d, {
      x: cx, y: 5.56, w: 2.88, h: 1.12,
      fontFace: FONT, fontSize: 10, color: C.body, margin: 0, lineSpacingMultiple: 1.3,
    });
    if (i < 2) {
      s.addShape(pres.ShapeType.line, {
        x: cx + 2.96, y: 5.24, w: 0, h: 1.40,
        line: { color: C.subtle, width: 1 },
      });
    }
  });
}

// =========================================================================
// SLIDE 7: 工程总体架构、程序集分层与技术栈
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '03 / 关键工程架构实现', '三程序集分层与九大运行时模块，支撑 4.4 万行代码高内聚解耦');
  addFooter(s, 7);

  // 左侧：三程序集分层图
  // 1. Editor
  addCard(s, 0.55, 1.24, 7.25, 0.88, { fill: C.subtle, line: C.border });
  addBadge(s, 0.72, 1.38, 1.95, 0.28, 'Resonance.Editor', C.navy, C.white, 10);
  s.addText('21 文件 / 5,287 行', {
    x: 0.72, y: 1.72, w: 1.95, h: 0.26,
    fontFace: CODE_FONT, fontSize: 9.5, color: C.muted, align: 'center', margin: 0,
  });
  s.addText('15 个自定义 Inspector 检视面板 + 2 个工具窗口（含 ConfigCenterWindow）\n为策划与美术提供可视化参数调优、资源校验与一键配置入口', {
    x: 2.82, y: 1.38, w: 4.82, h: 0.60,
    fontFace: FONT, fontSize: 10.2, color: C.body, margin: 0, lineSpacingMultiple: 1.32,
  });

  // 2. Runtime (9 modules 3x3)
  addCard(s, 0.55, 2.24, 7.25, 3.64, { fill: C.white, line: C.blue, lineW: 1.5 });
  addBadge(s, 0.72, 2.38, 2.15, 0.28, 'Resonance.Runtime', C.blue, C.white, 10);
  s.addText('257 文件 / 36,686 行  ·  9 大核心业务模块（统一命名空间 Resonance）', {
    x: 2.98, y: 2.38, w: 4.65, h: 0.28,
    fontFace: FONT, fontSize: 10, bold: true, color: C.navy, valign: 'middle', margin: 0,
  });

  const modules = [
    ['Core (72 / 5,500)', '配置根/对象池/输入/调度'],
    ['UI (53 / 6,364)', '大厅/房间/设置/HUD'],
    ['World (25 / 2,597)', '关卡/音频/门梯交互'],
    ['Actors (23 / 4,925)', '角色控制/相机/骨骼受击'],
    ['Combat (22 / 5,544)', '枪械/弹道/爆炸/手感'],
    ['Anomaly (19 / 1,084)', '共鸣中枢/五类环境设施'],
    ['Multiplayer (19 / 4,797)', '大厅/准备厅/网络同步'],
    ['Bots (12 / 3,938)', '七态 FSM/掩体/寻路'],
    ['Match (12 / 1,937)', '三模式/计时/击杀播报'],
  ];
  modules.forEach(([mTitle, mDesc], i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    const mx = 0.72 + col * 2.34;
    const my = 2.78 + row * 1.00;
    addCard(s, mx, my, 2.22, 0.88, {
      fill: mTitle.startsWith('Anomaly') ? C.coralBg : C.subtle2,
      line: mTitle.startsWith('Anomaly') ? C.coral : C.border,
    });
    s.addText(mTitle, {
      x: mx + 0.10, y: my + 0.10, w: 2.02, h: 0.28,
      fontFace: CODE_FONT, fontSize: 9.8, bold: true, color: C.navy, margin: 0,
    });
    s.addText(mDesc, {
      x: mx + 0.10, y: my + 0.42, w: 2.02, h: 0.36,
      fontFace: FONT, fontSize: 9.5, color: C.body, margin: 0,
    });
  });

  // 3. Foundation
  addCard(s, 0.55, 6.00, 7.25, 0.86, { fill: C.subtle, line: C.border });
  addBadge(s, 0.72, 6.14, 2.25, 0.28, 'Resonance.Foundation', C.teal, C.white, 10);
  s.addText('32 文件 / 2,066 行', {
    x: 0.72, y: 6.48, w: 2.25, h: 0.24,
    fontFace: CODE_FONT, fontSize: 9.5, color: C.muted, align: 'center', margin: 0,
  });
  s.addText('枚举定义、自定义特性、全局事件总线、补间动画与通用基础设施支持', {
    x: 3.12, y: 6.14, w: 4.50, h: 0.56,
    fontFace: FONT, fontSize: 10.5, color: C.body, valign: 'middle', margin: 0,
  });

  // 右侧上：核心技术栈
  addCard(s, 8.05, 1.24, 4.73, 2.86, { fill: C.white, line: C.border, topBarColor: C.navy, topBarH: 0.05 });
  s.addText('核心技术栈与运行基座', {
    x: 8.24, y: 1.40, w: 4.35, h: 0.32,
    fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
  });
  const stack = [
    ['引擎版本', 'Unity 2022.3.62f3c1'],
    ['渲染管线', 'URP 14.0.12（6 档画质）'],
    ['联机框架', 'Photon PUN 2（v2.45）'],
    ['资源加载', 'Addressables 1.22.3'],
    ['AI 寻路', 'AI Navigation 1.1.7'],
    ['图形接口', 'Direct3D 11 + TMP 3.0.7'],
  ];
  stack.forEach(([k, v], i) => {
    const sy = 1.84 + i * 0.36;
    s.addText(k, {
      x: 8.24, y: sy, w: 1.15, h: 0.28,
      fontFace: FONT, fontSize: 10, bold: true, color: C.muted, margin: 0,
    });
    s.addText(v, {
      x: 9.45, y: sy, w: 3.15, h: 0.28,
      fontFace: FONT, fontSize: 10.5, bold: true, color: C.navy, margin: 0,
    });
  });

  // 右侧下：配置根与运行中枢
  addCard(s, 8.05, 4.28, 4.73, 2.58, { fill: C.white, line: C.border, topBarColor: C.coral, topBarH: 0.05 });
  s.addText('配置根与运行期中枢', {
    x: 8.24, y: 4.44, w: 4.35, h: 0.32,
    fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
  });
  s.addText('ProjectConfigRoot.asset（Addressables 单例）统一管理五大模块资产：MatchRules、WeaponCatalog、ArenaCatalog、FactionRoster 与 ResonanceSuite。\n\nRuntimeServices 与 GameController 在局内统一提供本机玩家、武器表、阵营配置与全局调度引用。', {
    x: 8.24, y: 4.86, w: 4.35, h: 1.84,
    fontFace: FONT, fontSize: 10.2, color: C.body, margin: 0, lineSpacingMultiple: 1.32,
  });
}

// =========================================================================
// SLIDE 8: Photon PUN 2 四通道网络同步架构
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '03 / 关键工程架构实现', '按同步频次与语义划分四条通道，兼顾带宽控制与断线容错');
  addFooter(s, 8);

  const channels = [
    ['通道 1 · 连续状态流', 'IPunObservable', '角色与机器人位姿、旋转与动作流序列化传输，配合客户端插值平滑消除抖动，避免高频 RPC 堵塞。', '高频连续帧同步', C.blue, C.blueBg],
    ['通道 2 · 瞬时动作指令', '[PunRPC]（28 个）', '负责开火表现、换弹动画、受击扣血及机器人开火同步等一次性离散动作，精准送达。', '点对点/广播调用', C.coral, C.coralBg],
    ['通道 3 · 轻量全局广播', 'RaiseEvent（10–22）', '维护于 NetProtocol.cs，涵盖准备、初同步、开局、聊天、击杀、夺旗、空投、门体与倒计时共 13 码。', '无需挂载 PhotonView', C.teal, C.tealBg],
    ['通道 4 · 房间共享状态', 'CustomProperties', '托管全局共鸣值 rr.resonance 等房间公开状态，确保中途加入与属性回调端最终一致。', '权威状态持久化', C.navy, C.subtle],
  ];
  channels.forEach(([tag, title, desc, foot, bar, bg], i) => {
    const x = 0.55 + i * 3.12;
    addCard(s, x, 1.28, 2.88, 3.65, { fill: C.white, line: C.border, topBarColor: bar, topBarH: 0.06 });
    addBadge(s, x + 0.16, 1.48, 2.56, 0.28, tag, bg, bar, 9.5);
    s.addText(title, {
      x: x + 0.16, y: 1.90, w: 2.56, h: 0.36,
      fontFace: CODE_FONT, fontSize: 13.5, bold: true, color: C.navy, margin: 0,
    });
    s.addText(desc, {
      x: x + 0.16, y: 2.38, w: 2.56, h: 1.85,
      fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
    });
    addCard(s, x + 0.16, 4.38, 2.56, 0.38, { fill: C.subtle2, line: C.border });
    s.addText(foot, {
      x: x + 0.16, y: 4.38, w: 2.56, h: 0.38,
      fontFace: FONT, fontSize: 9.5, bold: true, color: C.muted, align: 'center', valign: 'middle', margin: 0,
    });
  });

  // 下半区两张容错与实网卡
  addCard(s, 0.55, 5.14, 5.95, 1.72, { fill: C.white, line: C.border });
  s.addShape(pres.ShapeType.rect, { x: 0.55, y: 5.26, w: 0.08, h: 1.48, fill: { color: C.blue }, line: { type: 'none' } });
  s.addText('房主迁移容错（OnMasterClientSwitched）', {
    x: 0.80, y: 5.26, w: 5.50, h: 0.32,
    fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
  });
  s.addText('当房主中途退出房间时，系统自动在回调中接管对局倒计时、共鸣中枢结算及场内全部 AI 机器人控制权，保障联机对局零中断。', {
    x: 0.80, y: 5.66, w: 5.50, h: 1.00,
    fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.32,
  });

  addCard(s, 6.83, 5.14, 5.95, 1.72, { fill: C.white, line: C.border });
  s.addShape(pres.ShapeType.rect, { x: 6.83, y: 5.26, w: 0.08, h: 1.48, fill: { color: C.coral }, line: { type: 'none' } });
  s.addText('实网接入适配与离线分支', {
    x: 7.08, y: 5.26, w: 5.50, h: 0.32,
    fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
  });
  s.addText('采用 WebSocketSecure 协议接入 Photon Cloud usw 节点，实测大厅 RTT 235–280 ms、局内 RTT 246–262 ms；无外网时支持 OfflineMode 本地单机调试。', {
    x: 7.08, y: 5.66, w: 5.50, h: 1.00,
    fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.32,
  });
}

// =========================================================================
// SLIDE 9: 核心战斗实现——Heun 弹道与命中管线
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '03 / 关键工程架构实现', '二阶 Heun 步进弹道叠加预热对象池，实现精准判定与零运行时 GC');
  addFooter(s, 9);

  // 上半区：5 步水平流水线
  const pipeSteps = [
    ['1. 对象池取弹', 'ObjectRecycler 复用\n零 Instantiate 开销', C.blue],
    ['2. 二阶 Heun 步进', 'FixedUpdate 积分推进\n不给子弹挂载刚体', C.navy],
    ['3. 分段射线采样', 'hitTestFrameStep = 3\n每 3 帧采样防穿透', C.blue],
    ['4. 四路命中分流', '玩家骨骼 / 机器人 /\n共鸣设施 / 场景墙体', C.coral],
    ['5. 贴花与回收', '墙面生成弹孔贴花\n子弹立即归还对象池', C.teal],
  ];
  pipeSteps.forEach(([t, d, col], i) => {
    const px = 0.55 + i * 2.52;
    addCard(s, px, 1.26, 2.18, 1.56, { fill: C.white, line: C.border, topBarColor: col, topBarH: 0.06 });
    s.addText(t, {
      x: px + 0.12, y: 1.44, w: 1.94, h: 0.34,
      fontFace: FONT, fontSize: 11.5, bold: true, color: C.navy, margin: 0,
    });
    s.addText(d, {
      x: px + 0.12, y: 1.84, w: 1.94, h: 0.84,
      fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.28,
    });
    if (i < 4) {
      s.addShape(pres.ShapeType.rightArrow, {
        x: px + 2.23, y: 1.90, w: 0.24, h: 0.28,
        fill: { color: C.muted }, line: { type: 'none' },
      });
    }
  });

  // 下左：四路命中分流 (2x2)
  addCard(s, 0.55, 3.04, 6.55, 3.82, { fill: C.white, line: C.border });
  s.addText('ShotController 四路命中分流结算', {
    x: 0.76, y: 3.18, w: 6.10, h: 0.32,
    fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
  });
  const branches = [
    ['玩家骨骼受击', '按头 / 胸 / 臂 / 腿部位倍率扣血，衔接布娃娃、死亡观战与 5s 出生保护。', C.blueBg, C.blue],
    ['机器人目标', '直接扣血并立即唤醒受击警戒状态，触发 AI 反击与掩体转移。', C.subtle, C.navy],
    ['共鸣设施', '击打增幅器/抑制器或陨石设施，立即驱动全局共鸣值与环境变更。', C.coralBg, C.coral],
    ['场景碰撞体', '在命中点生成弹孔贴花后将子弹对象归还池内，避免运行时 GC。', C.tealBg, C.teal],
  ];
  branches.forEach(([t, d, bg, tc], i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    const bx = 0.76 + col * 3.12;
    const by = 3.60 + row * 1.56;
    addCard(s, bx, by, 2.98, 1.42, { fill: bg, line: bg });
    s.addText(t, {
      x: bx + 0.14, y: by + 0.12, w: 2.70, h: 0.28,
      fontFace: FONT, fontSize: 11.5, bold: true, color: tc, margin: 0,
    });
    s.addText(d, {
      x: bx + 0.14, y: by + 0.44, w: 2.70, h: 0.88,
      fontFace: FONT, fontSize: 9.8, color: C.navy, margin: 0, lineSpacingMultiple: 1.28,
    });
  });

  // 下右：多形态武器与零 GC 保障
  addCard(s, 7.33, 3.04, 5.45, 3.82, { fill: C.white, line: C.border, topBarColor: C.coral, topBarH: 0.06 });
  s.addText('多形态武器机制与性能保障', {
    x: 7.55, y: 3.22, w: 5.00, h: 0.34,
    fontFace: FONT, fontSize: 13.5, bold: true, color: C.navy, margin: 0,
  });
  const weaponsMech = [
    ['近战匕首 (Knife)', '基于 SphereCast 体积扫描判伤，贴身近战判定稳定。'],
    ['手雷与燃烧瓶', '抛物线物理投掷，Molotov 落地后生成持续燃烧伤害区。'],
    ['榴弹发射器', '范围爆炸伤害叠加障碍物遮挡射线校验，防止隔墙误伤。'],
    ['规则配置与零 GC', 'MatchRules 统一控制出生保护 5s、复活 7s、自动回血与掉枪；配合集中帧调度器实现连续 20 发开火 Gen0 GC = 0。'],
  ];
  weaponsMech.forEach(([t, d], i) => {
    const wy = 3.68 + i * 0.78;
    s.addText(t, {
      x: 7.55, y: wy, w: 5.00, h: 0.24,
      fontFace: FONT, fontSize: 11, bold: true, color: i === 3 ? C.coral : C.blue, margin: 0,
    });
    s.addText(d, {
      x: 7.55, y: wy + 0.24, w: 5.00, h: 0.46,
      fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.22,
    });
  });
}

// =========================================================================
// SLIDE 10: 机器人 AI 架构——七态状态机与掩体调度
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '03 / 关键工程架构实现', '房主端权威运行七态有限状态机，联动 30 处掩体点实现拟真对抗');
  addFooter(s, 10);

  // 左侧：行为模式 + 七态 FSM + 掩体机制
  addCard(s, 0.55, 1.24, 5.65, 0.68, { fill: C.white, line: C.border });
  s.addText('三大行为模式', {
    x: 0.74, y: 1.38, w: 1.25, h: 0.38,
    fontFace: FONT, fontSize: 11, bold: true, color: C.navy, valign: 'middle', margin: 0,
  });
  const modes = [
    ['激进 Aggressive', C.coralBg, C.coral],
    ['被动 Passive', C.subtle, C.navy],
    ['保护 Defensive', C.blueBg, C.blue],
  ];
  modes.forEach(([m, bg, tc], i) => {
    addBadge(s, 2.08 + i * 1.35, 1.40, 1.26, 0.34, m, bg, tc, 9.2);
  });

  // 七态状态机容器
  addCard(s, 0.55, 2.04, 5.65, 2.74, { fill: C.white, line: C.border });
  s.addText('AIBotAgent 七态有限状态机', {
    x: 0.74, y: 2.16, w: 3.50, h: 0.28,
    fontFace: FONT, fontSize: 12, bold: true, color: C.navy, margin: 0,
  });

  // 中心双核心态：观察 <-> 搜寻
  addBadge(s, 2.05, 2.58, 1.15, 0.42, '观察', C.coral, C.white, 11);
  addBadge(s, 3.65, 2.58, 1.15, 0.42, '搜寻', C.blue, C.white, 11);
  s.addShape(pres.ShapeType.leftRightArrow, {
    x: 3.24, y: 2.68, w: 0.37, h: 0.22,
    fill: { color: C.navy }, line: { type: 'none' },
  });

  // 外围五态节点
  const outerStates = ['待机', '巡逻', '跟随', '掩护', '驻守'];
  outerStates.forEach((st, i) => {
    const sx = 0.78 + i * 1.06;
    addCard(s, sx, 3.50, 0.94, 0.46, { fill: C.subtle, line: C.blue, lineW: 1 });
    s.addText(st, {
      x: sx, y: 3.50, w: 0.94, h: 0.46,
      fontFace: FONT, fontSize: 10.5, bold: true, color: C.navy, align: 'center', valign: 'middle', margin: 0,
    });
    s.addShape(pres.ShapeType.line, {
      x: sx + 0.47, y: 3.04, w: 0, h: 0.44,
      line: { color: C.slateMid, width: 1, dashType: 'dash' },
    });
  });
  s.addText('七态按视距感知、受击告警与掩体可用度实时迁移', {
    x: 0.74, y: 4.18, w: 5.25, h: 0.44,
    fontFace: FONT, fontSize: 10, color: C.muted, align: 'center', valign: 'middle', margin: 0,
  });

  // 底部 2 条工程要点卡
  addCard(s, 0.55, 4.92, 5.65, 0.90, { fill: C.white, line: C.border });
  s.addText('掩体成对调度 (AICoverSpotManager)', {
    x: 0.74, y: 5.02, w: 5.25, h: 0.26,
    fontFace: FONT, fontSize: 11, bold: true, color: C.blue, margin: 0,
  });
  s.addText('掩体节点采用「占用 + 冷却」配对机制，避免多名机器人扎堆抢点，提升分散交火真实感。', {
    x: 0.74, y: 5.30, w: 5.25, h: 0.44,
    fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.22,
  });

  addCard(s, 0.55, 5.94, 5.65, 0.92, { fill: C.white, line: C.border });
  s.addText('房主权威寻路与全端同步', {
    x: 0.74, y: 6.04, w: 5.25, h: 0.26,
    fontFace: FONT, fontSize: 11, bold: true, color: C.coral, margin: 0,
  });
  s.addText('仅房主端计算 NavMesh 寻路与状态决策；其余客户端同步位姿插值，并通过 RPC 触发开火与换弹。', {
    x: 0.74, y: 6.32, w: 5.25, h: 0.44,
    fontFace: FONT, fontSize: 9.8, color: C.body, margin: 0, lineSpacingMultiple: 1.22,
  });

  // 右侧：NavMesh 实景图 (1281x716 => w=6.25, h=3.49)
  addFramedImage(s, 'shot_4_2_ai_navmesh_cover.png', 6.53, 1.24, 6.25, 3.49);
  addCard(s, 6.53, 4.92, 6.25, 1.94, { fill: C.white, line: C.border, topBarColor: C.navy, topBarH: 0.05 });
  s.addText('OldQuarter 地图 AI 导航与掩体拓扑实景', {
    x: 6.72, y: 5.08, w: 5.85, h: 0.30,
    fontFace: FONT, fontSize: 12.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('• 蓝色网格为已烘焙 NavMesh 可行走区域，覆盖小镇街区、桥梁与农田。\n• 红色连线为 30 处 AICoverSpot 掩体节点之间的战术转移拓扑。\n• 支持蓝/红阵营 AI 自动补位，并在高共鸣阶段刷出第三阵营中立机器人并发交火。', {
    x: 6.72, y: 5.44, w: 5.85, h: 1.30,
    fontFace: FONT, fontSize: 10.2, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
  });
}

// =========================================================================
// SLIDE 11: URP 渲染管线、双相机防穿墙与后处理
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '03 / 关键工程架构实现', 'URP Base + Overlay 双相机栈根治枪模穿墙，兼顾水体与画质分级');
  addFooter(s, 11);

  // 左侧：实机图 (6.15 x 3.46)
  addFramedImage(s, 'shot_3_1_urp_dual_camera.png', 0.55, 1.26, 6.15, 3.46);
  addCard(s, 0.55, 4.88, 6.15, 1.98, { fill: C.white, line: C.border });
  s.addText('第一人称近墙持枪防穿墙实测', {
    x: 0.74, y: 5.02, w: 5.75, h: 0.30,
    fontFace: FONT, fontSize: 12.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('主相机设为深度 1 的 Base 相机渲染小镇地形与天空盒，武器相机设为深度 2 的 Overlay 相机并挂载至 Camera Stack，配合前景着色器确保贴近掩体墙面时枪械模型绝不穿模裁切。', {
    x: 0.74, y: 5.40, w: 5.75, h: 1.30,
    fontFace: FONT, fontSize: 10.5, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
  });

  // 右侧：3 张纵向技术卡
  const renderCards = [
    ['双相机栈与前景着色器', '双阵营玩家预制体主相机均挂载武器 Overlay 相机；采用自研前景着色器 OverlayURP.shader 保持第一人称持枪深度优先且光照协调。', C.blue],
    ['屏幕深度水体与 6 档画质', '河道水体基于屏幕坐标与深度图差值计算水深，叠双层反向滚动法线呈现水波；QualitySettings 6 档画质分别绑定性能/平衡/高画质 URP 资产。', C.teal],
    ['动态后处理与反射控制', 'PostProcessEffects.cs 运行时按需开关 Bloom、色差与动态模糊；通过反射读取 URP Renderer Feature 列表实现 SSAO 实时启停。', C.coral],
  ];
  renderCards.forEach(([t, d, bar], i) => {
    const ry = 1.26 + i * 1.90;
    addCard(s, 6.95, ry, 5.83, 1.74, { fill: C.white, line: C.border });
    s.addShape(pres.ShapeType.rect, {
      x: 6.95, y: ry + 0.14, w: 0.08, h: 1.46,
      fill: { color: bar }, line: { type: 'none' },
    });
    s.addText(t, {
      x: 7.20, y: ry + 0.18, w: 5.40, h: 0.34,
      fontFace: FONT, fontSize: 13.5, bold: true, color: C.navy, margin: 0,
    });
    s.addText(d, {
      x: 7.20, y: ry + 0.58, w: 5.40, h: 1.02,
      fontFace: FONT, fontSize: 10.8, color: C.body, margin: 0, lineSpacingMultiple: 1.35,
    });
  });
}

// =========================================================================
// SLIDE 12: 系统功能与多端联机实测验证
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '04 / 系统测试与性能分析', '接入 Photon 云端实测 6 项核心用例，联机、共鸣、AI 与对象池全通过');
  addFooter(s, 12);

  // 顶部 4 枚环境徽章
  const envBadges = [
    ['实网节点', 'Photon Cloud usw (WSS)', C.blueBg, C.blue],
    ['对局延迟', '局内 RTT 246–262 ms', C.subtle, C.navy],
    ['测试平台', 'Unity 2022.3 · D3D11 Ultra', C.subtle, C.navy],
    ['用例结论', '6 / 6 全部通过', C.tealBg, C.teal],
  ];
  envBadges.forEach(([k, v, bg, tc], i) => {
    const bx = 0.55 + i * 3.12;
    addCard(s, bx, 1.22, 2.88, 0.66, { fill: bg, line: bg });
    s.addText(k, {
      x: bx + 0.14, y: 1.28, w: 2.60, h: 0.22,
      fontFace: FONT, fontSize: 9, bold: true, color: tc, margin: 0,
    });
    s.addText(v, {
      x: bx + 0.14, y: 1.50, w: 2.60, h: 0.30,
      fontFace: FONT, fontSize: 11.5, bold: true, color: C.navy, margin: 0,
    });
  });

  // 6 行功能验证表
  const tcRows = [
    ['编号', '测试模块', '实测操作与输入条件', '实测观测结果', '结论'].map((t) => ({
      text: t,
      options: { bold: true, color: C.white, fill: { color: C.navy }, align: 'center', fontSize: 10.5 },
    })),
    ['TC-01', '大厅与切图', '主菜单创建 6 人小镇团队竞赛房并开局', '大厅 RTT 235–280 ms，自动分队并成功载入小镇场景', '通过'],
    ['TC-02', '共鸣重力同步', '触发共鸣接口推高数值至 160 与 1000', '房间属性全端同步；跳跃初速度由 5.00 升至 5.96 与 11.00', '通过'],
    ['TC-03', '地面火海启停', '共鸣越过阈值 150 至 350，再压低至 100', '激活 46 处火焰无色块遮挡；回落至 100 后全部销毁', '通过'],
    ['TC-04', '空投与中立 AI', '共鸣依次跨越步长 200 与 1000', '跨越 200 触发 2 次空投；跨越 1000 刷出中立 AI（机器人 5 → 6）', '通过'],
    ['TC-05', 'AI 状态机寻路', '小镇 30 处掩体环境下观测 5–6 名 AI', '在搜寻与警戒状态实时巡检切换并正常交火击杀', '通过'],
    ['TC-06', '对象池连射', '主武器自动步枪连续触发 20 次单发开火', '20 次弹道推进与射线判定正常，Gen0 GC 增量保持 0', '通过'],
  ].map((row, rIdx) => {
    if (rIdx === 0) return row;
    return row.map((cell, cIdx) => ({
      text: cell,
      options: {
        fill: { color: rIdx % 2 === 1 ? C.white : C.subtle2 },
        color: cIdx === 4 ? C.teal : (cIdx <= 1 ? C.navy : C.body),
        bold: cIdx === 0 || cIdx === 4,
        align: cIdx === 0 || cIdx === 4 ? 'center' : 'left',
        fontSize: 10,
      },
    }));
  });

  s.addTable(tcRows, {
    x: 0.55, y: 2.04, w: 12.23, h: 4.78,
    colW: [0.95, 1.75, 4.05, 4.58, 0.90],
    rowH: [0.48, 0.70, 0.70, 0.70, 0.70, 0.70, 0.70],
    border: { type: 'solid', color: C.border, pt: 0.75 },
    fontFace: FONT,
    valign: 'middle',
    margin: [4, 8, 4, 8],
  });
}

// =========================================================================
// SLIDE 13: URP 与高共鸣并发性能评测
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '04 / 系统测试与性能分析', '46 处火海并发稳居 67.7 FPS，预热对象池实现 360 帧零 Gen0 GC');
  addFooter(s, 13);

  // 左上：原生柱状图容器
  addCard(s, 0.55, 1.24, 6.75, 3.48, { fill: C.white, line: C.border });
  s.addChart(pres.ChartType.bar, [
    {
      name: '平均帧率 (FPS)',
      labels: ['W1 待机全特效', 'W2 高共鸣交火全特效', 'W3 高共鸣关后处理'],
      values: [87.7, 67.7, 55.6],
    },
    {
      name: '1% 低帧率 (FPS)',
      labels: ['W1 待机全特效', 'W2 高共鸣交火全特效', 'W3 高共鸣关后处理'],
      values: [69.4, 39.7, 37.8],
    },
  ], {
    x: 0.68, y: 1.32, w: 6.50, h: 3.32,
    barDir: 'col',
    barGrouping: 'clustered',
    chartColors: [C.blue, C.coral],
    showLegend: true,
    legendPos: 't',
    legendFontFace: FONT,
    legendFontSize: 10,
    showValue: true,
    dataLabelFormatCode: '0.0',
    dataLabelFontFace: CODE_FONT,
    dataLabelFontSize: 9.5,
    dataLabelColor: C.navy,
    catAxisLabelFontFace: FONT,
    catAxisLabelFontSize: 9.5,
    catAxisLabelColor: C.navy,
    valAxisLabelFontFace: CODE_FONT,
    valAxisLabelFontSize: 9,
    valAxisLabelColor: C.muted,
    valAxisMaxVal: 100,
    valGridLine: { color: C.subtle, size: 0.5 },
    catGridLine: { style: 'none' },
  });

  // 左下：紧凑 4 行对照表
  const perfRows = [
    ['核心性能指标', 'W1 待机全特效', 'W2 高共鸣全特效', 'W3 关后处理'].map((t) => ({
      text: t,
      options: { bold: true, color: C.white, fill: { color: C.navy }, align: 'center', fontSize: 9.5 },
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
        fill: { color: rIdx % 2 === 1 ? C.white : C.subtle2 },
        color: rIdx === 4 && cIdx > 0 ? C.teal : (cIdx === 0 ? C.navy : C.body),
        bold: cIdx === 0 || rIdx === 4,
        align: cIdx === 0 ? 'left' : 'center',
        fontSize: 9.5,
      },
    }));
  });
  s.addTable(perfRows, {
    x: 0.55, y: 4.86, w: 6.75, h: 1.98,
    colW: [1.95, 1.60, 1.60, 1.60],
    rowH: [0.38, 0.39, 0.39, 0.39, 0.39],
    border: { type: 'solid', color: C.border, pt: 0.75 },
    fontFace: FONT,
    valign: 'middle',
    margin: [2, 6, 2, 6],
  });

  // 右侧：3 张结论卡
  const perfNotes = [
    ['高负载流畅达标', '从 W1 待机切入激活 46 处火海与 AI 交火连射的 W2 状态，Draw Call 仅从 218.0 升至 236.5，平均帧耗时增加 3.37 ms，平均帧率稳居 67.7 FPS 流畅交火区间。', C.blue],
    ['零 GC 内存平稳', '3 组共 360 帧采样期内 Gen0 垃圾回收增量全部为 0 次，已分配总内存从 W1 到 W2 仅静态增长 14.4 MB，证明预热对象池与集中调度有效消除高频分配。', C.teal],
    ['W3 现象客观复盘', '关闭四项后处理后 Draw Call 下降 15.7%、三角面数降 23.7%；联机采样中恰好遇 AI 密集寻路重算拉低主线程，在排除 AI 波动的对照采样中帧率由 62.3 升至 67.0 FPS。', C.coral],
  ];
  perfNotes.forEach(([t, d, bar], i) => {
    const ny = 1.24 + i * 1.90;
    addCard(s, 7.55, ny, 5.23, 1.76, { fill: C.white, line: C.border });
    s.addShape(pres.ShapeType.rect, {
      x: 7.55, y: ny + 0.14, w: 0.08, h: 1.48,
      fill: { color: bar }, line: { type: 'none' },
    });
    s.addText(t, {
      x: 7.80, y: ny + 0.16, w: 4.80, h: 0.32,
      fontFace: FONT, fontSize: 13, bold: true, color: C.navy, margin: 0,
    });
    s.addText(d, {
      x: 7.80, y: ny + 0.54, w: 4.80, h: 1.12,
      fontFace: FONT, fontSize: 10.2, color: C.body, margin: 0, lineSpacingMultiple: 1.32,
    });
  });
}

// =========================================================================
// SLIDE 14: 团队成员分工与协作模式
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '05 / 团队分工与实训总结', '三人职责清晰、契约与配置解耦，保障联机全链路高效集成');
  addFooter(s, 14);

  const members = [
    [
      '吴凯东', '程序框架设计', C.navy, C.subtle,
      [
        ['工程架构分层', '设计 Foundation / Runtime / Editor 三程序集划分与 Addressables 配置加载根。'],
        ['联机同步基座', '封装 Photon PUN 2 四通道同步、NetProtocol 事件码协议及房主迁移接管机制。'],
        ['渲染与工具链', '实现 URP 双相机防穿墙、集中帧调度器、预热对象池及 15 个编辑器扩展。'],
      ],
    ],
    [
      '杨世杰', '业务逻辑实现', C.blue, C.blueBg,
      [
        ['对局与模式控制', '实现团队、混战、夺旗三模式控制器、准备厅及局内计分与复活流程。'],
        ['共鸣核心与响应', '开发房主端共鸣结算中枢，落地重力、火海、空投、中立 AI 与陨石五类响应。'],
        ['战斗与 AI 系统', '实现 Heun 步进弹道、八武器与骨骼受击，以及七态 AI 状态机与掩体调度。'],
      ],
    ],
    [
      '周欣琦', '美术、策划、测试', C.coral, C.coralBg,
      [
        ['关卡与视觉表现', '搭建 OldQuarter 小镇场景与烘焙 NavMesh，制作水体/火海粒子及大厅 HUD。'],
        ['数值与玩法策划', '主导四兵种八武器数值平衡，设计共鸣权重表与五类环境阈值梯度。'],
        ['联机与性能测试', '执行 Photon 云端 6 项核心用例与三工况 360 帧采样，推进缺陷回归闭环。'],
      ],
    ],
  ];

  members.forEach(([name, role, bar, badgeBg, items], i) => {
    const mx = 0.55 + i * 4.18;
    addCard(s, mx, 1.26, 3.88, 4.38, { fill: C.white, line: C.border, topBarColor: bar, topBarH: 0.07 });
    s.addText(name, {
      x: mx + 0.22, y: 1.48, w: 1.60, h: 0.40,
      fontFace: FONT, fontSize: 19, bold: true, color: C.navy, margin: 0,
    });
    addBadge(s, mx + 1.88, 1.52, 1.78, 0.34, role, badgeBg, bar, 10);
    s.addShape(pres.ShapeType.line, {
      x: mx + 0.22, y: 2.02, w: 3.44, h: 0,
      line: { color: C.subtle, width: 1 },
    });
    items.forEach(([itTitle, itDesc], j) => {
      const iy = 2.18 + j * 1.12;
      s.addText(itTitle, {
        x: mx + 0.22, y: iy, w: 3.44, h: 0.28,
        fontFace: FONT, fontSize: 11.5, bold: true, color: bar, margin: 0,
      });
      s.addText(itDesc, {
        x: mx + 0.22, y: iy + 0.30, w: 3.44, h: 0.68,
        fontFace: FONT, fontSize: 10, color: C.body, margin: 0, lineSpacingMultiple: 1.28,
      });
    });
  });

  // 底部协作横带
  addCard(s, 0.55, 5.82, 12.23, 1.02, { fill: C.subtle, line: C.border });
  const collab = [
    ['1. 接口契约先行', 'NetProtocol 事件码与 ConfigRoot 结构先定稿，程序并行开发零冲突'],
    ['2. 配置零代码并行', '策划与美术直接调优 31 份资产与配置中心，无需等待脚本修改'],
    ['3. 双轨联调闭环', '外网 Photon 云端联机实测 + 离线单机快速回归，问题当日闭环'],
  ];
  collab.forEach(([ct, cd], i) => {
    const cx = 0.74 + i * 4.02;
    addCard(s, cx, 5.94, 3.74, 0.78, { fill: C.white, line: C.border });
    s.addText(ct, {
      x: cx + 0.14, y: 6.00, w: 3.46, h: 0.24,
      fontFace: FONT, fontSize: 10.5, bold: true, color: C.navy, margin: 0,
    });
    s.addText(cd, {
      x: cx + 0.14, y: 6.26, w: 3.46, h: 0.40,
      fontFace: FONT, fontSize: 9.2, color: C.body, margin: 0,
    });
    if (i < 2) {
      s.addShape(pres.ShapeType.rightArrow, {
        x: cx + 3.79, y: 6.22, w: 0.18, h: 0.22,
        fill: { color: C.blue }, line: { type: 'none' },
      });
    }
  });
}

// =========================================================================
// SLIDE 15: 项目总结与后续演进
// =========================================================================
{
  const s = pres.addSlide();
  addHeader(s, '05 / 团队分工与实训总结', '实训总结与后续演进方向');
  addFooter(s, 15);

  // 上左：总结
  addCard(s, 0.55, 1.26, 5.95, 2.74, { fill: C.white, line: C.border, topBarColor: C.blue, topBarH: 0.06 });
  s.addText('项目总结与实训收获', {
    x: 0.78, y: 1.44, w: 5.50, h: 0.34,
    fontFace: FONT, fontSize: 14.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('• 完整交付可实网联机运行的 Unity URP + PUN 2 多人 FPS 工程，累计 310 个一方脚本、44,039 行 C# 代码。\n• 以「全局共鸣中枢」实现差异化玩法创新，重力、火海、空投、中立 AI 与陨石 5 类剧变多端一致运行。\n• 依托预热对象池、集中帧调度与数据驱动配置，兼顾高负载 67.7 FPS / 零 Gen0 GC 与三人高效协作。', {
    x: 0.78, y: 1.88, w: 5.50, h: 1.98,
    fontFace: FONT, fontSize: 10.8, color: C.body, margin: 0, lineSpacingMultiple: 1.42,
  });

  // 上右：演进方向
  addCard(s, 6.83, 1.26, 5.95, 2.74, { fill: C.white, line: C.border, topBarColor: C.coral, topBarH: 0.06 });
  s.addText('后续演进方向', {
    x: 7.06, y: 1.44, w: 5.50, h: 0.34,
    fontFace: FONT, fontSize: 14.5, bold: true, color: C.navy, margin: 0,
  });
  s.addText('• 网络体验升级：引入客户端预测与服务端延迟补偿，进一步降低高 RTT 网络环境下的命中判定延迟。\n• 内容与模式扩展：扩充多张主题对战地图，启用武器表预留的 Burst 连发模式及自定义共鸣规则预设。\n• 架构向专用服演进：探索 Dedicated Server 替代房主权威模式，提升反作弊能力与大房间承载上限。', {
    x: 7.06, y: 1.88, w: 5.50, h: 1.98,
    fontFace: FONT, fontSize: 10.8, color: C.body, margin: 0, lineSpacingMultiple: 1.42,
  });

  // 下半区：深靛蓝致谢主横幅
  addCard(s, 0.55, 4.22, 12.23, 2.62, { fill: C.navy, line: C.navy });
  s.addShape(pres.ShapeType.rect, {
    x: 0.55, y: 4.22, w: 0.14, h: 2.62,
    fill: { color: C.coral }, line: { type: 'none' },
  });
  s.addText('感谢各位老师聆听 · 敬请批评指正', {
    x: 1.00, y: 4.58, w: 11.20, h: 0.68,
    fontFace: FONT, fontSize: 28, bold: true, color: C.white, margin: 0,
  });
  s.addText('《共鸣之境》实训项目组：吴凯东 · 杨世杰 · 周欣琦　　|　　指导教师：黄取治', {
    x: 1.00, y: 5.42, w: 11.20, h: 0.42,
    fontFace: FONT, fontSize: 13.5, color: C.slateMid, margin: 0,
  });
  s.addText('福建师范大学协和学院 · 信息技术系 · 数字媒体技术专业　|　2026 年 11 月', {
    x: 1.00, y: 6.02, w: 11.20, h: 0.34,
    fontFace: FONT, fontSize: 10.5, color: C.slateLight, margin: 0,
  });
}

pres.writeFile({ fileName: OUT_FILE }).then(() => {
  console.log('Successfully generated:', OUT_FILE);
}).catch((err) => {
  console.error('Failed to generate PPTX:', err);
  process.exit(1);
});
