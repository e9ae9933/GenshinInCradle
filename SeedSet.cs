using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using HarmonyLib;
using nel;
using nel.smnp;
using XX;
using Application = UnityEngine.Application;
using Random = System.Random;

namespace GenshinInCradle;

using Random = Random;

public class SeedSet {
    public static int randCount;
    public static Dictionary<XorsMaker, long> randCounts = new();

    public static Dictionary<string, EnemySummoner> sfToSummoner = new();

    [HarmonyPatch(typeof(XorsMaker), "get0")]
    [HarmonyPostfix]
    public static void onGet0(XorsMaker __instance, ref uint __result) {
        randCounts.TryGetValue(__instance, out long cnt);
        randCounts[__instance] = cnt + 1;
        if (__instance == NightController.Xors) randCount++;

        if (Configs.configForceRandomSeedEnabled.Value && __instance == X.Xors)
            __result = Configs.configForceRandomSeedValue.Value;
    }

    [HarmonyPatch(typeof(XorsMaker), "readBinaryFrom")]
    [HarmonyPrefix]
    public static void onReadBinaryFrom(XorsMaker __instance) {
        if (__instance == NightController.Xors) randCount = 0;
    }

    [HarmonyPatch(typeof(EnemySummoner), "activate")]
    [HarmonyPostfix]
    public static void onActivateOuter(EnemySummoner __instance) {
        if (!Configs.configShowEnemyListShortcut.Value.IsPressed()) return;
        SummonerPlayer player = __instance.getPlayer();
        List<SmnEnemyKind> AKindRest = new(Utils.getField<List<SmnEnemyKind>>(player, "AKindRest"));
        SmnPoint[] ___ASmnPos = Utils.getField<SmnPoint[]>(player, "ASmnPos");
        StringBuilder sb = new();
        float weight_add = 0;
        foreach (SmnEnemyKind smnEnemyKind in AKindRest) {
            string id = smnEnemyKind.enemyid;
            if (id.Contains("_"))
                id = id.Substring(0, id.LastIndexOf("_"));
            string l10n = TX.Get("Enemy_" + id, id);
            sb.Append(l10n).Append(",");
            if (smnEnemyKind.isOverDrive())
                sb.Append(smnEnemyKind.thunder_overdrive ? "雷暴 " : " ").Append(smnEnemyKind.pre_overdrive ? "原生 " : " ")
                    .Append("污染体").Append(",");
            /*
            SmnPoint point=getTarget(player,smnEnemyKind,null,___ASmnPos,ref weight_add);
            if (point == null) sb.Append("获取位置失败,");
            else
            {
                sb.Append($"x={point.x} y={point.y} weight_add={weight_add},");
            }*/
            if ((smnEnemyKind.nattr & ENATTR.ATK) > 0) sb.Append("攻").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.DEF) > 0) sb.Append("防").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.MP_STABLE) > 0) sb.Append("稳").Append(",");

            if ((smnEnemyKind.nattr & ENATTR.FIRE) > 0) sb.Append("火").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.ICE) > 0) sb.Append("冰").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.THUNDER) > 0) sb.Append("雷").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.SLIMY) > 0) sb.Append("粘").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.ACME) > 0) sb.Append("毒").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.INVISIBLE) > 0) sb.Append("隐").Append(",");
            if ((smnEnemyKind.nattr & ENATTR.BIG) > 0) sb.Append("巨").Append(",");

            sb.AppendLine();
        }

        string s = "刷怪列表\n未翻转。\n" + sb;
        MessageBox.Show(s);
    }

    [HarmonyPatch(typeof(NightController.SummonerData), "getSummoner")]
    [HarmonyPostfix]
    public static void onGetSummoner(string sf_key, EnemySummoner __result) {
        if (__result != null) {
            Console.WriteLine("Found summoner " + sf_key);
            sfToSummoner[sf_key] = __result;
        }
    }

    [HarmonyPatch(typeof(EnemySummoner), "activateInner")]
    [HarmonyPrefix]
    public static void onActivate(EnemySummoner __instance, EfParticleFuncCalc ___FuncBase, CsvReaderA ___CR,
        ref bool bgm_replaced,
        ref M2LpSummon ___Lp, M2LpSummon _Lp, ref bool __runOriginal) {
        try {
            if (!Configs.configGetSeeds.Value.IsPressed()) return;
            int n = Configs.configGetSeedNum.Value;
            int k = randCount;
            string battleString = __instance.GetManager().getSummonerScript(__instance.key, out _) ?? "";
            DialogResult ans = MessageBox.Show
            ($"""
              检测到战斗点{__instance.key}
              {battleString}
              我们会进行{n}次生成。
              使用偏移 {k}
              点击确定生成。
              点击取消正常出怪。
              """, null,
                MessageBoxButtons.YesNo);
            if (ans == DialogResult.Yes) {
                __runOriginal = false;
                Console.WriteLine("Starting attempt " + n);
                FileStream binaryStream;
                string fileName;
                fileName = $"{__instance.key}.aicseed";

                binaryStream = File.OpenWrite(fileName);
                BufferedStream bufferedStream = new(binaryStream, 16 * 1024 * 1024);

                ___Lp = _Lp;
                EnemySummoner.ActiveScript = __instance;
                Random random = new();
                Stopwatch sw3 = Stopwatch.StartNew();
                uint[] s11 = new uint[4];
                uint[] s1 = NightController.Xors.Randseed;
                uint[] s2 = NightController.Xors.RandseedFirst;

                long writtenLength = 0;
                for (int i = 1; i <= n; i++) {
                    for (int j = 0; j < 4; j++) {
                        byte[] b = new byte[4];
                        random.NextBytes(b);
                        s1[j] = s2[j] = BitConverter.ToUInt32(b, 0);
                    }

                    for (int j = 1; j <= k; j++) NightController.Xors.get0();
                    for (int j = 0; j < 4; j++) s11[j] = s1[j];

                    SummonerPlayerAdvanced s = new(__instance, ___FuncBase, ___CR, out bgm_replaced);
                    s.close(false);
                    List<SmnEnemyKind> AKindRest = s.AKindRest;
                    byte[] output = getBytes(i, AKindRest, s1, s11, s2);
                    bufferedStream.Write(output, 0, output.Length);
                    writtenLength += output.Length;

                    if (i == 1 || i == n || i % 1000 == 0) {
                        double elapsedSeconds = sw3.ElapsedMilliseconds / 1000.0;
                        double t = (double)sw3.ElapsedMilliseconds / i;
                        double estimateSeconds = elapsedSeconds * (n - i) / i;
                        Console.WriteLine(
                            $"Attempt #{i} / {n} with {t:F2}ms / {1000 / t:F2}/s elapsed {elapsedSeconds:F2}s est. {estimateSeconds:F2}s, {writtenLength / 1048576.0:F2} MiB");
                    }
                }

                EnemySummoner.ActiveScript = null;

                bufferedStream.Flush();
                bufferedStream.Close();
                binaryStream.Close();
                MessageBox.Show($"已经将生成信息写入游戏目录下的{__instance.key}中。");
            }
        }
        catch (Exception e) {
            MessageBox.Show(e.Message + "\n" + e.StackTrace);
            Application.Quit();
        }
    }

    public static byte[] getBytes(int id, List<SmnEnemyKind> AKindRest, uint[] s3, uint[] s1, uint[] s2) {
        MemoryStream ms = new();
        BinaryWriter bw = new(ms);
        bw.Write(0x12345678U);
        bw.Write(id);
        for (int i = 0; i < 4; i++) bw.Write(s1[i]);
        for (int i = 0; i < 4; i++) bw.Write(s2[i]);
        for (int i = 0; i < 4; i++) bw.Write(s3[i]);
        bw.Write(AKindRest.Count);
        foreach (SmnEnemyKind smnEnemyKind in AKindRest) {
            string enemyid = smnEnemyKind.enemyid;
            int nattr = (int)smnEnemyKind.nattr;
            byte overdrive = (byte)((smnEnemyKind.pre_overdrive ? 1 : 0) | (smnEnemyKind.thunder_overdrive ? 2 : 0));
            bw.Write(enemyid);
            bw.Write(nattr);
            bw.Write(overdrive);
        }

        bw.Write(0x87654321U);
        bw.Close();
        ms.Close();
        return ms.ToArray();
    }
}