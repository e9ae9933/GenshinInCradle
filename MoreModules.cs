using System;
using System.Linq;
using HarmonyLib;
using m2d;
using nel;
using UnityEngine;
using XX;
using Logger = XX.Logger;

namespace GenshinInCradle;

public class MoreModules {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIPictureBase), "emotSuperSensitive")]
    public static void onGetEmotSP(UIPictureBase __instance, ref UIPictureBase.EMSTATE st, ref UIEMOT __result) {
        if (!Configs.configSuppressNonSensitive.Value) return;
        ref UIEMOT v = ref __result;
        st &= UIPictureBase.EMSTATE.NORMAL |
              UIPictureBase.EMSTATE.BATTLE |
              UIPictureBase.EMSTATE.LOWHP |
              UIPictureBase.EMSTATE.SP_SENSITIVE;
        v = UIEMOT.STAND;
        UIEMOT[] allowed = [UIEMOT.STAND, UIEMOT.BENCH];
        if (!allowed.Contains(v))
            v = UIEMOT.STAND;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIPictureBase), "applyDamage")]
    public static void onApplyDamage(ref bool __runOriginal) {
        if (!Configs.configSuppressNonSensitive.Value) return;
        __runOriginal = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AttackInfo), "shuffleHpMpDmg")]
    public static void onShuffle(AttackInfo __instance, M2Mover Target) {
        if (!Configs.configSuppressDamageFlow.Value) return;
        if (__instance.AttackFrom is PR) {
            UILog.Instance.AddLog(
                $"已抑制 {__instance.AttackFrom?.name ?? "null"} 的伤害浮动 [{__instance.damage_randomize_min * 100:F2}%, 1]");
            __instance.damage_randomize_min = 1;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AttackInfo), "shuffleHpMpDmg")]
    public static void onShuffle2(AttackInfo __instance, M2Mover Target) {
        if (!Configs.configSuppressDamageFlow.Value) return;
        if (__instance.AttackFrom is PR)
            UILog.Instance.AddLog($"基础伤害 {__instance.hpdmg0} / 初次结算伤害 {__instance.hpdmg_current}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2PrADmg), "applyDamage",
        [typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool), typeof(string), typeof(bool), typeof(bool)],
        [
            ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal
        ])]
    public static void onM2Prd(M2PrADmg __instance, NelAttackInfo Atk, ref HITTYPE add_hittype, bool force,
        string fade_key, bool decline_ui_additional_effect, bool from_press_damage) {
        if (!Configs.configShowDamage.Value) return;
        Console.WriteLine($"hpdmg {Atk._hpdmg} when hpdmg0 {Atk.hpdmg0} hpcur {Atk.hpdmg_current}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NelEnemy), "applyHpDamage")]
    public static void onApplyHpDamage(NelEnemy __instance, int val, ref int mpdmg, bool force, NelAttackInfo Atk) {
        if (!Configs.configShowDamage.Value) return;
        if (Atk.AttackFrom is PR) {
            double ratio = (double)val / Atk._hpdmg;
            UILog.Instance.AddLog(
                $"{NDAT.getEnemyName(__instance.id)} 伤害 {Atk._hpdmg} -> {val} / 伤害倍率 {100 * ratio:F2}%");
        }
    }

    [HarmonyPatch(typeof(M2Ser), "Add")]
    [HarmonyPrefix]
    public static void suppress(M2Ser __instance, SER ser, ref bool __runOriginal) {
        if (!Configs.configSuppressBurstFaint.Value) return;
        if (ser == SER.BURST_TIRED) {
            float ratio = (__instance.Mv as PR)?.Skill?.getBurstSelector()?.fainted_ratio ?? -1;
            if (ratio >= 0) {
                if (ratio < 1) {
                    __runOriginal = false;
                    UILog.Instance.AddLog($"已抑制概率为 {ratio * 100:F2}% 的晕厥。");
                }
                else {
                    UILog.Instance.AddLog($"未抑制概率为 {ratio * 100:F2}% 的晕厥。");
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Cursor), "visible", MethodType.Setter)]
    public static void onVisible(ref bool __0) {
        if (!Configs.configAlwaysShowCursor.Value) return;
        __0 = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GL), "Flush")]
    public static void onFlush(ref bool __runOriginal) {
        if (!Configs.configSuppressGLFlush.Value) return;
        // Console.WriteLine($"Suppressed useless GL.Flush()");
        __runOriginal = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Logger), "fineTimeStampViewer")]
    public static void onFineTimeStampViewer() {
        if (!Configs.configAlwaysF7.Value) return;
        X.DEBUGANNOUNCE = true;
        X.DEBUGTIMESTAMP = true;
    }
}