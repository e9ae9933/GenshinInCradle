using System;
using System.Collections.Generic;
using System.IO;
using CriWare;
using HarmonyLib;
using m2d;
using nel;
using XX;
using YamlDotNet.Serialization;

namespace GenshinInCradle;

public class SoundReplay {
    private static long replayStartingTicks;
    private static readonly Dictionary<int, M2SoundPlayerItem> replayItems = new();
    private static Queue<Rec> replayQueue = new();


    private static double replayTime;
    private static bool replaying;
    private static readonly List<Tuple<Rec.What, M2SoundPlayerItem>> theList = new();
    private static readonly Dictionary<M2SoundPlayerItem, int> dict = new();
    private static int top;

    public static void tick() {
        if (Configs.configResetPassedFramesShortcut.Value.IsDown()) {
            Advanced.passedFrames = 0;
            replayQueue.Clear();
            UILog.Instance.AddAlert("reset passed frames");
        }

        if (Configs.configReplaySoundShortcut.Value.IsDown()) {
            replaying = !replaying;
            replayTime = 0;
            replayStartingTicks = DateTime.Now.Ticks;
            Advanced.passedFrames = 0;
        }

        if (Configs.configReadSoundShortcut.Value.IsDown()) {
            IDeserializer des = new DeserializerBuilder().Build();
            List<Rec> lst = des.Deserialize<List<Rec>>(File.ReadAllText("sound.yml"));
            replayQueue = new Queue<Rec>(lst);
            UILog.Instance.AddAlert("read from yml");
        }

        if (Configs.configSaveSoundShortcut.Value.IsDown()) {
            ISerializer ser = new SerializerBuilder().Build();
            File.WriteAllText("sound.yml", ser.Serialize(replayQueue));
            UILog.Instance.AddAlert("wrote to yml");
        }

        if (!replaying) {
            ISerializer ser = new SerializerBuilder().Build();
            double now = Advanced.passedFrames;
            foreach (Tuple<Rec.What, M2SoundPlayerItem> tp in theList) {
                Rec rec = getRec(tp.Item2, tp.Item1, now);
                //Console.WriteLine("cue "+tp.Item1+" "+rec.current_cue);
                replayQueue.Enqueue(rec);
            }

            theList.Clear();
        }
        else {
            theList.Clear();
            replayTime = (DateTime.Now.Ticks - replayStartingTicks) / 10000000.0 * 60;
            Console.WriteLine(replayTime);
            Advanced.passedFrames = (long)replayTime;
            M2SoundPlayer snd = M2DBase.Instance.Snd;
            float pre_x = snd.pre_x, pre_y = snd.pre_y;
            while (replayQueue.Count > 0 && replayQueue.Peek().time <= replayTime) {
                Rec rec = replayQueue.Dequeue();
                if (rec.what == Rec.What.PREPARE) {
                    int index = rec.index;
                    M2SoundPlayerItem ans;
                    if (rec.mapx == 0 && rec.mapy == 0) {
                        ans = snd.play(rec.current_cue);
                        Console.WriteLine($"play {rec.current_cue}");
                    }
                    else {
                        ans = snd.playAt(rec.current_cue, rec.key, rec.mapx - rec.pre_x + pre_x,
                            rec.mapy - rec.pre_y + pre_y);
                        Console.WriteLine($"play {rec.current_cue} {rec.mapx - rec.pre_x} {rec.mapy - rec.pre_y}");
                    }

                    if (ans != null) {
                        ans.duration = rec.duration;
                        replayItems[index] = ans;
                    }
                }
                else if (rec.what == Rec.What.STOP) {
                    int index = rec.index;
                    if (replayItems.ContainsKey(index)) {
                        Console.WriteLine($"stop {rec.current_cue}");
                        replayItems[index].Stop();
                    }
                }
            }

            if (replayQueue.Count == 0) {
                UILog.Instance.AddAlert("Stopped replay.");
                replaying = !replaying;
            }
        }
    }

    private static Rec getRec(M2SoundPlayerItem player, Rec.What what, double now) {
        if (!dict.ContainsKey(player)) dict[player] = top++;
        int index = dict[player];
        float pre_x = player.Con.pre_x, pre_y = player.Con.pre_y;
        Rec rt = new() {
            what = what,
            time = now,
            key = player.key,
            current_cue = player.current_cue,
            duration = player.duration,
            is_loop = player.is_loop,
            voice_priority_manual = player.voice_priority_manual,
            resume_flag = player.resume_flag,
            stype = player.stype,
            mapx = player.mapx,
            mapy = player.mapy,
            volume_pan = player.volume_pan,
            volume_maunal = player.volume_maunal,
            pre_x = pre_x,
            pre_y = pre_y,
            index = index
        };
        return rt;
    }

    [HarmonyPatch(typeof(M2SoundPlayerItem), "prepare", typeof(string), typeof(string), typeof(bool))]
    [HarmonyPostfix]
    public static void onPrepare(M2SoundPlayerItem __instance, string header, string cue_key, bool force) {
        // Console.WriteLine("preparing "+header+" "+cue_key);
        theList.Add(Tuple.Create(Rec.What.PREPARE, __instance));
    }

    [HarmonyPatch(typeof(M2SoundPlayerItem), "prepare", typeof(CriAtomExAcb), typeof(string), typeof(CriAtomEx.CueInfo),
        typeof(float), typeof(float), typeof(bool))]
    [HarmonyPostfix]
    public static void onPrepare2(M2SoundPlayerItem __instance, string name, float mapx, float mapy, bool _long_flag) {
        // Console.WriteLine("preparing2 "+name);
        theList.Add(Tuple.Create(Rec.What.PREPARE, __instance));
    }

    [HarmonyPatch(typeof(M2SoundPlayerItem), "Stop")]
    [HarmonyPrefix]
    public static void onStop(M2SoundPlayerItem __instance) {
        theList.Add(Tuple.Create(Rec.What.STOP, __instance));
    }

    public class Rec {
        public enum What {
            PREPARE,
            STOP
        }

        public string current_cue;
        public long duration;
        public int index;
        public byte is_loop;
        public string key;
        public float mapx;
        public float mapy;
        public float pre_x;
        public float pre_y;
        public bool resume_flag;
        public SndPlayer.SNDTYPE stype;
        public double time;
        public byte voice_priority_manual;
        public float volume_maunal;
        public float volume_pan;
        public What what;
    }
}