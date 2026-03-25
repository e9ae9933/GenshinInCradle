using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using m2d;
using nel;
using Application = UnityEngine.Application;

[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: IgnoresAccessChecksTo("unsafeAssem")]
[assembly: IgnoresAccessChecksTo("ttr")]

namespace GenshinInCradle;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class GenshinInCradle : BaseUnityPlugin {
    internal new static ManualLogSource Logger;

    // public static ConfigEntry<KeyboardShortcut> configGetAllItemsShortcut;

    // public static ConfigEntry<bool> configSequenceScreenshot;

    public static bool paused;

    public void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        try {
            Configs.init(this.Config);
            Logger.LogInfo("patching GenshinInCradle");
            Harmony.CreateAndPatchAll(typeof(GenshinInCradle));
            Logger.LogInfo("patching SeedSet");
            Harmony.CreateAndPatchAll(typeof(SeedSet));
            Logger.LogInfo("patching Patchers");
            Harmony.CreateAndPatchAll(typeof(Patchers));
            Logger.LogInfo("patching Advanced");
            Harmony.CreateAndPatchAll(typeof(UnifontRenderer));
            Logger.LogInfo("patching UnifontRenderer");
            Harmony.CreateAndPatchAll(typeof(Advanced));
            Logger.LogInfo("patching MoreModules");
            Harmony.CreateAndPatchAll(typeof(MoreModules));
            // Logger.LogInfo("patching SoundReplay");
            // Harmony.CreateAndPatchAll(typeof(SoundReplay));
            Logger.LogInfo("patching TestPatches1");
            Harmony.CreateAndPatchAll(typeof(TestPatches1));
            Logger.LogInfo("patching TestPatches2");
            Harmony.CreateAndPatchAll(typeof(TestPatches2));
            Logger.LogInfo("patching Screenshot");
            Harmony.CreateAndPatchAll(typeof(Screenshot));
            Logger.LogInfo("patching HashinoMizuhaYouDidGreat");
            Harmony.CreateAndPatchAll(typeof(HashinoMizuhaYouDidGreat));

            Screenshot.StartSequence(this);
        }
        catch (Exception e) {
            MessageBox.Show("GenshinInCradle fail\n" + e);
        }

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void Update() {
        SoundReplay.tick();
        if (Configs.configFasterShortcut.Value.IsDown()) Configs.configAdvancedFrames.Value += 5;

        if (Configs.configSlowerShortcut.Value.IsDown()) Configs.configAdvancedFrames.Value -= 5;

        if (Configs.configPauseShortcut.Value.IsDown()) {
            paused = !paused;
            Map2d.setTimeScale(1);
        }

        if (Configs.configSaveImmediatelyShortcut.Value.IsDown()) {
            NelM2DBase nm2d = M2DBase.Instance as NelM2DBase;
            if (nm2d != null) {
                UILogRow row = COOK.autoSave(nm2d, false, true);
            }
        }
        if (Configs.configGetAllItemsShortcut.Value.IsDown()) {
            NelM2DBase nm2d = (NelM2DBase)M2DBase.Instance;
            NelItemManager imng = nm2d.IMNG;
            ItemStorage storage = imng.getHouseInventory();
            foreach (KeyValuePair<string, NelItem> pair in NelItem.OData) {
                for (int g = 0; g < 5; g++) {
                    int n = 1000 - storage.getCount(pair.Value, g);
                    if (n > 0) storage.Add(pair.Value, n, g);
                }
            }
            UILog.Instance.AddAlert("已获得所有物品……");
        }
        if (Configs.configSetSeedShortcut.Value.IsDown()) {
            try {
                uint[] s1 = Utils.getField<uint[]>(NightController.Xors, "Randseed");
                uint[] s2 = Utils.getField<uint[]>(NightController.Xors, "RandseedFirst");
                int k = SeedSet.randCount;
                string str = Utils.ShowInputDialog($"""
                                                    输入种子 2。
                                                    种子 2 应当是用逗号和空格隔开的 4 个非负整数。
                                                    当前正在使用偏移系数 {k}，请确认偏移系数无误（正常来说，应该是144）
                                                    """, null);
                string[] seq = str.Split([',', ' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries);
                uint[] target = new uint[4];
                for (int i = 0; i < 4; i++)
                    target[i] = uint.Parse(seq[i]);
                for (int i = 0; i < 4; i++)
                    s1[i] = s2[i] = target[i];
                for (int i = 0; i < k; i++)
                    NightController.Xors.get0();
                SeedSet.randCount = k;
                UILog.Instance.AddLog("偏移系数: " + SeedSet.randCount);
                UILog.Instance.AddLog("种子1: " + string.Join(",", s1));
                UILog.Instance.AddLog("种子2: " + string.Join(",", s2));
            }
            catch (Exception ex) {
                MessageBox.Show("无效字符串。\n请使用空格或tab或逗号隔开的4个数字表示种子2。\n" + ex, null, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        else if (Configs.configGetSeedShortcut.Value.IsDown()) {
            uint[] s1 = Utils.getField<uint[]>(NightController.Xors, "Randseed");
            uint[] s2 = Utils.getField<uint[]>(NightController.Xors, "RandseedFirst");
            StringBuilder sb = new();
            sb.AppendFormat("种子1: {0} {1} {2} {3}\n", s1[0], s1[1], s1[2], s1[3]);
            sb.AppendFormat("种子2: {0} {1} {2} {3}\n", s2[0], s2[1], s2[2], s2[3]);
            sb.AppendLine("到现在已经进行的随机次数: " + SeedSet.randCount);
            MessageBox.Show(sb.ToString());
        }

        TestPatches1.tick();
        TestPatches2.tick();
        Application.targetFrameRate = Configs.configAdvancedFrames.Value;
    }
}