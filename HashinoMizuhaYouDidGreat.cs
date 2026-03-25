using System;
using HarmonyLib;
using m2d;
using nel;
using XX;

namespace GenshinInCradle;

public class HashinoMizuhaYouDidGreat {
    /*
        public static string note1 = "如果你看到了这个，说明你正在反编译mod";
        public static string note2 = "这里放了一些哈酱写了的bug的修复";
        public static string note3 = "但是这些bug极难被触发，条件苛刻（例如一局游戏开100w次战斗）";
        public static string note4 = "因此这些bug虽然都是纯恶性bug，但是不上报的原因是没有必要修复";
        public static string note5 = "-- by e9ae9933";*/
    // [HarmonyPatch(typeof(STB), "Clear")]
    // [HarmonyPostfix]
    // public static void onClear(STB __instance)
    // {
    //     if(__instance.Capacity > 128)
    //         __instance.Capacity = 128;
    //     // We clear those capacity.
    // }
    [HarmonyPatch(typeof(RBase<IRunAndDestroy>), "Pop")]
    [HarmonyPrefix]
    public static void onPopPre(object __instance, ref bool __runOriginal, ref object __result) {
        if (!Configs.configFixMistakeAssign.Value) return;
        if (__instance is MGContainer mgc) {
            __result = mgc.Create();
            __runOriginal = false;
            typeof(MGContainer).GetMethod("Add", Utils.flags).Invoke(__instance, [__result, 64]);
        }
    }

    private static string getName(M2MagicCaster caster) {
        if (caster is M2Mover mv) return $"{mv.key}";
        return caster.GetType().Name;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MagicItem), "init")]
    public static void MagicItem_init(MagicItem __instance, int _id, M2MagicCaster _Caster, MGKIND _kind,
        MGHIT _hittype) {
        MagicItem mg = __instance;
        int lv = Configs.configMistakeAssignReportLevel.Value;
        bool returned = mg.killed || mg.closed;
        bool casterKilled = mg.Caster == null || mg.Caster as M2Mover == null;
        if (lv >= 1 && !returned && (lv >= 2 || !casterKilled)) {
            UILog.Instance.AddAlert($"错误覆写：将 {mg.id}; {getName(mg.Caster)}; {mg.kind} 覆写为 " +
                                    $"{_id}; {getName(_Caster)}; {_kind}");
            dumpMagic(mg, "error overwrite");
            Console.WriteLine($"错误覆写：将 {mg.id}; {getName(mg.Caster)}; {mg.kind} 覆写为 " +
                              $"{_id}; {getName(_Caster)}; {_kind}");
        }
    }

    private static void dumpMagic(MagicItem mg, string prefix = "") {
        if (mg == null) Console.WriteLine($"{prefix}; null");
        else
            Console.WriteLine(
                $"{prefix}; {mg.kind};{mg.Caster};{mg.casttime};{mg.closed};{mg.exploded};{mg.killed};{mg.id};{mg.Other}");
    }
}