# The Realm of Resonance — 项目解析报告

## 一、项目概况

| 属性       | 值                                                       |
| -------- | ------------------------------------------------------- |
| 项目名称     | The realm of resonance（共鸣之域）                            |
| 公司名      | DefaultCompany（未修改）                                     |
| 版本       | 0.1                                                     |
| Unity 版本 | 2022.3.62f3c1（URP 渲染管线）                                 |
| 分辨率      | 1920 x 1080                                             |
| 构建场景     | MainMenu → Desert → Test → Town                         |
| 基础框架     | MFPS 2.0（Lovatto Studio）多人 FPS 框架                       |
| 网络方案     | Photon PUN2（源码内置于 Assets/MFPS/Content/Required/Photon/） |

## 二、项目性质

这是一个基于 **MFPS 2.0** 二次开发的 **多人团队第一人称射击（FPS）游戏**。核心创新是 **"共鸣值（Resonance）"系统** —— 一个由玩家战斗行为驱动的全局环境机制，让战斗激烈程度直接影响战场环境危险度。

两支队伍对抗，队名分别为 **"涌浪人"** 和 **"筑波者"**。

## 三、代码结构

项目共有 **485 个 C# 脚本**：

- **457 个**属于 MFPS 第三方框架（Assets/MFPS/Scripts/）
- **23 个**为项目自研脚本（Assets/Scripts/）
- **5 个**为其他插件脚本

### 自研脚本清单

```
Assets/Scripts/
├── AI/
│   └── UpdateNavmesh.cs              # 定时重新烘焙 NavMesh（动态场景）
├── Editor/
│   ├── FindMissingFonts.cs           # 查找 TMP 缺失字体
│   └── GravityFacilityTools.cs       # 重力设施批量复位工具
├── Meteorite/
│   ├── MeteoriteFacility.cs          # 陨石设施（继承 ResonanceFacility）
│   ├── MeteoriteProjectile.cs       # 陨石弹道（继承 bl_GrenadeLauncherProjectile）
│   └── MeteoriteShooterMarker.cs    # 开火者头顶箭头标记
├── Resonance/
│   ├── Environment/
│   │   ├── GravityFacility.cs        # 重力平台（IPunObservable 位置同步）
│   │   ├── Planet.cs                 # 背景星球自转
│   │   └── ResonanceFacility.cs      # 共鸣设施基类（IMFPSDamageable）
│   ├── Manager/
│   │   ├── ResonanceManager.cs       # ★ 共鸣管理器（核心中枢）
│   │   └── Environment/
│   │       ├── EnvironmentEffectManager.cs  # 环境效果管理器基类
│   │       ├── GravityManager.cs            # 重力/弹跳力管理
│   │       ├── GroundFireManager.cs         # 地面火焰管理
│   │       ├── ItemDropManager.cs           # 补给掉落管理
│   │       └── RobotManager.cs             # 机器人生成管理
│   ├── Settings/
│   │   ├── ResonanceConfig.cs         # 共鸣值变化系数表
│   │   ├── GravityConfig.cs           # 重力配置
│   │   ├── GroundFireConfig.cs        # 地火配置
│   │   ├── ItemDropConfig.cs          # 掉落配置
│   │   ├── MeteoriteConfig.cs         # 陨石配置
│   │   └── RobotConfig.cs            # 机器人配置
│   └── UI/
│       └── ResonanceUI.cs            # 共鸣值 UI 显示
└── Utilities/
    └── GetMainCamera.cs              # Canvas 指定 worldCamera
```

## 四、核心系统详解

### 4.1 共鸣值系统（ResonanceManager）

**ResonanceManager** 是整个游戏的中枢，采用单例模式 + `MonoBehaviourPunCallbacks`：

- 维护全局 `currentResonance`（共鸣值）
- 通过 **Photon 房间自定义属性**（Key = `"ResonanceValue"`）在网络间同步
- `ChangeResonance(string changeKey, float multiplier)`：仅 MasterClient 可执行，按配置键查表增减共鸣值并广播
- 提供 `OnResonanceChanged` 事件，所有环境管理器订阅后联动
- 启动时从房间属性恢复共鸣值

### 4.2 共鸣值如何被改变

共鸣值由战斗行为驱动，修改点散落在 MFPS 武器脚本中：

| 行为     | 共鸣值变化               | 修改位置                                |
| ------ | ------------------- | ----------------------------------- |
| 击中敌人   | +（按伤害值 × 系数）        | bl_Bullet.cs, bl_ExplosionDamage.cs |
| 击中队友   | -（按伤害值 × 系数）        | bl_Bullet.cs, bl_ExplosionDamage.cs |
| 射击共鸣设施 | ±（按设施 resonanceKey） | bl_Bullet.cs → SendFacilityDamage() |

