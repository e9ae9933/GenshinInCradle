using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace GenshinInCradle;

public static class Screenshot {
    public static void StartSequence(MonoBehaviour pluginInstance) {
        Directory.CreateDirectory("screenshots");
        pluginInstance.StartCoroutine(CaptureSequenceLoop());
    }

    private static IEnumerator CaptureSequenceLoop() {
        while (true) {
            yield return new WaitForEndOfFrame();

            if (Configs.configSequenceScreenshot.Value) {
                Stopwatch sw = Stopwatch.StartNew();
                int w = Screen.width;
                int h = Screen.height;
                Texture2D t2d = new(w, h, TextureFormat.RGB24, false);
                t2d.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                t2d.Apply();
                byte[] jpgData = t2d.EncodeToJPG();
                Object.Destroy(t2d);
                string path = Path.Combine("screenshots", $"{Advanced.passedFrames}.jpg");
                File.WriteAllBytes(path, jpgData);
                // Console.WriteLine($"tick {Advanced.passedFrames}, stopwatch {sw.ElapsedTicks/10000.0:F5} ms");
            }
        }
    }
}