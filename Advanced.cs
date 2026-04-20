using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using m2d;
using nel;
using nel.smnp;
using UnityEngine;
using XX;
using Logger = XX.Logger;

namespace GenshinInCradle;

public class Advanced {
    public static long passedFrames;
    private static long lastTicks;
    private static long nowTicks;
    public static double passedGt;
    public static double lastGt;
    public static List<M2Ray> rayList = new();

    private static bool isAdvancedEnabled() {
        return Configs.configAdvancedBool.Value;
    }

    [HarmonyPatch(typeof(M2Ray), "CastRayAndCollider")]
    [HarmonyPostfix]
    public static void cast(ref RaycastHit2D AHit, M2Ray __instance) {
        if (!isAdvancedEnabled()) return;
        rayList.Add(__instance);
        // Console.WriteLine("cast! raylist "+(rayList.Count));
    }

    public static void cast2(M2Ray instance, out float num, out float num2, out float num3, out float num4) {
        RAYSHAPE shape = instance.shape;
        float radius = instance.radius;
        num = num4 = 0;
        num2 = num3 = 1;
        if (shape == RAYSHAPE.RECT) goto end;
        if (shape == RAYSHAPE.DAIA) {
            num4 = 45f;
        }
        else if (shape == RAYSHAPE.RECT_HH) {
            num2 = 1.33f;
        }
        else if (shape == RAYSHAPE.RECT_HH2) {
            num2 = 2f;
        }
        else if (shape == RAYSHAPE.RECT_HH2C) {
            num3 = 0.5f;
        }
        else if (shape == RAYSHAPE.RECT_VH) {
            num3 = 1.33f;
        }
        else {
            num2 = 0f;
            num3 = radius;
        }

        end: ;
    }

    private static Tuple<float, float> toScreen(ref float x, ref float y) {
        NelM2DBase m2d = M2DBase.Instance as NelM2DBase;
        Map2d mp = m2d.curMap;
        x = mp.ux2effectScreenx(mp.map2ux(x)) * 64;
        y = mp.uy2effectScreeny(mp.map2uy(y)) * 64;
        return new Tuple<float, float>(x, y);
    }