非房主玩家通过 RPC（`RPC_RequestResonanceChange`）上报房主执行。

### 4.3 环境效果管理器（观察者模式）

所有管理器继承 `EnvironmentEffectManager` 抽象基类，订阅 `OnResonanceChanged`：

| 管理器                   | 触发条件                          | 效果                                                                               |
| --------------------- | ----------------------------- | -------------------------------------------------------------------------------- |
| **GravityManager**    | 共鸣值持续变化                       | 用 AnimationCurve 修改玩家 jumpSpeed；驱动 GravityFacility 物体在 Start/Final 位置间插值移动（平台升降） |
| **GroundFireManager** | 共鸣 ≥ 150（activationThreshold） | 在 GroundFire 点位生成地火；伤害和火焰大小随共鸣值曲线上升                                              |
| **RobotManager**      | 共鸣每达到 1000 倍数                 | 调用 bl_AIMananger.SpawnBot 生成 Team3 机器人                                           |
| **ItemDropManager**   | 共鸣每达到 50 倍数                   | 在 DropArea 随机位置网络生成弹药/医疗补给                                                       |

### 4.4 陨石系统

- **ResonanceFacility**：实现 MFPS 的 `IMFPSDamageable`，带 `resonanceKey` 字段，玩家射击可主动改变共鸣值
- **MeteoriteFacility**（继承 ResonanceFacility）：被击中时按 30% 概率触发陨石，选定敌方队伍随机玩家为目标，RPC 广播全客户端，倒计时后从空中生成陨石，伤害基于共鸣值曲线计算
- **MeteoriteProjectile**：重写弹道逻辑，使陨石以固定速度直线飞向目标点
- **MeteoriteShooterMarker**：在开火者头顶生成箭头标记显示 5 秒

### 4.5 网络模型

- **MasterClient 权威**：共鸣值变更仅在 MasterClient 执行
- **同步方式**：Photon 房间自定义属性（`ResonanceValue` key）+ RPC 上报 + IPunObservable 位置同步
- **场景**：Town（主玩法关卡，含 45 个 GroundFirePoint、9 个 CoverPoint、NavMesh AI）

## 五、依赖包

### UPM 包

| 包                                    | 版本      | 用途            |
| ------------------------------------ | ------- | ------------- |
| com.unity.render-pipelines.universal | 14.0.12 | URP 渲染管线      |
| com.unity.shadergraph                | 14.0.12 | 着色器图          |
| com.unity.ai.navigation              | 1.1.7   | NavMesh AI 导航 |
| com.unity.textmeshpro                | 3.0.7   | 文本渲染          |
| com.unity.timeline                   | 1.7.7   | 时间线           |
| com.unity.postprocessing             | 3.5.1   | 后处理           |

### 第三方框架

- **MFPS 2.0**（Lovatto Studio）— 多人 FPS 框架，版本 1.9.4
- **Photon PUN2** — 网络解决方案（源码内置，非 UPM 安装）

## 六、设计模式总结

| 模式    | 应用位置                                                                   |
| ----- | ---------------------------------------------------------------------- |
| 单例模式  | ResonanceManager                                                       |
| 观察者模式 | OnResonanceChanged 事件 + EnvironmentEffectManager 订阅                    |
| 策略模式  | ScriptableObject 配置（ResonanceConfig 等）解耦数值与逻辑                          |
| 模板方法  | EnvironmentEffectManager 抽象基类定义 UpdateEffect 流程                        |
| 继承扩展  | MeteoriteProjectile 继承 MFPS 弹道类、MeteoriteFacility 继承 ResonanceFacility |

## 七、潜在问题

1. **GroundFireManager.Start()** 中遍历 fireZones 列表时调用 RemoveAt，存在集合修改风险
2. **ItemDropManager.DropItem()** 按 lastIndex 顺序循环掉落而非随机选择，且 `index >= Length-1` 判断可能越界
3. **配置资产缺失**：各 *Config 的 .asset 实例在 Assets 下未找到，可能内嵌于场景或尚未创建
4. **MFPS 源码侵入**：对 bl_Bullet.cs、bl_ExplosionDamage.cs、bl_PlayerReferences.cs 的直接修改会增加框架升级难度

## 八、自研代码量统计

- 自研脚本：23 个 .cs 文件
- 修改的 MFPS 脚本：约 3 个（bl_Bullet.cs, bl_ExplosionDamage.cs, bl_PlayerReferences.cs）
- ScriptableObject 配置类：6 个
- 环境效果管理器：5 个（含基类）
- 陨石系统：3 个脚本
- 编辑器工具：2 个
- 其他工具：3 个

---

*报告生成时间：2026-09-24*  
*分析工具：WorkBuddy AI*
