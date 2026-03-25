using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Better;
using m2d;
using nel;
using nel.smnp;
using XX;

namespace GenshinInCradle;

public class SummonerPlayerAdvanced : SummonerPlayer {
    private static long time = 0;

    public SummonerPlayerAdvanced(EnemySummoner _Summoner, EfParticleFuncCalc _FuncBase, CsvReaderA _CR,
        out bool bgm_replaced, bool auto_prepare_content = true)
        : base(_Summoner, _FuncBase, _CR, out bgm_replaced, auto_prepare_content) {
    }

    public static void log(double id, [CallerLineNumber] int line = 0) {
        long end = System.Diagnostics.Stopwatch.GetTimestamp();
        Console.WriteLine($"#{id} - Line {line} - XORSP #{SeedSet.randCount} - {end - time}");
        time = System.Diagnostics.Stopwatch.GetTimestamp();
    }
    public void prepareEnemyConentFromScript2(
        List<SmnEnemyKind> AKindL,
        List<SmnPoint> ASmnPosL,
        ref SPCntInfo CI) {
        log(2.41);
        CsvReaderA CR = this.CR;
        if (!this.open_from_event)
            CR.VarCon.removeTemp();
        CR.VarCon.define("_here", this.key);
        CR.VarCon.define("_map", this.Lp.Mp.key);
        ENATTR nattr = ENATTR.NORMAL;
        NightController nightCon = this.Lp.nM2D.NightCon;
        QuestTracker.SummonerEntry qentry = this.QEntry;
        log(2.42);
        if (qentry.valid && this.QEntry.fix_enemykind > 0) {
            EnemySummonerManager manager = this.Summoner.GetManager();
            if (manager != null) {
                bool flag = (this.QEntry.fix_enemykind & int.MinValue) != 0;
                string str = NDAT.ToStr((ENEMYID)this.QEntry.fix_enemykind);
                int num = X.Mx(1, X.IntR((float)(3.5999999046325684 - X.Abs(manager.getEnemyPower(str).x) - (flag ? 3.0 : 0.0))));
                this.OMaxEnemyAppear[(flag ? "OD_" : "") + str] = new EnAppearMax(X.Mx(1, X.IntR(num) + CI.appear_add));
            }
        }
        log(2.43);
        while (CR.read()) {
            bool fix_flag = false;
            switch (CR.cmd) {
                case "%ADD_COUNT":
                    string str1 = CR.slice_join();
                    if (TX.isStart(str1, "*=")) {
                        CI.count_add = X.IntR(CI.count_add * X.Nm(TX.slice(str1, 2), 1f, true));
                        continue;
                    }
                    if (TX.isStart(str1, "!")) {
                        str1 = TX.slice(str1, 1);
                        CI.count_add = 0;
                    }
                    CI.count_add += (int)this.CalcScript(str1);
                    continue;
                case "%BGM_BLOCK":
                    this.bgm_block_to = CR._1;
                    continue;
                case "%BGM_OVERRIDE_KEY":
                    this.bgm_block_ovr = CR._2;
                    continue;
                case "%BGM_REPLACE_WHEN_CLOSE":
                    this.bgm_replace_when_close = TX.eval(CR._1) != 0.0;
                    continue;
                case "%CANNOT_THUNDER_TO":
                    if (this.Acannot_thunder_to_enemyid == null) this.Acannot_thunder_to_enemyid = new List<string>(CR.clength - 1);
                    int clength = CR.clength;
                    for (int _i = 1; _i < clength; ++_i) this.Acannot_thunder_to_enemyid.Add(CR.getIndex(_i));
                    continue;
                case "%DELAY_FILLED":
                    this.delay_filled = this.Calc(CR, 1);
                    continue;
                case "%DELAY_ONE":
                    this.delay_one = this.Calc(CR, 1);
                    this.delay_one_second = this.delay_one * nightCon.summoner_delayonesecond_ratio(this);
                    continue;
                case "%DELAY_ONE_FIRST":
                    this.delay_one = this.Calc(CR, 1);
                    continue;
                case "%EN":
                case "%EN_OD":
                    SmnEnemyKind enemyEntry = this.createEnemyEntry(CR, CI.splitter_id, ref CI.count_add, out fix_flag, nattr);
                    if (enemyEntry != null) {
                        AKindL.Add(enemyEntry);
                        if (!fix_flag)
                            CI.countadd_priority = X.Mn(CI.countadd_priority, enemyEntry.def_count);
                        if (!enemyEntry.pre_overdrive && enemyEntry.EnemyDesc.overdriveable) {
                            CI.odable_enemy_exist = true;
                        }
                    }
                    continue;
                case "%ENATTR":
                    readEnAttr(ref nattr, CR);
                    continue;
                case "%EN_HR":
                    ++CI.splitter_id;
                    this.Asplitter_title.Add(new SplitterTerm(CR, (int)this.delay_one_second));
                    continue;
                case "%EN_LOAD":
                    continue;
                case "%FATAL":
                    if (CR.clength < 3 || TX.eval(CR.slice_join(2)) != 0.0) {
                        this.Summoner.fatal_key = CR._1;
                        X.dl("フェイタルキー " + this.Summoner.fatal_key);
                    }
                    continue;
                case "%GOLEMTOY_DIVIDE":
                    golemtoy_divide_count = (int)this.Calc(CR, 1, golemtoy_divide_count.ToString());
                    continue;
                case "%INITIAL_DELAY":
                    this.delay = this.Calc(CR, 1);
                    continue;
                case "%MAX_APPEAR":
                case "%MAX_APPEAR_AA":
                    bool flag = CR.cmd == "%MAX_APPEAR_AA" && this.Lp.auto_activate_enemy;
                    if (CR.clength == 3) {
                        qentry = this.QEntry;
                        if (!qentry.kindMatch(CR._1)) {
                            float num = this.CalcF(CR, ref fix_flag, 2, "99");
                            if (!(fix_flag | flag))
                                num = X.Mx(X.Mn(2f, num), num + CI.appear_add);
                            string str2 = CR._1;
                            bool _is_od = false;
                            if (TX.isStart(str2, "OD_")) {
                                str2 = TX.slice(str2, 3);
                                _is_od = true;
                            }
                            this.OMaxEnemyAppear[str2] = new EnAppearMax(X.Mx(1, X.IntR(num)), _is_od: _is_od);
                        }
                        continue;
                    }
                    float num1 = this.CalcF(CR, ref fix_flag, 1, "99");
                    if (!(fix_flag | flag))
                        num1 = X.Mx(1f, num1 + CI.appear_add);
                    this.max_enemy_appear_whole = X.Mx(2, X.IntR(num1));
                    continue;
                case "%MAX_CNT":
                    if (CR.clength == 3) {
                        qentry = this.QEntry;
                        if (!qentry.kindMatch(CR._1)) {
                            if (CI.OEnemyCountMax == null)
                                CI.OEnemyCountMax = new BDic<string, int[]>(1);
                            float num2 = this.CalcF(CR, ref fix_flag, 2, "99");
                            if (!fix_flag)
                                num2 += nightCon.summoner_enemy_countmax_addition(this);
                            CI.OEnemyCountMax[CR._1] = new int[2] {
                                X.IntR(num2),
                                0
                            };
                        }
                        continue;
                    }
                    float num3 = this.CalcF(CR, ref fix_flag, 1, "99");
                    if (!fix_flag)
                        num3 += nightCon.summoner_enemy_countmax_addition(this);
                    CI.max_enemy_count = X.IntR(num3);
                    continue;
                case "%NEXT_SCRIPT":
                    this.next_script_key = CR._1;
                    continue;
                case "%POS":
                case "%POS@":
                case "%POS_LN":
                case "%POS_LN@":
                    M2LabelPoint m2LabelPoint = this.Lp;
                    int _i1 = 0;
                    string str3 = "";
                    if (CR.cmd == "%POS@" || CR.cmd == "%POS_LN@") {
                        ++_i1;
                        str3 = CR.getIndex(_i1);
                    }
                    if (CR.cmd == "%POS_LN@" || CR.cmd == "%POS_LN") {
                        ++_i1;
                        m2LabelPoint = this.Mp.getPoint(CR.getIndex(_i1)) ?? m2LabelPoint;
                    }
                    SmnPoint smnPoint = new(this, CR.Nm(1 + _i1), CR.Nm(2 + _i1), this.Calc(CR, 3 + _i1, "1"),
                        XORSP(), CR.slice(4 + _i1)) {
                        name = str3,
                        Lp = m2LabelPoint
                    };
                    ASmnPosL.Add(smnPoint);
                    continue;
                case "%PUPPETREVENGE":
                    if (this.Lp.is_sudden_puppetrevenge) {
                        CsvReaderA crPuppetRevenge = this.Summoner.createCRPuppetRevenge(false);
                        this.Summoner.fineReelData(CR._1, ref this.AReel, ref this.AReelSecretSrc, ref this.replace_reel_secret_to_lower);
                        CR = crPuppetRevenge;
                        CR.seek_set();
                        CI.force_can_get_whole_reels = this.drop_all_reels_after = true;
                    }
                    continue;
                case "%REPLACE_BGM":
                    if (CR.clength < 3 || TX.eval(CR.slice_join(2)) != 0.0) {
                        this.bgm_replace = CR._1;
                    }
                    continue;
                case "%SPEVENT":
                    if (this.Eventor == null) this.Eventor = new SummonerPlayerEventor(this);
                    this.Eventor.read(CR, nattr, this.Asplitter_title[CI.splitter_id].title);
                    continue;
                default:
                    if (!this.readOther(CR) && CR.cmd.IndexOf("#") != 0) {
                        CR.tError("不明なコマンド: " + CR.cmd);
                    }
                    continue;
            }
        }
        log(2.44);
        if (ASmnPosL.Count != 0)
            return;
        ASmnPosL.Add(new SmnPoint(this, 0.0f, 0.0f, 1f, XORSP()));
        log(2.45);
    }
    public void prepareEnemyConentFromScript3(
        List<SmnEnemyKind> AKindL,
        List<SmnPoint> ASmnPosL,
        ref SPCntInfo CI) {
        log(2.41);
        CsvReaderA CR = this.CR;
        if (!this.open_from_event)
            CR.VarCon.removeTemp();
        CR.VarCon.define("_here", this.key);
        CR.VarCon.define("_map", this.Lp.Mp.key);
        ENATTR nattr = ENATTR.NORMAL;
        NightController nightCon = this.Lp.nM2D.NightCon;
        QuestTracker.SummonerEntry qentry = this.QEntry;
        log(2.42);
        if (qentry.valid && this.QEntry.fix_enemykind > 0) {
            EnemySummonerManager manager = this.Summoner.GetManager();
            if (manager != null) {
                bool flag = (this.QEntry.fix_enemykind & int.MinValue) != 0;
                string str = NDAT.ToStr((ENEMYID)this.QEntry.fix_enemykind);
                int num = X.Mx(1, X.IntR((float)(3.5999999046325684 - X.Abs(manager.getEnemyPower(str).x) - (flag ? 3.0 : 0.0))));
                this.OMaxEnemyAppear[(flag ? "OD_" : "") + str] = new EnAppearMax(X.Mx(1, X.IntR(num) + CI.appear_add));
            }
        }
        log(2.43);
        while (CR.read()) {
            bool fix_flag = false;
            switch (CR.cmd) {
                case "%ADD_COUNT":
                    string str1 = CR.slice_join();
                    if (TX.isStart(str1, "*=")) {
                        CI.count_add = X.IntR(CI.count_add * X.Nm(TX.slice(str1, 2), 1f, true));
                        continue;
                    }
                    if (TX.isStart(str1, "!")) {
                        str1 = TX.slice(str1, 1);
                        CI.count_add = 0;
                    }
                    CI.count_add += (int)this.CalcScript(str1);
                    continue;
                case "%BGM_BLOCK":
                    this.bgm_block_to = CR._1;
                    continue;
                case "%BGM_OVERRIDE_KEY":
                    this.bgm_block_ovr = CR._2;
                    continue;
                case "%BGM_REPLACE_WHEN_CLOSE":
                    this.bgm_replace_when_close = TX.eval(CR._1) != 0.0;
                    continue;
                case "%CANNOT_THUNDER_TO":
                    if (this.Acannot_thunder_to_enemyid == null) this.Acannot_thunder_to_enemyid = new List<string>(CR.clength - 1);
                    int clength = CR.clength;
                    for (int _i = 1; _i < clength; ++_i) this.Acannot_thunder_to_enemyid.Add(CR.getIndex(_i));
                    continue;
                case "%DELAY_FILLED":
                    this.delay_filled = this.Calc(CR, 1);
                    continue;
                case "%DELAY_ONE":
                    this.delay_one = this.Calc(CR, 1);
                    this.delay_one_second = this.delay_one * nightCon.summoner_delayonesecond_ratio(this);
                    continue;
                case "%DELAY_ONE_FIRST":
                    this.delay_one = this.Calc(CR, 1);
                    continue;
                case "%EN":
                case "%EN_OD":
                    SmnEnemyKind enemyEntry = this.createEnemyEntry(CR, CI.splitter_id, ref CI.count_add, out fix_flag, nattr);
                    if (enemyEntry != null) {
                        AKindL.Add(enemyEntry);
                        if (!fix_flag)
                            CI.countadd_priority = X.Mn(CI.countadd_priority, enemyEntry.def_count);
                        if (!enemyEntry.pre_overdrive && enemyEntry.EnemyDesc.overdriveable) {
                            CI.odable_enemy_exist = true;
                        }
                    }
                    continue;
                case "%ENATTR":
                    readEnAttr(ref nattr, CR);
                    continue;
                case "%EN_HR":
                    ++CI.splitter_id;
                    this.Asplitter_title.Add(new SplitterTerm(CR, (int)this.delay_one_second));
                    continue;
                case "%EN_LOAD":
                    continue;
                case "%FATAL":
                    if (CR.clength < 3 || TX.eval(CR.slice_join(2)) != 0.0) {
                        this.Summoner.fatal_key = CR._1;
                        X.dl("フェイタルキー " + this.Summoner.fatal_key);
                    }
                    continue;
                case "%GOLEMTOY_DIVIDE":
                    golemtoy_divide_count = (int)this.Calc(CR, 1, golemtoy_divide_count.ToString());
                    continue;
                case "%INITIAL_DELAY":
                    this.delay = this.Calc(CR, 1);
                    continue;
                case "%MAX_APPEAR":
                case "%MAX_APPEAR_AA":
                    bool flag = CR.cmd == "%MAX_APPEAR_AA" && this.Lp.auto_activate_enemy;
                    if (CR.clength == 3) {
                        qentry = this.QEntry;
                        if (!qentry.kindMatch(CR._1)) {
                            float num = this.CalcF(CR, ref fix_flag, 2, "99");
                            if (!(fix_flag | flag))
                                num = X.Mx(X.Mn(2f, num), num + CI.appear_add);
                            string str2 = CR._1;
                            bool _is_od = false;
                            if (TX.isStart(str2, "OD_")) {
                                str2 = TX.slice(str2, 3);
                                _is_od = true;
                            }
                            this.OMaxEnemyAppear[str2] = new EnAppearMax(X.Mx(1, X.IntR(num)), _is_od: _is_od);
                        }
                        continue;
                    }
                    float num1 = this.CalcF(CR, ref fix_flag, 1, "99");
                    if (!(fix_flag | flag))
                        num1 = X.Mx(1f, num1 + CI.appear_add);
                    this.max_enemy_appear_whole = X.Mx(2, X.IntR(num1));
                    continue;
                case "%MAX_CNT":
                    if (CR.clength == 3) {
                        qentry = this.QEntry;
                        if (!qentry.kindMatch(CR._1)) {
                            if (CI.OEnemyCountMax == null)
                                CI.OEnemyCountMax = new BDic<string, int[]>(1);
                            float num2 = this.CalcF(CR, ref fix_flag, 2, "99");
                            if (!fix_flag)
                                num2 += nightCon.summoner_enemy_countmax_addition(this);
                            CI.OEnemyCountMax[CR._1] = new int[2] {
                                X.IntR(num2),
                                0
                            };
                        }
                        continue;
                    }
                    float num3 = this.CalcF(CR, ref fix_flag, 1, "99");
                    if (!fix_flag)
                        num3 += nightCon.summoner_enemy_countmax_addition(this);
                    CI.max_enemy_count = X.IntR(num3);
                    continue;
                case "%NEXT_SCRIPT":
                    this.next_script_key = CR._1;
                    continue;
                case "%POS":
                case "%POS@":
                case "%POS_LN":
                case "%POS_LN@":
                    M2LabelPoint m2LabelPoint = this.Lp;
                    int _i1 = 0;
                    string str3 = "";
                    if (CR.cmd == "%POS@" || CR.cmd == "%POS_LN@") {
                        ++_i1;
                        str3 = CR.getIndex(_i1);
                    }
                    if (CR.cmd == "%POS_LN@" || CR.cmd == "%POS_LN") {
                        ++_i1;
                        m2LabelPoint = this.Mp.getPoint(CR.getIndex(_i1)) ?? m2LabelPoint;
                    }
                    SmnPoint smnPoint = new(this, CR.Nm(1 + _i1), CR.Nm(2 + _i1), this.Calc(CR, 3 + _i1, "1"),
                        XORSP(), CR.slice(4 + _i1)) {
                        name = str3,
                        Lp = m2LabelPoint
                    };
                    ASmnPosL.Add(smnPoint);
                    continue;
                case "%PUPPETREVENGE":
                    if (this.Lp.is_sudden_puppetrevenge) {
                        CsvReaderA crPuppetRevenge = this.Summoner.createCRPuppetRevenge(false);
                        this.Summoner.fineReelData(CR._1, ref this.AReel, ref this.AReelSecretSrc, ref this.replace_reel_secret_to_lower);
                        CR = crPuppetRevenge;
                        CR.seek_set();
                        CI.force_can_get_whole_reels = this.drop_all_reels_after = true;
                    }
                    continue;
                case "%REPLACE_BGM":
                    if (CR.clength < 3 || TX.eval(CR.slice_join(2)) != 0.0) {
                        this.bgm_replace = CR._1;
                    }
                    continue;
                case "%SPEVENT":
                    if (this.Eventor == null) this.Eventor = new SummonerPlayerEventor(this);
                    this.Eventor.read(CR, nattr, this.Asplitter_title[CI.splitter_id].title);
                    continue;
                default:
                    if (!this.readOther(CR) && CR.cmd.IndexOf("#") != 0) {
                        CR.tError("不明なコマンド: " + CR.cmd);
                    }
                    continue;
            }
        }
        log(2.44);
        if (ASmnPosL.Count != 0)
            return;
        ASmnPosL.Add(new SmnPoint(this, 0.0f, 0.0f, 1f, XORSP()));
        log(2.45);
    }
    public override void prepareEnemyConentInner(ref bool bgm_replaced) {
        log(1);
        NelM2DBase m2D = this.Lp.Mp.M2D as NelM2DBase;
        this.Asplitter_title.Clear();
        this.Asplitter_title.Add(new SplitterTerm());
        NightController nightCon = this.Lp.nM2D.NightCon;
        int thunder_overdrive;
        m2D.NightCon.SummonerInited(this.Summoner, this.Lp.Mp, out thunder_overdrive);
        int _overdrive_capacity_weather = X.Mx(0, thunder_overdrive + this.Summoner.get_thunder_odable_def());
        int _count_add = nightCon.summoner_enemy_count_addition(this.Summoner);
        int _appear_add = nightCon.summoner_max_addition(this);
        log(2);
        SPCntInfo CI = new(_appear_add, _count_add, _overdrive_capacity_weather) {
            auto_activate_immediate_summon = this.Lp.auto_activate_enemy
        };
        log(2.1);
        int nattr_addable_count = nightCon.summoner_attachable_nattr_max(this);
        this.max_enemy_appear_whole = X.Mx(2, 3 + _appear_add);
        this.thunder_odable_appear = this.Summoner.thunder_odable_appear + nightCon.summoner_max_thunder_odable_appear(this);
        this.bgm_block_ovr = null;
        this.Summoner.GetManager();
        log(2.4);
        List<SmnEnemyKind> targetEnemyKinds = new(4);
        List<SmnPoint> ASmnPosL = new();
        this.prepareEnemyConentFromScript2(targetEnemyKinds, ASmnPosL, ref CI);
        log(2.7);
        List<SmnEnemyKind> smnEnemyKindList2 = new(targetEnemyKinds.Count);
        smnEnemyKindList2.Clear();
        smnEnemyKindList2.AddRange(targetEnemyKinds);
        int num1 = 0;
        float num2 = 0.0f;
        targetEnemyKinds.Clear();
        log(3);
        List<int[]> numArrayList = new(2);
        for (int index1 = smnEnemyKindList2.Count - 1; index1 >= 0; --index1) {
            SmnEnemyKind Src = smnEnemyKindList2[index1];
            numArrayList.Clear();
            int num3 = 999;
            if (CI.OEnemyCountMax != null)
                foreach (KeyValuePair<string, int[]> keyValuePair in CI.OEnemyCountMax)
                    if (Src.isSame(keyValuePair.Key)) {
                        numArrayList.Add(keyValuePair.Value);
                        num3 = X.Mn(keyValuePair.Value[0] - keyValuePair.Value[1], num3);
                    }

            int num4 = X.Mn(num3, Src.def_count);
            if (num3 <= Src.def_count)
                Src.count_add_weight = 0.0f;
            for (int index2 = numArrayList.Count - 1; index2 >= 0; --index2)
                numArrayList[index2][1] += num4;
            if (!Src.count_fix)
                num2 += Src.count_add_weight;
            if (Src.pre_overdrive)
                num1 += num4;
            while (--num4 >= 0)
                targetEnemyKinds.Add(new SmnEnemyKind(Src));
        }

        log(4);
        if (num2 > 0.0 && smnEnemyKindList2.Count > 0) {
            shuffle(smnEnemyKindList2);
            int num5 = 0;
            while (_count_add > 0 && num2 > 0.0 && ++num5 < 300) {
                SmnEnemyKind Src = null;
                if (CI.countadd_priority <= 0) {
                    int v1 = 1;
                    for (int index = smnEnemyKindList2.Count - 1; index >= 0; --index) {
                        SmnEnemyKind smnEnemyKind = smnEnemyKindList2[index];
                        if (!smnEnemyKind.count_fix) {
                            v1 = X.Mn(v1, smnEnemyKind.def_count);
                            if (smnEnemyKind.def_count == CI.countadd_priority) {
                                Src = smnEnemyKind;
                                Src.def_count = 1;
                                break;
                            }
                        }
                    }

                    CI.countadd_priority = v1;
                }

                if (Src == null && CI.countadd_priority > 0) {
                    float num6 = XORSP() * num2;
                    for (int index = smnEnemyKindList2.Count - 1; index >= 0; --index) {
                        SmnEnemyKind smnEnemyKind = smnEnemyKindList2[index];
                        if (smnEnemyKind.count_add_weight > 0.0) {
                            num6 -= smnEnemyKind.count_add_weight;
                            if (num6 <= 0.00019999999494757503) {
                                Src = smnEnemyKind;
                                break;
                            }
                        }
                    }
                }

                if (Src != null) {
                    --_count_add;
                    SmnEnemyKind smnEnemyKind = new(Src);
                    smnEnemyKind.pre_overdrive = false;
                    targetEnemyKinds.Add(smnEnemyKind);
                    if (CI.OEnemyCountMax != null) {
                        bool flag = false;
                        foreach (KeyValuePair<string, int[]> keyValuePair in CI.OEnemyCountMax)
                            if (smnEnemyKind.isSame(keyValuePair.Key) &&
                                ++keyValuePair.Value[1] >= keyValuePair.Value[0])
                                flag = true;
                        if (flag) {
                            num2 -= Src.count_add_weight;
                            Src.count_add_weight = 0.0f;
                        }
                    }
                }
            }
        }

        smnEnemyKindList2.Clear();
        log(5);
        for (int index3 = 0; index3 < 2; ++index3) {
            if (CI.odable_enemy_exist && CI.overdrive_capacity_weather > 0) {
                int count = targetEnemyKinds.Count;
                int[] A = X.makeCountUpArray(count);
                shuffle(A, count);
                for (int index4 = 0; index4 < count; ++index4) {
                    int index5 = A[index4];
                    SmnEnemyKind smnEnemyKind = targetEnemyKinds[index5];
                    NDAT.EnemyDescryption enemyDesc = smnEnemyKind.EnemyDesc;
                    if (!smnEnemyKind.pre_overdrive && !smnEnemyKind.thunder_overdrive && enemyDesc.overdriveable) {
                        smnEnemyKind.thunder_overdrive = true;
                        smnEnemyKindList2.Add(smnEnemyKind);
                        ++num1;
                        if (--CI.overdrive_capacity_weather <= 0)
                            break;
                    }
                }
            }

            if (index3 == 0) {
                if (targetEnemyKinds.Count > CI.max_enemy_count) {
                    for (int index6 = smnEnemyKindList2.Count - 1; index6 >= 0; --index6)
                        targetEnemyKinds.Remove(smnEnemyKindList2[index6]);
                    shuffle(targetEnemyKinds);
                    int count = targetEnemyKinds.Count - (CI.max_enemy_count + smnEnemyKindList2.Count);
                    if (count > 0)
                        targetEnemyKinds.RemoveRange(CI.max_enemy_count + smnEnemyKindList2.Count, count);
                    for (int index7 = smnEnemyKindList2.Count - 1; index7 >= 0; --index7)
                        targetEnemyKinds.Add(smnEnemyKindList2[index7]);
                }
                else {
                    shuffle(targetEnemyKinds);
                }

                if (this.QEntry.valid && this.QEntry.fix_enemykind >= 0) {
                    float mp_min;
                    float mp_max;
                    int num7 = nightCon.summoner_enemy_for_qentry_special_count_min(this, out mp_min, out mp_max);
                    ENEMYID fixEnemykind = (ENEMYID)this.QEntry.fix_enemykind;
                    int v1 = 0;
                    for (int index8 = targetEnemyKinds.Count - 1; index8 >= 0; --index8) {
                        SmnEnemyKind K = targetEnemyKinds[index8];
                        if (!this.is_follower(K))
                            v1 = X.Mx(v1, K.splitter_id);
                        if (K.isSame(fixEnemykind))
                            --num7;
                    }

                    if (num7 > 0)
                        while (--num7 >= 0) {
                            SmnEnemyKind smnEnemyKind = new(NDAT.ToStr(fixEnemykind & ~ENEMYID._OVERDRIVE_FLAG), 1,
                                CI.splitter_id, mp_min, mp_max, "", _mp_add_weight: 3f) {
                                pre_overdrive = (fixEnemykind & ENEMYID._OVERDRIVE_FLAG) > 0
                            };
                            targetEnemyKinds.Add(smnEnemyKind);
                        }
                    else
                        break;
                }
                else {
                    break;
                }
            }
            else {
                break;
            }
        }

        log(6);
        if (CI.splitter_id >= 1)
            targetEnemyKinds.Sort(this.fnSortKind);
        smnEnemyKindList2.Clear();
        this.prepareEnemyEnAttr(targetEnemyKinds, smnEnemyKindList2, nattr_addable_count);
        log(7);
        log(8);
        log(9);
        this.ASmnPos = ASmnPosL.ToArray();
        this.AKindRest = targetEnemyKinds;
        this.AKindRest.Sort(this.fnSortKind);
        log(10);
    }
}