    public static void renderWholeMoverMode2(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        List<M2RenderTicket> list = ___AADob[draw_id];
        NelM2DBase m2d = M2DBase.Instance as NelM2DBase;
        Map2d mp = m2d.curMap;
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        if (Configs.configDisplayAttackBox.Value)
            foreach (M2Ray ray in rayList) {
                MeshDrawer md2 = new();
                md2.activate("attack_box_refreshed", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
                md2.Col = C32.d2c(0xFFFFFFFFU);
                if (ray.Caster == null || ray.Caster.transform == null) {
                    Console.WriteLine("returned because of null matrix");
                    continue;
                }

                float num, num2, num3, num4;
                float clenb = mp.CLENB;
                cast2(ray, out num, out num2, out num3, out num4);
                float x = ray.getUPos().x / clenb * 64, y = ray.getUPos().y / clenb * 64;
                float ratio = 64;
                if (num2 == 0f) {
                    float sx = x * clenb, sy = y * clenb, r = num3 * ratio;
                    float tx = sx + ray.Dir.x * ray.len * ratio;
                    float ty = sy + ray.Dir.y * ray.len * ratio;
                    float dis = (float)Math.Sqrt((sx - tx) * (sx - tx) + (sy - ty) * (sy - ty));
                    List<Vector4> v = new();
                    const int n = 12;
                    for (int j = 0; j <= n; j++) {
                        double angle = Math.PI * j / n + Math.PI / 2;
                        v.Add(new Vector4((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r, 0, 1));
                    }

                    for (int j = 0; j <= n; j++) {
                        double angle = Math.PI * j / n - Math.PI / 2;
                        v.Add(new Vector4((float)Math.Cos(angle) * r + dis, (float)Math.Sin(angle) * r, 0, 1));
                    }

                    int m = v.Count;
                    Quaternion q = Quaternion.AngleAxis((float)(Math.Atan2(ty - sy, tx - sx) * 180.0 / Math.PI),
                        new Vector3(0, 0, 1));
                    Matrix4x4 transform = Matrix4x4.Translate(new Vector3(sx, sy, 0)) * Matrix4x4.Rotate(q);
                    md2.Col = C32.d2c(0xBFFF0000);
                    for (int j = 2; j < m; j++) {
                        Vector4 v0 = transform * v[0];
                        Vector4 v1 = transform * v[j];
                        Vector4 v2 = transform * v[j - 1];
                        md2.Triangle(v0.x, v0.y, v1.x, v1.y, v2.x, v2.y);
                    }

                    md2.Col = C32.d2c(0xFF00FF00);
                    md2.Line(sx, sy, tx, ty, 3);
                }
                else {
                    float sx = x * clenb, sy = y * clenb;
                    float tx = sx + ray.Dir.x * ray.len * ratio;
                    float ty = sy + ray.Dir.y * ray.len * ratio;
                    float dis = (float)Math.Sqrt((sx - tx) * (sx - tx) + (sy - ty) * (sy - ty));
                    num2 *= ray.radius * ratio;
                    num3 *= ray.radius * ratio;

                    md2.Col = C32.d2c(0xBFFF0000);
                    md2.Box(sx, sy, num2 * 2, num3 * 2);
                    md2.Col = C32.d2c(0xFF00FF00);
                    md2.Line(sx, sy, tx, ty, 3);
                }

                BLIT.RenderToGLImmediate001(md2, setpass: true);
            }

        rayList.Clear();
        //ray end
        // map start
        if (Configs.configDisplayMapBox.Value) {
            MeshDrawer mdb = new();
            mdb.activate("map_box", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            // M2FootManager foot = mp.Pr.getFootManager();
            // Console.WriteLine($"{foot.getFootTime()} {foot.vague_foot} {mp.Pr.hasFoot()} {mp.Pr.canJump()}");
            Action<BCCLine, uint> draw = (Bcc, col) => {
                float shiftx, shifty;
                Bcc.BCC.getBaseShift(out shiftx, out shifty);
                bool flag = mp.Pr.getPhysic().getFootManager().get_FootBCC() == Bcc;
                // float num = mp.Pr.mleft + shiftx;
                // float num2 = mp.Pr.mtop + shifty;
                // float num3 = mp.Pr.mright + shiftx;
                // float num4 = (flag ? mp.Pr.mbottom : X.Mx(mp.Pr.y, mp.Pr.mbottom - 0.22f)) + shifty;
                if (Bcc.AMapDmg != null)
                    foreach (M2MapDamageContainer.M2MapDamageItem ditem in Bcc.AMapDmg) {
                        mdb.Col = C32.d2c(0xFFFF7F00U);
                        float extendx = 0.14f;
                        float extendy = Bcc._xd != 0 || !flag && Bcc.foot_aim == AIM.B ? -0.23f : extendx;
                        float l = ditem.x - extendx;
                        float r = ditem.right + extendx;
                        float u = ditem.y - extendy;
                        float d = ditem.bottom + extendy;
                        toScreen(ref l, ref u);
                        toScreen(ref r, ref d);
                        mdb.Line(l, u, l, d, 2);
                        mdb.Line(l, d, r, d, 2);
                        mdb.Line(r, d, r, u, 2);
                        mdb.Line(r, u, l, u, 2);
                    }

                {
                    float l = Bcc.x - 0.18f;
                    float r = Bcc.right + 0.18f;
                    float u = Bcc.y - 0.18f;
                    float d = Bcc.bottom + 0.18f;
                    toScreen(ref l, ref u);
                    toScreen(ref r, ref d);
                    mdb.Col = C32.d2c(col);
                    mdb.Line(l, u, l, d, 2);
                    mdb.Line(l, d, r, d, 2);
                    mdb.Line(r, d, r, u, 2);
                    mdb.Line(r, u, l, u, 2);
                }
                float sx = Bcc.sx, sy = Bcc.sy, dx = Bcc.dx, dy = Bcc.dy;
                toScreen(ref sx, ref sy);
                toScreen(ref dx, ref dy);
                mdb.Col = C32.d2c(0xFF007FFFU);
                mdb.Line(sx, sy, dx, dy, 4);
                mdb.Col = C32.d2c(0xFF0000FFU);
                mdb.Box(sx, sy, 8, 8);
                mdb.Box(dx, dy, 8, 8);
            };
            foreach (BCCLine bccLine in mp.BCC.getLineVectorLift() ?? [])
                draw(bccLine, 0xFF7FFF00);
            foreach (BCCLine[] bccLinese in mp.BCC.getLineVector() ?? [])
            foreach (BCCLine bccLine in bccLinese)
                draw(bccLine, 0xFFCFFF00);
            BLIT.RenderToGLImmediate001(mdb, setpass: true);
        }
        // map end

        if (Configs.configDisplayPlayer.Value && Utils.getPR() != null) renderPlayer(Utils.getPR());
        // bounding box start
        if (Configs.configDisplayBoundingBox.Value) {
            MeshDrawer md = new();
            md.activate("bounding_box_refresh", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            for (int mvc = 0; mvc < mp.mover_count; mvc++) {
                M2Mover mover = mp.getMv(mvc);
                if (mover == null) continue;
                // Console.WriteLine("found mover " + mover);
                if (mover.getColliderCreator() == null) continue;
                PolygonCollider2D cld = mover.getColliderCreator().Cld;
                if (cld == null) continue;
                // Console.WriteLine("found cld with count "+cld.pathCount);
                for (int pcnt = 0; pcnt < cld.pathCount; pcnt++) {
                    Vector2[] v = cld.GetPath(pcnt);
                    if (v == null) continue;
                    float baseX = mover.x, baseY = mover.y;
                    toScreen(ref baseX, ref baseY);
                    md.Col = C32.d2c(0xFF66CCFFU);
                    if (mover is PR pr) {
                        if (pr.isNoDamageActive() || !pr.can_applydamage_state())
                            md.Col = C32.d2c(0xFFFFFF00U);
                        if (pr.Skill.ShE.parry_t > 0)
                            md.Col = C32.d2c(0xFF00FF00U);
                    }

                    int n = v.Length;
                    for (int i = 0; i < n; i++) {
                        int j = (i + 1) % n;
                        float d = 64f;
                        md.Line(baseX + v[i].x * d, baseY + v[i].y * d, baseX + v[j].x * d, baseY + v[j].y * d,
                            3);
                    }

                    md.Col = C32.d2c(0xFFEE0000U);
                    md.Box(baseX, baseY, 4, 4);
                }
            }

            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
            BLIT.RenderToGLImmediate001(md, md.draw_triangle_count, setpass: true);
        }

        if (Configs.configDisplayMagicItem.Value) {
            MGContainer mgc = m2d.MGC;
            MagicItem[] mgs = Utils.getField<MagicItem[]>(mgc, "AItems");
            MeshDrawer mdGraphics = new(), mdText = new();
            mdGraphics.activate("magic_dumper_graphics", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            mdText.activate("magic_dumper_text", UnifontRenderer.glyphMaterial, false, C32.d2c(0xFFFFFFFFU));
            for (int i = 0; i < mgs.Length; i++) {
                MagicItem mg = mgs[i];
                if (mg == null || mg.kind == MGKIND.NONE || mg.closed || mg.killed) continue;
                Console.WriteLine($"found magic {mg}");
                float x = mg.sx, y = mg.sy;
                toScreen(ref x, ref y);
                mdGraphics.Col = C32.d2c(0xFFFF00FF);
                mdGraphics.Rect(x, y, 3, 3);
                uint c = 0xFFFFFFFF;
                if (mg.closed && mg.killed) c = 0x3FFFFFFF;
                else if (mg.closed) c = 0x7F7FFF7F;
                else if (mg.killed) c = 0x7FFF7F7F;
                mdText.Col = C32.d2c(c);
                List<string> targetStrings = new();
                targetStrings.Add($"MagicItem #{i} / {mg.kind}");
                targetStrings.Add($"Caster {mg.Caster}");
                targetStrings.Add($"casttime {mg.casttime} / t {mg.t}");
                targetStrings.Add($"phase {mg.phase} / type {mg.type}");
                targetStrings.Add($"(sx, sy) = ({mg.sx}, {mg.sy}) / (dx, dy) = ({mg.dx}, {mg.dy})");
                targetStrings.Add($"sa {mg.sa} / da {mg.da} / sz {mg.sz} / dz {mg.dz}");
                targetStrings.Add($"fnRunMain {mg.fnRunMain.Method.DeclaringType} : {mg.fnRunMain.Method}");
                for (int j = 0; j < targetStrings.Count; j++) {
                    UnifontRenderer.draw(mdText, targetStrings[j], x, y + j * 16, 1, 1, true);
                }
            }

            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
            BLIT.RenderToGLImmediate001(mdGraphics, setpass: true);
            BLIT.RenderToGLImmediate001(mdText, setpass: true);
        }

        if (Configs.configDisplayMist.Value) {
            MeshDrawer mdGraphics = new(), mdText = new();
            mdText.activate("mist_dumper_text", UnifontRenderer.glyphMaterial, false, C32.d2c(0xFFFFFFFFU));
            mdGraphics.activate("mist_dumper_graphics", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            MistManager mistManager = m2d.MIST;
            List<MistManager.MistGenerator> AGen = Utils.getField<List<MistManager.MistGenerator>>(mistManager, "AGen");
            if (AGen != null) {
                Dictionary<Vector2Int, ulong> dict = new();
                PR pr = Utils.getPR();
                float prscreenx = pr?.x ?? 0, prscreeny = pr?.y ?? 0;
                toScreen(ref prscreenx, ref prscreeny);
                for (int ti = 0; ti < AGen.Count; ti++) {
                    MistManager.MistGenerator generator = AGen[ti];
                    MistManager.MistKind k = generator.K;
                    MistDrawer mistDrawer = k.Msd;
                    if (mistDrawer == null) continue;
                    List<MistDrawer.MstPt> lst = mistDrawer.APoint;
                    List<MistDrawer.MstPt> rev = lst;
                    string results = k?.AAtk
                        ?.Select(atkItem => atkItem?.SerDmg?.getRawObject())
                        ?.Where(serDmg => serDmg != null)
                        ?.SelectMany(serDmg => serDmg)
                        ?.Aggregate(
                            new StringBuilder(),
                            (sb, kvp) => sb.Append($"[{kvp.Key}'{kvp.Value}]"),
                            sb => sb.ToString()
                        ) ?? "none";
                    UnifontRenderer.draw(mdText,
                        $"#{ti} bitmask {1UL << ti} type {k.type} {results} amount {generator.amount}",
                        prscreenx - 120, prscreeny + 160 + 16 * ti, 1, 1, true);
                    if (rev != null)
                        foreach (MistDrawer.MstPt mstpt in rev) {
                            if (mstpt.active == false) continue;
                            Vector2Int v2i = new(mstpt.x, mstpt.y);
                            if (!dict.ContainsKey(v2i)) dict.Add(v2i, 0UL);
                            dict[v2i] = dict[v2i] | 1UL << ti;
                        }
                }

                foreach (KeyValuePair<Vector2Int, ulong> kvp in dict) {
                    float clenb = mp.CLENB;
                    mdGraphics.Col = C32.d2c(0x7FFFFFFFU);
                    float screenx = kvp.Key.x, screeny = kvp.Key.y;
                    toScreen(ref screenx, ref screeny);
                    mdGraphics.Line(screenx, screeny, screenx, screeny - clenb, 2);
                    mdGraphics.Line(screenx, screeny, screenx + clenb, screeny, 2);
                    mdGraphics.Line(screenx + clenb, screeny, screenx + clenb, screeny - clenb, 2);
                    mdGraphics.Line(screenx, screeny - clenb, screenx + clenb, screeny - clenb, 2);
                    UnifontRenderer.draw(mdText, $"{kvp.Value}", screenx + clenb / 2, screeny - clenb / 2, 1, 1, true);
                }
            }

            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
            BLIT.RenderToGLImmediate001(mdText, setpass: true);
            BLIT.RenderToGLImmediate001(mdGraphics, setpass: true);
        }
        if (Configs.configDisplayMapGrid.Value)
        {
            int minGridX = 0;
            int maxGridX = mp.clms;
            int minGridY = 0;
            int maxGridY = mp.rows;
            float gridSize = mp.CLENB;

            MeshDrawer gridMeshDrawer = new MeshDrawer(null, 4, 6);
            gridMeshDrawer.activate("map_grid_graphics", MTRX.MtrMeshNormal, false, C32.d2c(uint.MaxValue), null);
            gridMeshDrawer.Col = C32.d2c(2147483647U);

            for (int gridY = minGridY; gridY < maxGridY; gridY++)
            {
                float screenX = (float)minGridX;
                float screenY = (float)gridY;
                Advanced.toScreen(ref screenX, ref screenY);
                gridMeshDrawer.Line(screenX, screenY, screenX + gridSize * maxGridX, screenY, 2);
            }

            for (int gridX = minGridX; gridX < maxGridX; gridX++)
            {
                float screenX = (float)gridX;
                float screenY = (float)minGridY;
                Advanced.toScreen(ref screenX, ref screenY);
                gridMeshDrawer.Line(screenX, screenY, screenX, screenY - gridSize * maxGridY, 2);
            }
            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
            BLIT.RenderToGLImmediate001(gridMeshDrawer, setpass: true);
        }
            MeshDrawer md3 = new();
        md3.activate("test", UnifontRenderer.glyphMaterial, false, C32.d2c(0xFFFFFFFF));
        // md3.Box(0, 0, 56, 56);
        md3.Col = C32.d2c(0xFF000000U);
        md3.setCurrentMatrix(Matrix4x4.Scale(new Vector3(1, -1, 1)));
        UnifontRenderer.draw(md3, "ADVANCED mode 2", -26, -8, 1, -1);
        UnifontRenderer.glyphMaterial.SetPass(0);
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        BLIT.RenderToGLImmediate001(md3);
    }

    [HarmonyPatch(typeof(M2MovRenderContainer), "RenderWholeMover")]
    [HarmonyPostfix]
    public static void renderWholeMover(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        if (!isAdvancedEnabled()) return;
        if (Cam.name != Configs.configAdvancedBoxLayer.Value) return;
        if (Configs.configAdvancedBoxMode.Value == 2)
            renderWholeMoverMode2(ref JCon, ref Cam, ref draw_id, ref ___AADob);
        else
            Legacy.renderWholeMoverMode1(ref JCon, ref Cam, ref draw_id, ref ___AADob);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IN), "run")]
    public static void onINRun(ref bool __runOriginal) {
        if (!isAdvancedEnabled()) return;
        if (!GenshinInCradle.paused) passedFrames++;
        nowTicks = DateTime.Now.Ticks - lastTicks;
        lastTicks += nowTicks;
        if (GenshinInCradle.paused)
            __runOriginal = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Map2d), "run")]
    public static void onMap2dRun() {
        if (!isAdvancedEnabled()) return;
        lastGt = Map2d.TS;
        passedGt += lastGt;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IN), "LoggerStbAdd")]
    public static void onLoggerStbAdd(STB Stb) {
        if (!isAdvancedEnabled()) return;
        double mspt = nowTicks / 1e4;
        Stb.Add(" " + Application.targetFrameRate + " " + passedFrames + " " + mspt + " " + (int)(1000 / mspt) + " " +
                lastGt + " " + passedGt);
        PR pr = (NelM2DBase.Instance as NelM2DBase)?.getPrNoel();
        SeedSet.randCounts.TryGetValue(X.Xors, out long cnt0);
        long cnt1 = SeedSet.randCount;
        Stb.Add($" {cnt0} {cnt1}");
        if (pr != null) Stb.Add($" {pr.x:F6} {pr.y:F6}");
        if (GenshinInCradle.paused)
            Stb.Add(" ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff");
        Utils.setStaticField(typeof(Logger), "view_fps_t", 0f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Map2d), "setTimeScale")]
    public static void onSetTimeScale(ref float v, bool force) {
        if (!isAdvancedEnabled()) return;
        if (GenshinInCradle.paused)
            v = 0;
    }

    public static void renderPlayer(PR pr) {
        if (!isAdvancedEnabled()) return;
        M2PrSkill skill = pr.Skill;
        M2Shield s = skill.ShE.Shield;
        float pow = Utils.getField<float>(s, "pow");
        float appear = s.appearable_time;
        float ratio = pow / appear;
        float bx = pr.x, by = pr.y;
        toScreen(ref bx, ref by);
        // Console.WriteLine($"b {bx} {by}");
        MeshDrawer mdGraphics = new(), mdText = new();
        mdGraphics.activate("render_player_graphics", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFF));
        mdText.activate("render_player_text", UnifontRenderer.glyphMaterial, false, C32.d2c(0xFFFFFFFF));
        drawBox(mdGraphics, bx, by + 80, pow >= 0 ? 1 - ratio : -ratio, C32.d2c(pow >= 0 ? 0xFF66CCFF : 0xFFEE0000));
        MKind MKBurst = Utils.getField<MKind>(skill.getBurstSelector(), "MKBurst");
        float faint = MDAT.calcBurstFaintedRatio(pr, MKBurst, skill.getBurstSelector().execute_count,
            pr.Ser.burstConsumeRatio());
        drawBox(mdGraphics, bx, by + 88, Configs.configBurstFaintReverse.Value ? 1 - faint : faint,
            C32.d2c(faintColor(faint)));
        mdText.Col = new Color32(255, 255, 255, 255);
        List<string> targetStrings = new();
        if (pr.Skill.getCurMagic() is { } curmg)
            targetStrings.Add($"current magic {curmg.kind} {curmg.phase} {curmg.t:F2}");
        else
            targetStrings.Add("current magic not present");
        targetStrings.Add($"faint {faint * 100.0:F2}% {appear - pow:F2}");
        targetStrings.Add($"state {pr.get_current_state()} {pr.get_state_time():F2}");
        targetStrings.Add($"x=({pr.x:F6}, {pr.y:F6}); v=({pr.vx:F6}, {pr.vy:F6})");
        float es = pr.RCenemy_sink.val;
        double num = 6f;
        if (!(pr.Mp.floort < pr.sink_ignore_skill_effect)) {
            double re = pr.getRE(RCP.RPI_EFFECT.SINK_REDUCE);
            if (re < 0) num = Math.Max(1, (1 + re) * num);
            else if (re > 0) num += 90.0 * Math.Pow(Math.Min(1, re), 2);
        }
        drawBox(mdGraphics, bx, by + 116, (float)Math.Abs(es / num), C32.d2c(es >= 0 ? 0xFFCC66FF : 0xFF66FFCC));
        targetStrings.Add($"sink={es:F6} / {num:F6}");
        foreach (IRunAndDestroy runAndDestroy in pr.Mp.ARunningObject) {
            Console.WriteLine($"runAndDestroy {runAndDestroy}");
            if (runAndDestroy is SummonerPlayer pl) {
                targetStrings.Add($"delay {pl.delay} delay_filled {pl.delay_filled}");
                targetStrings.Add($"delay_one {pl.delay_one} delay_one_second {pl.delay_one_second}");
            }
        }
        float currentY = by + 100;
        foreach (string text in targetStrings) {
            UnifontRenderer.draw(mdText, text, bx - 80, currentY, 1, 1, true);
            currentY += 16;
        }

        for (int i = 0; i < 14; i++) {
            bool ok = pr.isNoDamageActive((NDMG)i);
            int x = (i >= 7 ? 1 : -1) * 28;
            int y = (3 - i % 7) * 12 + 18;
            mdGraphics.Col = C32.d2c(ok ? 0xFF00FF00 : 0xFFFF0000);
            mdGraphics.Box(bx + x, by + y, 8, 8);
        }

        mdGraphics.Col = C32.d2c(pr.isNoDamageActive() ? 0xFF00FF00 : 0xFFFF0000);
        mdGraphics.Circle(bx - 28, by - 42, 6);
        mdGraphics.Col = C32.d2c(!pr.can_applydamage_state() ? 0xFF00FF00 : 0xFFFF0000);
        mdGraphics.Circle(bx + 28, by - 42, 6);

        BLIT.RenderToGLImmediate001(mdGraphics, setpass: true);
        BLIT.RenderToGLImmediate001(mdText, setpass: true);
    }

    private static uint faintColor(float f) {
        return f == 0 ? 0xFF00FF00 :
            f <= 0.1 ? 0xFFCCCCFF :
            f <= 0.33 ? 0xFFFFFF00 :
            f <= 0.67 ? 0xFFFF7F00 :
            f < 1 ? 0xFFFF0000 : 0xFFFF7FFF;
    }

    private static void drawBox(MeshDrawer md, float x, float y, float ratio, Color32 color) {
        md.Col = C32.d2c(0xFF000000);
        float len = 86, len2 = 87, t1 = 4, t2 = 8;
        md.Line(x - len2, y, x + len2, y, t2);
        md.Col = color;
        md.Line(x - len, y, x - len + 2 * len * ratio, y, t1);
    }
}