using System;
using System.Collections.Generic;
using m2d;
using nel;
using UnityEngine;
using XX;

namespace GenshinInCradle;

public class Legacy {
    public static void renderWholeMoverMode1(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        List<M2RenderTicket> list = ___AADob[draw_id];
        foreach (M2Ray ray in Advanced.rayList) {
            // Console.WriteLine("ray " + ray + " caster " + ray.Caster);
            MeshDrawer md2 = new();
            md2.activate("attack_box_refreshed", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            md2.Col = C32.d2c(0xFFFFFFFFU);
            md2.getMaterial().SetPass(0);
            if (ray.Caster == null || ray.Caster.transform == null) {
                Console.WriteLine("returned because of null matrix");
                continue;
            }

            Matrix4x4 mat = ray.Caster.transform.localToWorldMatrix;
            float num, num2, num3, num4;
            Map2d m2d = ray.Mp;
            float clenb = m2d.CLENB;
            Advanced.cast2(ray, out num, out num2, out num3, out num4);
            float x = ray.getMapPos().x, y = ray.getMapPos().y;
            x -= ray.Caster.x;
            y -= ray.Caster.y;
            if (num2 == 0f) {
                float sx = x * clenb, sy = -y * clenb, r = num3 * 64;
                float tx = sx + ray.Dir.x * ray.len * 64;
                float ty = sy + ray.Dir.y * ray.len * 64;
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
                float sx = x * clenb, sy = -y * clenb;
                float tx = sx + ray.Dir.x * ray.len * 64;
                float ty = sy + ray.Dir.y * ray.len * 64;
                float dis = (float)Math.Sqrt((sx - tx) * (sx - tx) + (sy - ty) * (sy - ty));
                num2 *= ray.radius * 64;
                num3 *= ray.radius * 64;

                md2.Col = C32.d2c(0xBFFF0000);
                md2.Box(sx, sy, num2 * 2, num3 * 2);
                md2.Col = C32.d2c(0xFF00FF00);
                md2.Line(sx, sy, tx, ty, 3);
            }

            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * mat);
            BLIT.RenderToGLImmediate001(md2);
        }

        Advanced.rayList.Clear();
        //ray end
        // bounding box start
        foreach (M2RenderTicket ticket in list) {
            M2Mover mover = ticket.AssignMover;
            Matrix4x4 mat = Matrix4x4.identity;
            if (mover == null) continue;
            if (mover.transform != null) mat = mover.transform.localToWorldMatrix;
            if (mover.getColliderCreator() == null) continue;
            if (mover.getColliderCreator().Cld == null) continue;
            Vector2[] v = mover.getColliderCreator().Cld.GetPath(0);
            if (v == null) continue;
            int n = v.Length;
            MeshDrawer md = new();
            md.activate("bounding_box_refresh", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
            md.getMaterial().SetPass(0);
            if (mover is PR pr) {
                for (int i = 0; i < 14; i++) {
                    bool ok = pr.isNoDamageActive((NDMG)i);
                    int x = (i >= 7 ? 1 : -1) * 28;
                    int y = (3 - i % 7) * 12 + 18;
                    md.Col = C32.d2c(ok ? 0xFF00FF00 : 0xFFFF0000);
                    md.Box(x, y, 8, 8);
                }

                md.Col = C32.d2c(pr.isNoDamageActive() ? 0xFF00FF00 : 0xFFFF0000);
                md.Circle(-28, -42, 6);
                md.Col = C32.d2c(!pr.can_applydamage_state() ? 0xFF00FF00 : 0xFFFF0000);
                md.Circle(28, -42, 6);

                md.Col = C32.d2c(0xFF66CCFFU);
                if (pr.isNoDamageActive() || !pr.can_applydamage_state())
                    md.Col = C32.d2c(0xFFFFFF00U);

                if (pr.Skill.ShE.parry_t > 0)
                    md.Col = C32.d2c(0xFF00FF00U);
                for (int i = 0; i < n; i++) {
                    int j = (i + 1) % n;
                    float d = 64f;
                    md.Line(v[i].x * d, v[i].y * d, v[j].x * d, v[j].y * d, 3);
                }
            }
            else {
                md.Col = C32.d2c(0xFF66CCFFU);
                for (int i = 0; i < n; i++) {
                    int j = (i + 1) % n;
                    float d = 64f;
                    md.Line(v[i].x * d, v[i].y * d, v[j].x * d, v[j].y * d, 3);
                }
            }

            md.Col = C32.d2c(0xFFEE0000U);
            md.Box(0, 0, 4, 4);
            GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * mat);
            BLIT.RenderToGLImmediate001(md, md.draw_triangle_count);
        }

        MeshDrawer md3 = new();
        md3.activate("test", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFF));
        md3.Box(0, 0, 56, 56);
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        BLIT.RenderToGLImmediate001(md3);
    }
}