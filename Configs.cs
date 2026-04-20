using BepInEx.Configuration;
using UnityEngine;

namespace GenshinInCradle;

public class Configs {
    public static ConfigEntry<KeyboardShortcut> configPauseShortcut;
    public static ConfigEntry<KeyboardShortcut> configFasterShortcut;
    public static ConfigEntry<KeyboardShortcut> configSlowerShortcut;
    public static ConfigEntry<KeyboardShortcut> configGetSeedShortcut;
    public static ConfigEntry<KeyboardShortcut> configSetSeedShortcut;
    public static ConfigEntry<int> configAdvancedFrames;
    public static ConfigEntry<bool> configAdvancedBool;
    public static ConfigEntry<bool> configShowDamage;
    public static ConfigEntry<bool> configSequenceScreenshot;
    public static ConfigEntry<string> configAdvancedBoxLayer;
    public static ConfigEntry<int> configAdvancedBoxMode;
    public static ConfigEntry<bool> configBurstFaintReverse;
    public static ConfigEntry<bool> configForceRandomSeedEnabled;
    public static ConfigEntry<uint> configForceRandomSeedValue;
    public static ConfigEntry<bool> configSuppressBurstFaint;
    public static ConfigEntry<bool> configSuppressDamageFlow;
    public static ConfigEntry<bool> configSuppressNonSensitive;
    public static ConfigEntry<bool> configDisplayMist;
    public static ConfigEntry<bool> configDisplayMapGrid;
    public static ConfigEntry<bool> configDisplayAttackBox;
    public static ConfigEntry<bool> configDisplayBoundingBox;
    public static ConfigEntry<bool> configDisplayMapBox;
    public static ConfigEntry<bool> configDisplayPlayer;
    public static ConfigEntry<bool> configDisplayMagicItem;
    public static ConfigEntry<int> configMistakeAssignReportLevel;
    public static ConfigEntry<bool> configFixMistakeAssign;
    public static ConfigEntry<string> configSuspicious;
    public static ConfigEntry<bool> configTheOneYouWhoSee;
    public static ConfigEntry<bool> configAlwaysShowCursor;
    public static ConfigEntry<bool> configSuppressGLFlush;
    public static ConfigEntry<bool> configAlwaysF7;
    public static ConfigEntry<KeyboardShortcut> configResetPassedFramesShortcut;
    public static ConfigEntry<KeyboardShortcut> configReplaySoundShortcut;
    public static ConfigEntry<KeyboardShortcut> configSaveSoundShortcut;
    public static ConfigEntry<KeyboardShortcut> configReadSoundShortcut;
    public static ConfigEntry<KeyboardShortcut> configGetAllItemsShortcut;
    public static ConfigEntry<KeyboardShortcut> configSaveImmediatelyShortcut;
    public static ConfigEntry<KeyboardShortcut> configShowEnemyListShortcut;
    public static ConfigEntry<KeyboardShortcut> configGetSeeds;
    public static ConfigEntry<int> configGetSeedNum;
    public static void init(ConfigFile Config) {
        configAdvancedBool = Config.Bind("高级功能", "高级功能总开关", false);
        configAdvancedBoxLayer = Config.Bind("高级功能", "碰撞箱图层", "M2D Camera -mover");
        configAdvancedBoxMode = Config.Bind("高级功能", "碰撞箱显示模式", 2,
            new ConfigDescription("1 为旧版（不建议），2 为新版", new AcceptableValueRange<int>(1, 2)));
        configDisplayMist = Config.Bind("高级功能", "显示毒雾判定箱", false);
        configDisplayMapGrid = Config.Bind("高级功能", "显示地图坐标网格", false);
        configDisplayAttackBox = Config.Bind("高级功能", "显示攻击判定箱", true);
        configDisplayBoundingBox = Config.Bind("高级功能", "显示实体碰撞箱", true);
        configDisplayMapBox = Config.Bind("高级功能", "显示地图碰撞箱", false);
        configDisplayPlayer = Config.Bind("高级功能", "显示玩家详细信息", true);
        configDisplayMagicItem = Config.Bind("高级功能", "显示魔法信息", false);
        configBurstFaintReverse = Config.Bind("高级功能", "圣光显示差值", false);
        configPauseShortcut = Config.Bind("TAS 相关", "暂停游戏", new KeyboardShortcut(KeyCode.End));
        configFasterShortcut = Config.Bind("TAS 相关", "加速 5 帧", new KeyboardShortcut(KeyCode.PageUp));
        configSlowerShortcut = Config.Bind("TAS 相关", "减速 5 帧", new KeyboardShortcut(KeyCode.PageDown));
        configAdvancedFrames = Config.Bind("TAS 相关", "帧率", 60,
            new ConfigDescription("滑动", new AcceptableValueRange<int>(1, 60)));
        configSequenceScreenshot = Config.Bind("TAS 相关", "自动截图", false);
        configGetSeedShortcut = Config.Bind("种子相关", "获取种子", new KeyboardShortcut(KeyCode.F12));
        configSetSeedShortcut = Config.Bind("种子相关", "修改种子", new KeyboardShortcut(KeyCode.F12, KeyCode.S));
        configGetAllItemsShortcut = Config.Bind("实用原版修改", "一键获取所有物品", new KeyboardShortcut(KeyCode.F12, KeyCode.I));
        configSuppressBurstFaint = Config.Bind("RNG 修改", "圣光晕厥抑制", false);
        configSuppressDamageFlow = Config.Bind("RNG 修改", "伤害浮动抑制", false);
        configMistakeAssignReportLevel = Config.Bind("漏洞修复", "错误分配告警等级", 0,
            new ConfigDescription("滑动", new AcceptableValueRange<int>(0, 2)));
        configFixMistakeAssign = Config.Bind("漏洞修复", "修复魔法错误分配（粗暴解法，未验证正确性）", false);
        configSuppressNonSensitive = Config.Bind("杂项", "超超健全", false);
        configShowDamage = Config.Bind("杂项", "伤害显示", false);
        configForceRandomSeedEnabled = Config.Bind("RNG 修改", "强行设置主随机数启用", false);
        configForceRandomSeedValue = Config.Bind("RNG 修改", "强行设置主随机数值", 0U);
        configSuspicious = Config.Bind("神秘开关", "神秘字符串", "");
        configTheOneYouWhoSee = Config.Bind("神秘开关", "你看到的我", false);
        configAlwaysShowCursor = Config.Bind("杂项", "不隐藏鼠标", false);
        configSuppressGLFlush = Config.Bind("杂项", "激进优化", false);
        configAlwaysF7 = Config.Bind("杂项", "自动 F7 (重启游戏生效)", true);
        configResetPassedFramesShortcut = Config.Bind("音效回放", "重置刻数", new KeyboardShortcut(KeyCode.F11, KeyCode.D));
        configReplaySoundShortcut = Config.Bind("音效回放", "开始回放", new KeyboardShortcut(KeyCode.F11, KeyCode.P));
        configSaveSoundShortcut = Config.Bind("音效回放", "保存回放", new KeyboardShortcut(KeyCode.F11, KeyCode.S));
        configReadSoundShortcut = Config.Bind("音效回放", "读取回放", new KeyboardShortcut(KeyCode.F11, KeyCode.R));
        configSaveImmediatelyShortcut = Config.Bind("杂项", "立刻保存",
            new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl, KeyCode.LeftAlt));
        configShowEnemyListShortcut = Config.Bind("种子相关", "战斗开始时显示出怪列表", new KeyboardShortcut(KeyCode.Home));
        configGetSeeds = Config.Bind("种子相关", "战斗开始时显示刷种子", new KeyboardShortcut(KeyCode.Insert));
        configGetSeedNum = Config.Bind("种子相关", "期望种子刷取数", 20000, new ConfigDescription("滑动", new AcceptableValueRange<int>(1, (int)2e9)));
    }
}