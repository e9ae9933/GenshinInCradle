using System;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using m2d;
using nel;

namespace GenshinInCradle;

public class Utils {
    public static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                       BindingFlags.Static;

    public static FieldInfo getFieldInfo(Type type, string name) {
        return type.GetField(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    }

    public static T getField<T>(object o, string name) {
        return (T)o.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(o);
    }

    public static void setStaticField<T>(Type type, string name, T value) {
        getFieldInfo(type, name).SetValue(null, value);
    }

    public static T getStaticField<T>(Type type, string name) {
        return (T)getFieldInfo(type, name).GetValue(null);
    }

    public static void setField<T>(object o, string name, T value) {
        getFieldInfo(o.GetType(), name).SetValue(o, value);
    }

    [CanBeNull]
    public static PR getPR() {
        return (M2DBase.Instance as NelM2DBase)?.getPrNoel();
    }

    public static string ShowInputDialog(string text, string caption) {
        using (Form prompt = new()) {
            prompt.Width = 400;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.TopMost = true; // 确保在游戏窗口最上层

            Label textLabel = new() { Left = 20, Top = 20, Text = text, Width = 340 };
            TextBox textBox = new() { Left = 20, Top = 50, Width = 340 };
            Button confirmation = new() { Text = "确定", Left = 260, Width = 100, Top = 80, DialogResult = DialogResult.OK };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}