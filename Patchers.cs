using System;
using HarmonyLib;
using m2d;
using nel;
using UnityEngine;
using XX;
using static m2d.M2MoverPr;
using Random = System.Random;

namespace GenshinInCradle;

internal class Patchers {
    //忿怒的报偿
    private static int cnt1;
    private static long beginmillis2, cnt2 = -1;
    private static bool bypassInitShotgun;

    private static bool isEnhancerEnabled(string id) {
        try {
            if (Configs.configTheOneYouWhoSee.Value)
                return true;
            return false;
            int g = (NelM2DBase.Instance as NelM2DBase).IMNG.getInventoryEnhancer()
                .getTopGrade(NelItem.GetById("Enhancer_" + id, true));
            return g >= 2;
        }
        catch (Exception e) {
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PR), "runPre")]
    public static void onRunPre(PR __instance) {
        PR pr = __instance;
        if (isEnhancerEnabled("neuvillette")) {
            M2PrSkill skill = __instance.Skill;
            MagicItem mg = skill.getCurMagic();
            if (Input.GetKeyDown(KeyCode.Backspace)) cnt1 = 0;
            if (mg == null || !(skill.getChantCompletedRatio() >= 1) || mg.kind != MGKIND.WHITEARROW) return;
            Random rand = new();
            cnt1++;
            if (cnt1 % 3 != 0) return;
            Map2d mp = __instance.Mp;
            double d = double.PositiveInfinity;
            M2Mover target = null;
            foreach (M2Mover mv in mp.getVectorMover())
                if (mv is NelEnemy && !mv.destructed) {
                    double d2 = (mv.x - pr.x) * (mv.x - pr.x) + (mv.y - pr.y) * (mv.y - pr.y);
                    // d2 = rand.NextDouble();
                    if (d > d2) {
                        target = mv;
                        d = d2;
                    }
                }

            double sa = rand.NextDouble() * 2 * Math.PI + Math.PI;
            double q = Math.Sin(3 * sa) * 0.1 + 1;
            double r = 4.25 * q, r2x = 4 * q, r2y = 2 * q;
            Console.WriteLine($"target is {target}");
            pr.Skill.PtcVar("cx", __instance.x).PtcVar("cy", __instance.y).PtcVar("time", 36f / 2 / 1.5f);
            // if(rand.NextDouble() < 0.1)
            pr.Skill.PtcSTTimeFixed("burst_prepare", 0f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
            double x = pr.x, y = pr.y;
            if (target != null) {
                pr.moveBy((float)(target.x - pr.x - r * Math.Cos(sa)), (float)(target.y - pr.y + r * Math.Sin(sa)));
                x = target.x - pr.x - r2x * Math.Cos(sa);
                y = target.y - pr.y + r2y * Math.Sin(sa);
            }

            __instance.NM2D.Cam.setQuake(40, 20, 0);
            for (int p = 0; p < 4; p++) {
                MagicItem mg3 = mg.createNewMagic(null, MGKIND.WHITEARROW, (float)x, (float)y, false);
                mg3.reduce_mp = 20;
                mg3.run(1);
                int i;
                for (i = 0; i < 100 && mg3.phase < 2; i += 10)
                    mg3.run(10);
                mg3.sa = p == 0 ? (float)sa : (float)rand.NextDouble() * 1000;
                mg3.Atk0.hpdmg0 = mg3.Atk0.hpdmg0;
                mg3.sz *= 3;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PR), "runPre")]
    public static void onRunPre2(PR __instance) {
        PR pr = __instance;
        if (isEnhancerEnabled("neuvillette1")) {
            Map2d mp = __instance.Mp;
            M2PrSkill skill = __instance.Skill;
            MagicItem mg = skill.getCurMagic();
            if (mg == null || !(skill.getChantCompletedRatio() >= 1) || mg.kind != MGKIND.WHITEARROW) {
                beginmillis2 = 0;
                cnt2 = -1;
                return;
            }

            if (beginmillis2 == 0) beginmillis2 = millis();
            const double interval = 60.0 / 128.0 * 1000.0;
            const int steps = 8;
            if (cnt2 * interval < millis() - beginmillis2) {
                cnt2++;
                int step = (int)(cnt2 % steps);
                double r = 10.25 + 0.5 * step;
                int n = 6 + step;
                for (int i = 0; i < n; i++) {
                    MagicItem m = mg.MGC.setMagic(pr, MGKIND.FIREBALL, MGHIT.PR | MGHIT.IMMEDIATE);
                    float theta = 2 * Mathf.PI * i / n;
                    m.sx = (float)(pr.x + Math.Cos(theta) * r);
                    m.sy = (float)(pr.y + Math.Sin(theta) * r);
                    m.reduce_mp = 10;
                    pr.Skill.prepareMagicForCooking(m, m, false);
                    m.run(0f);
                    m.t = 140f;
                    // m.Atk1.hpdmg0 = (int)(m.Atk1.hpdmg0 * 1.56);
                    // m.Mn._1.thick *= 1.56f;
                    m.Atk0.hpdmg0 = m.Atk0.hpdmg0 / 5;
                    m.run(0f);
                }
            }
        }
    }

    //金玉，礼予天地四方
    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2Shield), "run")]
    public static void onRun2(M2Shield __instance, ref float ___pow, ref float power_progress_level,
        ref float ___alpha_) {
        // if (isEnhancerEnabled("zhongli"))
        // 	if (___pow >= 0 && ___alpha_ > 0)
        // 	{
        // 		power_progress_level = 0;
        // 		__instance.appearable_time = 260f * 1.23f;
        // 	}
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(M2Shield), "checkShield")]
    public static void onCheckShield2(ref float val, M2Shield.RESULT __result, M2Shield __instance) {
        // if (isEnhancerEnabled("zhongli"))
        // 	if (__result == M2Shield.RESULT.GUARD || __result == M2Shield.RESULT.GUARD_CONTINUE)
        // 	{
        // 		if (__instance.Mv is PR pr)
        // 		{
        // 			int value = (int)(val);
        // 			pr.cureHp(value);
        // 			pr.Mp.DmgCntCon.Make(pr, value, 0);
        // 		}
        // 	}
    }

    //取胜者，大小通吃
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MgWhiteArrow), "run")]
    public static void onRun(MgWhiteArrow __instance, MagicItem Mg) {
        // if (isEnhancerEnabled("yelan"))
        // {
        // 	if (Mg.phase < 0)
        // 	{
        // 		MagicItem m = Mg.MGC.setMagic(Mg.Caster, MGKIND.FIREBALL, MGHIT.PR | MGHIT.IMMEDIATE);
        // 		m.sx = Mg.sx;
        // 		m.sy = Mg.sy;
        // 		if (Mg.MGC.M2D.getPrNoel() is PR pr)
        // 		{
        // 			pr.Skill.prepareMagicForCooking(m, m, false);
        // 			int add0 = 0;
        // 			pr.Skill.getOverChargeSlots().getMana(224f, ref add0);
        // 		}
        // 		m.run(0f);
        // 		m.t = 140f;
        // 		m.Atk1.hpdmg0 = (int)(m.Atk1.hpdmg0 * 1.56);
        // 		m.Mn._1.thick *= 1.56f;
        // 		m.run(0f);
        // 		Mg.kill();
        // 	}
        // }
    }

    // 降魔·护法夜叉
    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2PrSkill), "publishShotgunHit")]
    public static void onPublishShotgunHit(M2PrSkill __instance, MagicItem Mg, M2Ray.M2RayHittedItem HitItem,
        float right_agR, bool replace_normal, ref int reduce_mag) {
        return;
        if (isEnhancerEnabled("xiao"))
            if (__instance.Pr.get_current_state() == PR.STATE.COMET_SHOTGUN)
                reduce_mag = 0;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MDAT), "initShotGun")]
    public static void onInitShotGun(ref bool __runOriginal) {
        return;
        __runOriginal = !bypassInitShotgun;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2PrSkill), "runState")]
    public static void onRunState(bool first, ref float t, ref PR_MNP manip, M2PrSkill __instance) {
        return;
        if (isEnhancerEnabled("xiao"))
            if (__instance.Pr.get_current_state() == PR.STATE.COMET_SHOTGUN) {
                if (__instance.hasD(DECL.FLAG2)) {
                    if (t < 14f)
                        t = 14f;
                }
                else if (!__instance.hasD(DECL.FLAG0)) {
                    __instance.PtcVar("cx", __instance.x).PtcVar("cy", __instance.y).PtcVar("time", 36f / 2 / 1.5f);
                    __instance.PtcSTTimeFixed("burst_prepare", 0f, PtcHolder.PTC_HOLD.NORMAL,
                        PTCThread.StFollow.FOLLOW_C);
                    t = 19.999999f;
                }

                if (!__instance.hasD(DECL.FLAG2))
                    if (t >= 20f)
                        if (__instance.hasFoot() ||
                            __instance.Pr.hit_wall_collider ||
                            __instance.Pr.wallHitted(AIM.B) ||
                            !__instance.Pr.canStand((int)__instance.x, (int)(__instance.mbottom + 0.03f)))
                            try {
                                //Console.WriteLine("on");
                                MagicItem magicItem =
                                    __instance.NM2D.MGC.setMagic(__instance.Pr, MGKIND.PR_COMET, (MGHIT)1025);
                                MDAT.initShotGun(magicItem, __instance.getCurMagic(), __instance.getHoldingMp(true),
                                    __instance.magic_returnable_mp, __instance.getCurrentCaneEquip());
                                magicItem.Atk0.hit_ptcst_name = "";
                                __instance.prepareMagicForCooking(magicItem, null, true);
                                __instance.NM2D.Cam.setQuake(40, 20, 0);
                                __instance.PtcST("burst_after");
                                // __instance.PtcST("burst_after", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
                                // __instance.PtcST("burst_after", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
                                __instance.PtcVar("by", __instance.Pr.mbottom).PtcST("comet_ground_bump");
                                MagicItem mg = magicItem;
                                mg.sz *= 10f;
                                mg.Atk0.hpdmg0 = (int)(mg.Atk0.hpdmg0 * 0.6);

                                bypassInitShotgun = true;
                                mg.run(1f);
                                MagicItem.runTackle(mg, 1);
                                bypassInitShotgun = false;

                                mg.kill();
                                mg.changeRay(null);
                                mg.sz = 0;
                            }
                            catch (Exception e) {
                                bypassInitShotgun = false;
                                Console.WriteLine(e.ToString());
                            }
            }
    }

    // 幽蝶能留一缕芳
    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2Attackable), "applyHpDamage", typeof(int), typeof(bool), typeof(AttackInfo))]
    public static void onApplyHpDamage(M2Attackable __instance, ref int ___hp, ref int val) {
        if (isEnhancerEnabled("hutao"))
            if (__instance is PR) {
                PR pr = __instance as PR;
                if (val >= ___hp) {
                    val = ___hp - 100;
                    pr.Ser.CureAll();
                    pr.cureHp(0);
                    pr.changeState(PR.STATE.BURST);
                    PR_MNP p = 0;
                    float f = 0.000000001f;
                    bool _ = false;
                    pr.Skill.runState(true, ref f, ref p, ref _);
                    // Utils.setField(pr,"t_state",f);
                    pr.Skill.getBurstSelector().fainted_ratio = 0;
                    pr.GaugeBrk.Cure(9999999);
                    pr.cureMp((int)Math.Ceiling(pr.get_maxmp()));
                    // pr.GetType().GetMethod("setDamageCounter", flags).Invoke(pr, new object[] { -val, 0, DC.NORMAL, null });
                }
            }
    }

    public static long millis() {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
}