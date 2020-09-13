using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;

namespace RyonaRPG_ItemDataConverter
{
    class CommonEventGenerator
    {
        /// <summary> ラベルの配置間隔</summary>
        const int LabelSpan = 50;
        /// <summary> 開始コモン番号</summary>
        public const int CommonNumberStart = 2001;
        /// <summary> アイテム番号毎コモン区切り</summary>
        const int CommonSplit = 100;
        /// <summary> アイテム番号毎コモン区切り　アイテム情報の取得用　負荷軽減のためより細かく分割しています</summary>
        const int CommonSplitItemQuantity = 50;
        /// <summary>コモン「データ取得」 (ピクチャメニュー表示中は使えないため使用断念)</summary>
        const int CommonGetPlayerStatus = 56;
        /// <summary>コモン「HPMP計算」</summary>
        const int CommonHPMPCalc = 32;
        /// <summary>コモン「ステ状態取得」</summary>
        const int CommonGetPlayerDebuff = 57;

        /// <summary> アイテム番号MAX値（最大5000）</summary>
        const int ItemNumMax = 5000;

        /// <summary>主人公番号格納変数番号</summary>
        const int ValuePlayerNum = 6;
        /// <summary> アイテム番号格納変数番号</summary>
        const int ValueItemNum = 191;
        /// <summary> 「所持数」格納先変数番号</summary>
        const int ValueNumberPoss = 1665;
        /// <summary> 「値段」格納先変数番号</summary>
        const int ValuePrice = 1666;
        /// <summary> 検索filter格納変数番号</summary>
        const int ValueSearchFilter = 1661;
        /// <summary> 検索index格納変数番号</summary>
        const int ValueSearchIndex = 1662;
        /// <summary> ﾌｨﾙﾀ毎index上限値格納変数番号</summary>
        const int ValueFilterIndexMax = 1667;
        /// <summary>「主人公」変数 </summary>
        const int ValuePlayerNumber = 6;
        /// <summary>「HP/Max[%]」変数 </summary>
        const int ValueHPPercent = 21;
        /// <summary>「MP/Max[%]」変数 </summary>
        const int ValueMPPercent = 22;
        /// <summary>「HP/Max[%]」変数 </summary>
        const int ValueHP = 23;
        /// <summary>「MP/Max[%]」変数 </summary>
        const int ValueMP = 24;
        /// <summary>「HP最大値」変数 </summary>
        const int ValueHPMAX = 35;
        /// <summary>「MP最大値」変数 </summary>
        const int ValueMPMAX = 36;
        /// <summary> 「汎用1」変数</summary>
        const int ValueGeneral1 = 1;
        /// <summary> 「汎用2」変数</summary>
        const int ValueGeneral2 = 2;

        /// <summary> 種別_サブ種別変数</summary>
        const int ValueItemType = 1656;
        /// <summary> 装備品性能変数</summary>
        const int ValueItemATK = 1657;
        const int ValueItemDEF = 1658;
        const int ValueItemMAT = 1659;
        const int ValueItemMDF = 1660;

        /// <summary> メニュー強制終了スイッチ</summary>
        const int SwitchMenuInterrupt = 1565;

        /// <summary> 「ｱｲﾃﾑ-装備不可」スイッチ</summary>
        const int SwitchEquipPermission = 1573;

        /// <summary> 状態異常</summary>
        const int SwitchDebuffPoison = 101;
        const int SwitchDebuffDarkness = 102;
        const int SwitchDebuffSilent = 103;
        const int SwitchDebuffParalysis = 104;
        const int SwitchDebuffFall = 105;
        const int SwitchDebuffBleeding = 106;
        const int SwitchDebuffCapture = 107;
        const int SwitchDebuffMucus = 108;
        const int SwitchDebuffFear = 109;
        const int SwitchDebuffFrozen = 110;
        const int SwitchDebuffStun = 111;
        const int SwitchDebuffXXX = 112;


        /// <summary> アイテム使用時の音</summary>
        const string SoundItemUse = "神聖5";

        public static void CodeToClipboard(List<ItemData> itemDatas)
        {
            // コモン番号：コモンイベントのbyteリストで管理します
            Dictionary<int, List<byte>> dicCommonEvents = new Dictionary<int, List<byte>>();

            // 名前表示用コモンの作成
            CreateDrawNameCommons(ref dicCommonEvents, itemDatas);

            // 説明表示用コモンの作成
            CreateDrawDescCommons(ref dicCommonEvents, itemDatas);

            // 所持数取得用コモンの作成
            CreateGetItemQuantityCommons(ref dicCommonEvents, itemDatas);

            // index検索用コモンの作成
            CreateSearchFilterCommons(ref dicCommonEvents, itemDatas);

            // アイテム使用時の効果発生コモンの作成
            CreateUseItemCommons(ref dicCommonEvents, itemDatas);

            // アイテム名取得コモンの作成
            CreateGetItemNameCommon(ref dicCommonEvents, itemDatas);

            // アイテム情報取得コモンの作成
            CreateGetItemInfoCommons(ref dicCommonEvents, itemDatas);

            // 空いているIDの領域を空白コモンで埋めます
            int keyMin = 5000;
            int keyMax = 0;
            foreach (int key in dicCommonEvents.Keys)
            {
                if (key < keyMin) keyMin = key;
                if (key > keyMax) keyMax = key;
            }

            //List<byte> plainCommon = CreatePlainCommon();
            for (int i=keyMin; i<=keyMax; i++)
            {
                if (dicCommonEvents.ContainsKey(i) == false)
                {
                    dicCommonEvents.Add(i, CreatePlainCommon(i));
                }

            }

            // Listの中身を全部結合します
            int bytes = 0; // バイト数
            int num = dicCommonEvents.Count; // イベント数
            List<byte> code = new List<byte>();
            for (int i = keyMin; i <= keyMax; i++)
            {
                code.AddRange(dicCommonEvents[i]);
                bytes += dicCommonEvents[i].Count;
            }

            // バイト数
            List<byte> temp = new List<byte>();
            int cnt = 1;
            while(true)
            {
                int t = bytes % (int)Math.Pow(256, cnt);
                bytes -= t;
                if(cnt != 1)
                {
                    t /= (int)Math.Pow(256, cnt-1);
                }
                temp.Add(Convert.ToByte(t));
                if (bytes == 0) break;
                cnt++;
            }
            while (temp.Count < 4)
            {
                temp.Add(0);
            }
            code.InsertRange(0,temp);

            // 要素数
            temp = new List<byte>();
            cnt = 1;
            while (true)
            {
                int t = num % (int)Math.Pow(256, cnt);
                num -= t;
                if (cnt != 1)
                {
                    t /= (int)Math.Pow(256, cnt - 1);
                }
                temp.Add(Convert.ToByte(t));
                if (num == 0) break;
                cnt++;
            }
            while (temp.Count < 4)
            {
                temp.Add(0);
            }
            code.InsertRange(4, temp);

            SetClipboardData(code.ToArray());
        }

        /// <summary>
        /// 名前画像表示用コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateDrawNameCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 0;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 50;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            // 生成対象のピクチャー番号 増やすと重くなります
            List<int> pictureNum = new List<int>() { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62 };

            string NL = Environment.NewLine;
            // 2重リスト
            List<List<byte>> commonEvents = new List<List<byte>>();

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            // 開始位置に呼び出し用のコモンを配置 
            int cnt = 0;
            List<byte> events = new List<byte>();
            for (int i = 0; i < ItemNumMax; i += CommonSplit)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + CommonSplit));
                events.AddRange(new List<byte> { 0x02, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側？
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                cnt++;
            }
            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ名画像表示", events);
            dicCommonEvents.Add(triggerCommonNumber, d);


            // CommonSplit区切りでファイル生成
            // 実際の画像表示
            int commonNum = processCommonNumber;
            for (int cs = 0; cs < ItemNumMax; cs += CommonSplit)
            {
                events = new List<byte>();
                int cs_max = cs + CommonSplit;
                if (cs < itemDatas.Count)
                {
                    if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                    // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                    int label = 1;
                    for (int i = cs; i < cs_max; i += LabelSpan)
                    {
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + LabelSpan));
                        events.AddRange(new List<byte> { 0x02, 0x00 });
                        // 指定ラベルへ飛ぶ
                        events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(label));
                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        label++;
                    }

                    // 画像表示部分
                    label = 1;
                    for (int i = cs; i < cs_max; i++)
                    {
                        ItemData data = itemDatas[i];

                        // 50毎にLabelを設定
                        if (i % LabelSpan == 0)
                        {
                            events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                            events.AddRange(Common.IntToBerList(label));
                            label++;
                        }

                        // アイテム番号
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00, 0x00 });

                        // ピクチャ番号
                        for (int j = 0; j < pictureNum.Count; j++)
                        {
                            int pictNum = pictureNum[j];

                            // 条件分岐
                            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                            events.AddRange(Common.IntToBerList(333));
                            events.AddRange(new List<byte> { 0x00 });
                            events.AddRange(Common.IntToBerList(pictNum));
                            events.AddRange(new List<byte> { 0x00, 0x00 });

                            // ピクチャ表示
                            events.AddRange(new List<byte> { 0xD6, 0x66, 0x02 });
                            string fname = string.Format("Menu\\Item\\Name\\{0}", data.NameNumber);
                            events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(fname))); // 文字数
                            events.AddRange(sjisEnc.GetBytes(fname));
                            events.AddRange(new List<byte> { 0x0E });// この値何に使ってるのか不明なので要注意
                            events.AddRange(Common.IntToBerList(pictNum));
                            events.AddRange(new List<byte> { 0x01, 0x81, 0x26, 0x81, 0x27, 0x00, 0x64, 0x00, 0x01, 0x64, 0x64, 0x64, 0x64, 0x00, 0xBA, 0xE6, 0xDB, 0x7C });

                            // 条件分岐の終わり側
                            events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                        }
                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
                    }
                }

                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                cs_max = cs + CommonSplit;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ｱｲﾃﾑ名画像" + (cs + 1).ToString() + "~" + cs_max.ToString(), events);
                dicCommonEvents.Add(commonNum, d2);

                commonNum++;
            }
        }

        /// <summary>
        /// 説明画像表示用コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateDrawDescCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 1;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 100;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            // 生成対象のピクチャー番号 増やすと重くなります
            List<int> pictureNum = new List<int>() { 75 };

            string NL = Environment.NewLine;
            // 2重リスト
            List<List<byte>> commonEvents = new List<List<byte>>();

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            // 開始位置に呼び出し用のコモンを配置 
            int cnt = 0;
            List<byte> events = new List<byte>();
            for (int i = 0; i < ItemNumMax; i += CommonSplit)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + CommonSplit));
                events.AddRange(new List<byte> { 0x02, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側？
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                cnt++;
            }
            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ説明画像表示", events);
            dicCommonEvents.Add(triggerCommonNumber, d);


            // CommonSplit区切りでファイル生成
            // 実際の画像表示
            int commonNum = processCommonNumber;
            for (int cs = 0; cs < ItemNumMax; cs += CommonSplit)
            {
                events = new List<byte>();
                int cs_max = cs + CommonSplit;
                if (cs < itemDatas.Count)
                {
                    if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                    // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                    int label = 1;
                    for (int i = cs; i < cs_max; i += LabelSpan)
                    {
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + LabelSpan));
                        events.AddRange(new List<byte> { 0x02, 0x00 });
                        // 指定ラベルへ飛ぶ
                        events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(label));
                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        label++;
                    }

                    // 画像表示部分
                    label = 1;
                    for (int i = cs; i < cs_max; i++)
                    {
                        ItemData data = itemDatas[i];

                        // 50毎にLabelを設定
                        if (i % LabelSpan == 0)
                        {
                            events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                            events.AddRange(Common.IntToBerList(label));
                            label++;
                        }

                        // アイテム番号
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00, 0x00 });

                        // ピクチャ番号
                        for (int j = 0; j < pictureNum.Count; j++)
                        {
                            int pictNum = pictureNum[j];

                            // 条件分岐
                            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                            events.AddRange(Common.IntToBerList(333));
                            events.AddRange(new List<byte> { 0x00 });
                            events.AddRange(Common.IntToBerList(pictNum));
                            events.AddRange(new List<byte> { 0x00, 0x00 });

                            // ピクチャ表示
                            events.AddRange(new List<byte> { 0xD6, 0x66, 0x02 });
                            string fname = string.Format("Menu\\Item\\Desc\\{0}", data.NameNumber);
                            events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(fname))); // 文字数
                            events.AddRange(sjisEnc.GetBytes(fname));
                            events.AddRange(new List<byte> { 0x0E });// この値何に使ってるのか不明なので要注意
                            events.AddRange(Common.IntToBerList(pictNum));
                            events.AddRange(new List<byte> { 0x01, 0x81, 0x26, 0x81, 0x27, 0x00, 0x64, 0x00, 0x01, 0x64, 0x64, 0x64, 0x64, 0x00, 0xBA, 0xE6, 0xDB, 0x7C });

                            // 条件分岐の終わり側
                            events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                        }
                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
                    }
                }

                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                cs_max = cs + CommonSplit;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ｱｲﾃﾑ説明画像" + (cs + 1).ToString() + "~" + cs_max.ToString(), events);
                dicCommonEvents.Add(commonNum, d2);

                commonNum++;
            }
        }

        /// <summary>
        /// アイテム所持数の取得用コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateGetItemQuantityCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 2;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 150;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            // 開始位置に呼び出し用のコモンを配置 
            int cnt = 0;
            List<byte> events = new List<byte>();
            for (int i = 0; i < ItemNumMax; i += CommonSplitItemQuantity)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + CommonSplitItemQuantity));
                events.AddRange(new List<byte> { 0x02, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側？
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                cnt++;
            }
            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ所持数取得", events);
            dicCommonEvents.Add(triggerCommonNumber, d);


            // CommonSplit区切りでファイル生成
            // 実際の取得処理
            int commonNum = processCommonNumber;
            for (int cs = 0; cs < ItemNumMax; cs += CommonSplitItemQuantity)
            {
                events = new List<byte>();
                int cs_max = cs + CommonSplitItemQuantity;
                if (cs < itemDatas.Count)
                {
                    if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                    // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                    // アイテム情報取得処理をアイテム50毎に変更したため、ラベル関連処理を削除
                    /*
                    int label = 1;
                    for (int i = cs; i < cs_max; i += LabelSpan)
                    {
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + LabelSpan));
                        events.AddRange(new List<byte> { 0x02, 0x00 });
                        // 指定ラベルへ飛ぶ
                        events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(label));
                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        label++;
                    }
                    */

                    // 取得処理部分
                    //label = 1;
                    for (int i = cs; i < cs_max; i++)
                    {
                        ItemData data = itemDatas[i];

                        /*
                        // 50毎にLabelを設定
                        if (i % LabelSpan == 0)
                        {
                            events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                            events.AddRange(Common.IntToBerList(label));
                            label++;
                        }
                        */

                        // アイテム番号
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00, 0x00 });

                        // 所持数取得
                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueNumberPoss));
                        events.AddRange(Common.IntToBerList(ValueNumberPoss));
                        events.AddRange(new List<byte> { 0x00, 0x04 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00 });

                        // 装備数加算
                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueNumberPoss));
                        events.AddRange(Common.IntToBerList(ValueNumberPoss));
                        events.AddRange(new List<byte> { 0x01, 0x04 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x01 });

                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
                    }
                }

                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                cs_max = cs + CommonSplitItemQuantity;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ｱｲﾃﾑ所持数取得" + (cs + 1).ToString() + "~" + cs_max.ToString(), events);
                dicCommonEvents.Add(commonNum, d2);

                commonNum++;
            }
        }

        /// <summary>
        /// インデックス検索用フィルタコモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateSearchFilterCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 3;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 250;

            string NL = Environment.NewLine;

            // 登場しうるフィルタ候補を全てピックアップします
            List<int> filters = new List<int>();
            for (int i = 0; i < itemDatas.Count; i++)
            {
                int val = itemDatas[i].SearchFilter + 10000;
                if (!filters.Contains(val)) filters.Add(val);
            }
            // ワイルドカード検索の追加 0099みたいな奴
            for (int i = 0; i < 10; i++)
            {
                filters.Add(i * 100 + 99 + 10000);
            }
            // 全検索
            filters.Add(19999);

            // ☆トリガー部 フィルタ候補別に分岐する処理を作成します
            List<byte> events = new List<byte>();

            // 取得できなかった時の処理(0を返す)
            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x00, 0x00, 0x07, 0x00 });
            events.AddRange(Common.IntToBerList(ValueItemNum));
            events.AddRange(Common.IntToBerList(ValueItemNum));
            events.AddRange(new List<byte> { 0x00, 0x00 });
            events.AddRange(Common.IntToBerList(0));
            events.AddRange(new List<byte> { 0x00 });

            // フィルタ値が10000未満の時はそのままの値を返す (個別アイテム番号フィルタ)
            // 条件分岐
            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
            events.AddRange(Common.IntToBerList(ValueSearchFilter));
            events.AddRange(new List<byte> { 0x00 });
            events.AddRange(Common.IntToBerList(10000));
            events.AddRange(new List<byte> { 0x04, 0x00 });

            // 条件分岐2 indexが0であるか
            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
            events.AddRange(Common.IntToBerList(ValueSearchIndex));
            events.AddRange(new List<byte> { 0x00 });
            events.AddRange(Common.IntToBerList(0));
            events.AddRange(new List<byte> { 0x00, 0x00 });

            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
            events.AddRange(Common.IntToBerList(ValueItemNum));
            events.AddRange(Common.IntToBerList(ValueItemNum));
            events.AddRange(new List<byte> { 0x00, 0x01 });
            events.AddRange(Common.IntToBerList(ValueSearchFilter));
            events.AddRange(new List<byte> { 0x00 });

            // exit
            events.AddRange(new List<byte> { 0xE0, 0x16, 0x02, 0x00, 0x00 }); // 3byte目がインデントなので注意

            // 条件分岐の終わり側
            events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });

            // 条件分岐の終わり側
            events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

            for (int i = 0; i < filters.Count; i++)
            {
                // フィルタ毎の検索処理を作成
                int filter = filters[i];

                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueSearchFilter));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(filter));
                events.AddRange(new List<byte> { 0x00, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + i));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
            }

            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑﾌｨﾙﾀ検索", events);
            dicCommonEvents.Add(triggerCommonNumber, d);

            // ☆フィルタ毎のindex上限値を取得するコモンの作成
            events = new List<byte>();
            // 取得できなかった時の処理(0を返す)
            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x00, 0x00, 0x07, 0x00 });
            events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
            events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
            events.AddRange(new List<byte> { 0x00, 0x00 });
            events.AddRange(Common.IntToBerList(0));
            events.AddRange(new List<byte> { 0x00 });

            // フィルタ値が10000未満の時は1を返す (個別アイテム番号フィルタ)
            // 条件分岐
            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
            events.AddRange(Common.IntToBerList(ValueSearchFilter));
            events.AddRange(new List<byte> { 0x00 });
            events.AddRange(Common.IntToBerList(10000));
            events.AddRange(new List<byte> { 0x04, 0x00 });

            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
            events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
            events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
            events.AddRange(new List<byte> { 0x00, 0x00 });
            events.AddRange(Common.IntToBerList(1));
            events.AddRange(new List<byte> { 0x00 });

            // exit
            events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

            // 条件分岐の終わり側
            events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

            for (int i = 0; i < filters.Count; i++)
            {
                int cnt = 0;
                int filter = filters[i] - 10000;
                int FilterSubType = filter % 100; // サブ種別のフィルタ値
                int FilterType = (filter - FilterSubType) / 100; // 種別のフィルタ値

                // ワイルドカード検索
                if (FilterSubType == 99)
                {
                    if (FilterType == 99)
                    {
                        cnt = itemDatas.Count;
                    }
                    else
                    {
                        // サブ種別ワイルドカード検索
                        for (int j = 0; j < itemDatas.Count; j++)
                        {
                            ItemData data = itemDatas[j];
                            if (data.Type == FilterType && data.Name != "")
                            {
                                cnt++;
                            }
                        }
                    }
                }
                else
                {
                    // フィルタ検索
                    for (int j = 0; j < itemDatas.Count; j++)
                    {
                        ItemData data = itemDatas[j];
                        if (data.SearchFilter == filter && data.Name != "")
                        {
                            cnt++;
                        }
                    }
                }

                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueSearchFilter));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(filter+10000));
                events.AddRange(new List<byte> { 0x00, 0x00 });

                // cnt値のセット
                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
                events.AddRange(Common.IntToBerList(ValueFilterIndexMax));
                events.AddRange(new List<byte> { 0x00, 0x00 });
                events.AddRange(Common.IntToBerList(cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
            }

            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            d = MergeCommonHeader((triggerCommonNumber + 1).ToString() + "ﾌｨﾙﾀ上限値取得", events);
            dicCommonEvents.Add(triggerCommonNumber + 1, d);


            // ☆フィルタ毎のアイテム番号取得処理の作成
            for (int i = 0; i < filters.Count; i++)
            {
                events = new List<byte>();

                int filter = filters[i] - 10000;
                int FilterSubType = filter % 100; // サブ種別のフィルタ値
                int FilterType = (filter-FilterSubType) / 100; // 種別のフィルタ値

                // ワイルドカード検索
                int index = 0;
                if (FilterSubType == 99)
                {
                    if (FilterType == 99)
                    {
                        // 全検索 検索index+1を返す
                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x00, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueSearchIndex));
                        events.AddRange(new List<byte> { 0x00 });

                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x00, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x01, 0x00, 0x01, 0x00 });

                        // 全アイテム数の限界を超えた時は検索失敗
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(itemDatas.Count));
                        events.AddRange(new List<byte> { 0x03, 0x00 });

                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00, 0x00 });
                        events.AddRange(Common.IntToBerList(0));
                        events.AddRange(new List<byte> { 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x00, 0x00, 0x00 });
                    }
                    else
                    {
                        int label = 1;
                        // サブ種別ワイルドカード検索
                        // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                        label = 1;
                        index = 0;
                        for (int j = 0; j < itemDatas.Count; j++)
                        {
                            ItemData data = itemDatas[j];
                            if (data.Type == FilterType && data.Name != "")
                            {
                                if (index % LabelSpan == 0)
                                {
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                                    events.AddRange(Common.IntToBerList(ValueSearchIndex));
                                    events.AddRange(new List<byte> { 0x00 });
                                    events.AddRange(Common.IntToBerList(label * LabelSpan));
                                    events.AddRange(new List<byte> { 0x04, 0x00 });
                                    // 指定ラベルへ飛ぶ
                                    events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                                    events.AddRange(Common.IntToBerList(label));
                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                                    label++;
                                }
                                index++;
                            }
                        }

                        label = 1;
                        index = 0;
                        for (int j = 0; j < itemDatas.Count; j++)
                        {
                            ItemData data = itemDatas[j];
                            if (data.Type == FilterType && data.Name != "")
                            {
                                // 50毎にLabelを設定
                                if (index % LabelSpan == 0)
                                {
                                    events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                                    events.AddRange(Common.IntToBerList(label));
                                    label++;
                                }

                                // アイテム番号
                                // 条件分岐
                                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueSearchIndex));
                                events.AddRange(new List<byte> { 0x00 });
                                events.AddRange(Common.IntToBerList(index));
                                events.AddRange(new List<byte> { 0x00, 0x00 });

                                // 番号取得
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueItemNum));
                                events.AddRange(Common.IntToBerList(ValueItemNum));
                                events.AddRange(new List<byte> { 0x00, 0x00 });
                                events.AddRange(Common.IntToBerList(data.Number));
                                events.AddRange(new List<byte> { 0x00 });

                                // exit
                                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                                // 条件分岐の終わり側
                                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                                index++;
                            }
                        }
                    }
                }
                else
                {
                    // フィルタ検索
                    for (int j = 0; j < itemDatas.Count; j++)
                    {
                        ItemData data = itemDatas[j];
                        if (data.SearchFilter == filter && data.Name != "")
                        {
                            // アイテム番号
                            // 条件分岐
                            events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                            events.AddRange(Common.IntToBerList(ValueSearchIndex));
                            events.AddRange(new List<byte> { 0x00 });
                            events.AddRange(Common.IntToBerList(index));
                            events.AddRange(new List<byte> { 0x00, 0x00 });

                            // 番号取得
                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueItemNum));
                            events.AddRange(Common.IntToBerList(ValueItemNum));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(data.Number));
                            events.AddRange(new List<byte> { 0x00 });

                            // exit
                            events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                            // 条件分岐の終わり側
                            events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                            index++;
                        }
                    }
                }
                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                int commonNum = processCommonNumber + i;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ﾌｨﾙﾀ検索" + (filter+10000).ToString("00000"), events);
                dicCommonEvents.Add(commonNum, d2);
            }
        }

        /// <summary>
        /// アイテム使用時の効果発生コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateUseItemCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 5;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 350;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            // 開始位置に呼び出し用のコモンを配置 
            int cnt = 0;
            List<byte> events = new List<byte>();
            for (int i = 0; i < ItemNumMax; i += CommonSplit)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + CommonSplit));
                events.AddRange(new List<byte> { 0x02, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側？
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                cnt++;
            }
            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ使用", events);
            dicCommonEvents.Add(triggerCommonNumber, d);


            // CommonSplit区切りでファイル生成
            // 実際の取得処理
            int commonNum = processCommonNumber;
            for (int cs = 0; cs < ItemNumMax; cs += CommonSplit)
            {
                events = new List<byte>();
                int cs_max = cs + CommonSplit;
                if (cs < itemDatas.Count)
                {
                    if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                    // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                    // アイテム情報取得処理をアイテム50毎に変更したため、ラベル関連処理を削除
                    int label = 1;
                    for (int i = cs; i < cs_max; i += LabelSpan)
                    {
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + LabelSpan));
                        events.AddRange(new List<byte> { 0x02, 0x00 });
                        // 指定ラベルへ飛ぶ
                        events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(label));
                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        label++;
                    }

                    // 使用処理部分
                    label = 1;
                    for (int i = cs; i < cs_max; i++)
                    {
                        ItemData data = itemDatas[i];

                        // 50毎にLabelを設定
                        if (i % LabelSpan == 0)
                        {
                            events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                            events.AddRange(Common.IntToBerList(label));
                            label++;
                        }

                        // アイテム番号
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00, 0x00 });

                        #region 薬
                        // 薬
                        if (data.Type == (int)ItemData.TypeEnum.Healing)
                        {
                            // 使用可能か
                            bool available = false;

                            // コモン「HPMP計算」
                            events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 });
                            events.AddRange(Common.IntToBerList(CommonHPMPCalc));
                            events.AddRange(new List<byte> { 0x00 });

                            // コモン「(主)ステ状態取得」
                            events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 });
                            events.AddRange(Common.IntToBerList(CommonGetPlayerDebuff));
                            events.AddRange(new List<byte> { 0x00 });

                            // 汎用01をフラグにします 回復した時は1を入れます
                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueGeneral1));
                            events.AddRange(Common.IntToBerList(ValueGeneral1));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(0));
                            events.AddRange(new List<byte> { 0x00 });
                            
                            // HP回復
                            if (data.HealingHPper != 0 || data.HealingHPpt != 0)
                            {
                                available = true;
                                // 条件分岐 (HPがMAXか)
                                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueHPPercent));
                                events.AddRange(new List<byte> { 0x00 });
                                events.AddRange(Common.IntToBerList(100));
                                events.AddRange(new List<byte> { 0x04, 0x00 });

                                // 汎用01を1に
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral1));
                                events.AddRange(Common.IntToBerList(ValueGeneral1));
                                events.AddRange(new List<byte> { 0x00, 0x00 });
                                events.AddRange(Common.IntToBerList(1));
                                events.AddRange(new List<byte> { 0x00 });

                                // 回復量計算（汎用2に値をセット (MAX_HP*回復量(%))/100 + 回復量(pt)）
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueHPMAX));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x03, 0x00 });
                                events.AddRange(Common.IntToBerList(data.HealingHPper));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x04, 0x00 });
                                events.AddRange(Common.IntToBerList(100));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x01, 0x00 });
                                events.AddRange(Common.IntToBerList(data.HealingHPpt));
                                events.AddRange(new List<byte> { 0x00 });

                                // HP回復
                                events.AddRange(new List<byte> { 0xD1, 0x5C, 0x02, 0x00, 0x06, 0x02 });
                                events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x00 });

                                // 変数「HP」に加算　次回計算時HPが100％を超えるが妥協（コモン56データ取得がピクチャメニュー表示中使えないため）
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueHP));
                                events.AddRange(Common.IntToBerList(ValueHP));
                                events.AddRange(new List<byte> { 0x01, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x00 });

                                // 条件分岐の終わり側
                                events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                            }

                            // MP回復
                            if (data.HealingMPper != 0 || data.HealingMPpt != 0)
                            {
                                available = true;
                                // 条件分岐 (MPがMAXか)
                                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueMPPercent));
                                events.AddRange(new List<byte> { 0x00 });
                                events.AddRange(Common.IntToBerList(100));
                                events.AddRange(new List<byte> { 0x04, 0x00 });

                                // 汎用01を1に
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral1));
                                events.AddRange(Common.IntToBerList(ValueGeneral1));
                                events.AddRange(new List<byte> { 0x00, 0x00 });
                                events.AddRange(Common.IntToBerList(1));
                                events.AddRange(new List<byte> { 0x00 });

                                // 回復量計算（汎用2に値をセット (MAX_MP*回復量(%))/100 + 回復量(pt)）
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueMPMAX));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x03, 0x00 });
                                events.AddRange(Common.IntToBerList(data.HealingMPper));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x04, 0x00 });
                                events.AddRange(Common.IntToBerList(100));
                                events.AddRange(new List<byte> { 0x00 });

                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x01, 0x00 });
                                events.AddRange(Common.IntToBerList(data.HealingMPpt));
                                events.AddRange(new List<byte> { 0x00 });

                                // MP回復
                                events.AddRange(new List<byte> { 0xD1, 0x66, 0x02, 0x00, 0x05, 0x02 });
                                events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));

                                // 変数「MP」に加算　次回計算時HPが100％を超えるが妥協（コモン56データ取得がピクチャメニュー表示中使えないため）
                                events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                events.AddRange(Common.IntToBerList(ValueMP));
                                events.AddRange(Common.IntToBerList(ValueMP));
                                events.AddRange(new List<byte> { 0x01, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueGeneral2));
                                events.AddRange(new List<byte> { 0x00 });

                                // 条件分岐の終わり側
                                events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                            }

                            for(int j=0; j<data.Debuff.Count; j++)
                            {
                                int numDebuff = data.Debuff[j]; // DBの状態の番号と同じ　毒は2番
                                // 毒
                                if(numDebuff == 2)
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffPoison));
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x02 }); // 毒

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 暗闇
                                if (numDebuff == 3)
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffDarkness));
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x03 });

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 沈黙
                                if (numDebuff == 4)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffSilent));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x04 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 麻痺
                                if (numDebuff == 5)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffParalysis));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x05 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 転倒
                                if (numDebuff == 6)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffFall));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x06 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 出血
                                if (numDebuff == 7)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffBleeding));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x07 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 捕獲
                                if (numDebuff == 8)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffCapture));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x08 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 粘液
                                if (numDebuff == 9)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffMucus));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x09 });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 恐怖
                                if (numDebuff == 10)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffFear));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x0A });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 凍結
                                if (numDebuff == 11)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffFrozen));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x0B });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // 気絶
                                if (numDebuff == 12)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffStun));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x0C });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }
                                // ？？？(多分直っても無意味だけど)
                                if (numDebuff == 13)//
                                {
                                    available = true;
                                    // 条件分岐
                                    events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x00 });
                                    events.AddRange(Common.IntToBerList(SwitchDebuffXXX));//
                                    events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                                    // 汎用01を1に
                                    events.AddRange(new List<byte> { 0xCF, 0x6C, 0x02, 0x00, 0x07, 0x00 });
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(Common.IntToBerList(ValueGeneral1));
                                    events.AddRange(new List<byte> { 0x00, 0x00 });
                                    events.AddRange(Common.IntToBerList(1));
                                    events.AddRange(new List<byte> { 0x00 });

                                    // 治療
                                    events.AddRange(new List<byte> { 0xD1, 0x70, 0x02, 0x00, 0x04, 0x02 });
                                    events.AddRange(Common.IntToBerList(ValuePlayerNumber));
                                    events.AddRange(new List<byte> { 0x01 });
                                    events.AddRange(new List<byte> { 0x0D });//

                                    // 条件分岐の終わり側
                                    events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                                }

                                if(numDebuff >= 14)
                                {
                                    // 未設定 現在でばっぐつーる以外で解除不可
                                }
                                if(numDebuff == 1)
                                {
                                    // 未設定 現在でばっぐつーる以外で解除不可
                                }
                            }
                            
                            // 使った時
                            if (available == true)
                            {
                                // 条件分岐 (汎用1が1か)
                                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                                events.AddRange(Common.IntToBerList(ValueGeneral1));
                                events.AddRange(new List<byte> { 0x00 });
                                events.AddRange(Common.IntToBerList(1));
                                events.AddRange(new List<byte> { 0x00, 0x00 });

                                // 効果音再生
                                events.AddRange(new List<byte> { 0xDA, 0x1E, 0x02 });

                                string mes = SoundItemUse;
                                events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(mes)));
                                events.AddRange(sjisEnc.GetBytes(mes));

                                events.AddRange(new List<byte> { 0x03, 0x64, 0x64, 0x32 });

                                // 壊れる時は個数を一個減らします
                                if (data.Broken == true)
                                {
                                    events.AddRange(new List<byte> { 0xD0, 0x50, 0x02, 0x00, 0x05, 0x01, 0x00 });
                                    events.AddRange(Common.IntToBerList(data.Number));
                                    events.AddRange(new List<byte> { 0x00, 0x01 });
                                }

                                // else 
                                events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7A, 0x01, 0x00, 0x00 });

                                // メッセージ表示位置の修正
                                events.AddRange(new List<byte> { 0xCF, 0x08, 0x02, 0x00, 0x04, 0x00, 0x02, 0x00, 0x00});

                                // メッセージ表示
                                events.AddRange(new List<byte> { 0xCE, 0x7E, 0x02 });
                                mes = "使っても効果がないよ";
                                events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(mes)));
                                events.AddRange(sjisEnc.GetBytes(mes));
                                events.AddRange(new List<byte> { 0x00 });

                                // 条件分岐終了
                                events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                            }
                            
                        }
                        #endregion

                        #region 種
                        // 種
                        if (data.Type == (int)ItemData.TypeEnum.Seed)
                        {
                            // SubTypeが0なら普通の種系アイテム 1以上の場合は魔法扱い 区別はしない

                            // 使用可能か
                            bool available = false;
                            // 能力値増減 （面倒なので最大値チェックはしません）
                            if(data.SeedIncHP != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x00, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncHP));
                            }
                            if (data.SeedIncMP != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x01, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncMP));
                            }
                            if (data.SeedIncATK != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x02, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncATK));
                            }
                            if (data.SeedIncDEF != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x03, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncDEF));
                            }
                            if (data.SeedIncMAT != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x04, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncMAT));
                            }
                            if (data.SeedIncMDF != 0)
                            {
                                available = true;
                                events.AddRange(new List<byte> { 0xD1, 0x3E, 0x01, 0x00, 0x06, 0x02, 0x06, 0x00, 0x05, 0x00 });
                                events.AddRange(Common.IntToBerList(data.SeedIncMDF));
                            }

                            // 効果音再生
                            if (available == true)
                            {
                                events.AddRange(new List<byte> { 0xDA, 0x1E, 0x01});

                                string mes = SoundItemUse;
                                events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(mes)));
                                events.AddRange(sjisEnc.GetBytes(mes));

                                events.AddRange(new List<byte> { 0x03, 0x64, 0x64, 0x32 });
                            }
                            
                            // 壊れる時は個数を一個減らします
                            if (available == true && data.Broken == true)
                            {
                                events.AddRange(new List<byte> { 0xD0, 0x50, 0x01, 0x00, 0x05, 0x01, 0x00 });
                                events.AddRange(Common.IntToBerList(data.Number));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                            }
                        }
                        #endregion

                        #region スイッチ
                        // スイッチ
                        if (data.Type == (int)ItemData.TypeEnum.Switch)
                        {
                            // メニュー強制終了スイッチをONに
                            events.AddRange(new List<byte> { 0xCF, 0x62, 0x01, 0x00, 0x04, 0x00 });
                            events.AddRange(Common.IntToBerList(SwitchMenuInterrupt));
                            events.AddRange(Common.IntToBerList(SwitchMenuInterrupt));
                            events.AddRange(new List<byte> { 0x00 });

                            // アイテム使用時のスイッチをONに
                            events.AddRange(new List<byte> { 0xCF, 0x62, 0x01, 0x00, 0x04, 0x00 });
                            events.AddRange(Common.IntToBerList(data.UseEnableSwitchNumber));
                            events.AddRange(Common.IntToBerList(data.UseEnableSwitchNumber));
                            events.AddRange(new List<byte> { 0x00 });

                            // 壊れる時は個数を一個減らします
                            if (data.Broken == true)
                            {
                                events.AddRange(new List<byte> { 0xD0, 0x50, 0x01, 0x00, 0x05, 0x01, 0x00 });
                                events.AddRange(Common.IntToBerList(data.Number));
                                events.AddRange(new List<byte> { 0x00, 0x01 });
                            }
                        }
                        #endregion

                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
                    }
                }

                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                cs_max = cs + CommonSplit;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ｱｲﾃﾑ使用" + (cs + 1).ToString() + "~" + cs_max.ToString(), events);
                dicCommonEvents.Add(commonNum, d2);

                commonNum++;
            }
        }

        /// <summary>
        /// アイテム名取得コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateGetItemNameCommon(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先(トリガーだけで完結します)
            int triggerCommonNumber = CommonNumberStart + 6;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            List<byte> events = new List<byte>();

            // 負荷軽減のため50区切りで飛ぶ処理を作成
            int label = 1;
            for (int i = 0; i < itemDatas.Count; i += LabelSpan)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + LabelSpan));
                events.AddRange(new List<byte> { 0x02, 0x00 });
                // 指定ラベルへ飛ぶ
                events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                events.AddRange(Common.IntToBerList(label));
                // 条件分岐の終わり側
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                label++;
            }

            label = 1;
            for (int i = 0; i < itemDatas.Count; i++)
            {
                ItemData data = itemDatas[i];

                // 50毎にLabelを設定
                if (i % LabelSpan == 0)
                {
                    events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                    events.AddRange(Common.IntToBerList(label));
                    label++;
                }

                // アイテム番号
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + 1));
                events.AddRange(new List<byte> { 0x00, 0x00 });

                // アイテム名設定
                events.AddRange(new List<byte> { 0xD2, 0x72, 0x01});
                events.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(data.Name)));
                events.AddRange(sjisEnc.GetBytes(data.Name));
                events.AddRange(new List<byte> { 0x01, 0x65 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                // 条件分岐の終わり側
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
            }

            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            List<byte> d2 = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ名取得", events);
            dicCommonEvents.Add(triggerCommonNumber, d2);
        }

        /// <summary>
        /// アイテム情報の取得用コモンを作成します
        /// </summary>
        /// <param name="dicCommonEvents"></param>
        /// <param name="itemDatas"></param>
        private static void CreateGetItemInfoCommons(ref Dictionary<int, List<byte>> dicCommonEvents, List<ItemData> itemDatas)
        {
            // 呼び出し用コモンの格納先
            int triggerCommonNumber = CommonNumberStart + 7;
            // 内部処理コモンの格納先
            int processCommonNumber = CommonNumberStart + 400;

            // 文字バイト数取得用
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            // 開始位置に呼び出し用のコモンを配置 
            int cnt = 0;
            List<byte> events = new List<byte>();
            for (int i = 0; i < ItemNumMax; i += CommonSplit)
            {
                // 条件分岐
                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                events.AddRange(Common.IntToBerList(ValueItemNum));
                events.AddRange(new List<byte> { 0x00 });
                events.AddRange(Common.IntToBerList(i + CommonSplit));
                events.AddRange(new List<byte> { 0x02, 0x00 });

                // イベントの呼び出し
                events.AddRange(new List<byte> { 0xE0, 0x2A, 0x01, 0x00, 0x03, 0x00 }); // 3byte目がインデントなので注意
                events.AddRange(Common.IntToBerList(processCommonNumber + cnt));
                events.AddRange(new List<byte> { 0x00 });

                // exit
                events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 }); // 3byte目がインデントなので注意

                // 条件分岐の終わり側？
                events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                cnt++;
            }
            // 終了
            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref events);
            // コモンイベント情報の結合
            List<byte> d = MergeCommonHeader(triggerCommonNumber.ToString() + "ｱｲﾃﾑ情報取得", events);
            dicCommonEvents.Add(triggerCommonNumber, d);


            // CommonSplit区切りでファイル生成
            // 実際の取得処理
            int commonNum = processCommonNumber;
            for (int cs = 0; cs < ItemNumMax; cs += CommonSplit)
            {
                events = new List<byte>();
                int cs_max = cs + CommonSplit;
                if (cs < itemDatas.Count)
                {
                    if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                    // 負荷軽減のためにLabelSpan区切りでラベルを設定し飛ぶ処理を作成
                    // アイテム情報取得処理をアイテム50毎に変更したため、ラベル関連処理を削除
                    int label = 1;
                    for (int i = cs; i < cs_max; i += LabelSpan)
                    {
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + LabelSpan));
                        events.AddRange(new List<byte> { 0x02, 0x00 });
                        // 指定ラベルへ飛ぶ
                        events.AddRange(new List<byte> { 0xDE, 0x58, 0x01, 0x00, 0x01 });
                        events.AddRange(Common.IntToBerList(label));
                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });

                        label++;
                    }

                    // 取得処理部分
                    label = 1;
                    for (int i = cs; i < cs_max; i++)
                    {
                        ItemData data = itemDatas[i];

                        // 50毎にLabelを設定
                        if (i % LabelSpan == 0)
                        {
                            events.AddRange(new List<byte> { 0xDE, 0x4E, 0x00, 0x00, 0x01 });
                            events.AddRange(Common.IntToBerList(label));
                            label++;
                        }

                        // アイテム番号
                        // 条件分岐
                        events.AddRange(new List<byte> { 0xDD, 0x6A, 0x00, 0x00, 0x06, 0x01 });
                        events.AddRange(Common.IntToBerList(ValueItemNum));
                        events.AddRange(new List<byte> { 0x00 });
                        events.AddRange(Common.IntToBerList(i + 1));
                        events.AddRange(new List<byte> { 0x00, 0x00 });

                        // 値段取得
                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValuePrice));
                        events.AddRange(Common.IntToBerList(ValuePrice));
                        events.AddRange(new List<byte> { 0x00, 0x00 });
                        events.AddRange(Common.IntToBerList(data.Price));
                        events.AddRange(new List<byte> { 0x00 });

                        // 種別取得
                        events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                        events.AddRange(Common.IntToBerList(ValueItemType));
                        events.AddRange(Common.IntToBerList(ValueItemType));
                        events.AddRange(new List<byte> { 0x00, 0x00 });
                        events.AddRange(Common.IntToBerList(data.SearchFilter));
                        events.AddRange(new List<byte> { 0x00 });

                        // 装備不可スイッチをOFFに
                        events.AddRange(new List<byte> { 0xCF, 0x62, 0x01, 0x00, 0x04, 0x00 });
                        events.AddRange(Common.IntToBerList(SwitchEquipPermission));
                        events.AddRange(Common.IntToBerList(SwitchEquipPermission));
                        events.AddRange(new List<byte> { 0x01 });

                        // 装備性能取得
                        if (data.Type == (int)ItemData.TypeEnum.Weapon
                            || data.Type == (int)ItemData.TypeEnum.Shield
                            || data.Type == (int)ItemData.TypeEnum.Armor
                            || data.Type == (int)ItemData.TypeEnum.Helmet
                            || data.Type == (int)ItemData.TypeEnum.Accessory)
                        {
                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueItemATK));
                            events.AddRange(Common.IntToBerList(ValueItemATK));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(data.ATK));
                            events.AddRange(new List<byte> { 0x00 });

                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueItemDEF));
                            events.AddRange(Common.IntToBerList(ValueItemDEF));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(data.DEF));
                            events.AddRange(new List<byte> { 0x00 });

                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueItemMAT));
                            events.AddRange(Common.IntToBerList(ValueItemMAT));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(data.MAT));
                            events.AddRange(new List<byte> { 0x00 });

                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x00 });
                            events.AddRange(Common.IntToBerList(ValueItemMDF));
                            events.AddRange(Common.IntToBerList(ValueItemMDF));
                            events.AddRange(new List<byte> { 0x00, 0x00 });
                            events.AddRange(Common.IntToBerList(data.MDF));
                            events.AddRange(new List<byte> { 0x00 });

                            // 装備許可スイッチの設定
                            for (int j = 0; j < data.EquipPermission.Count; j++)
                            {
                                // 条件分岐
                                events.AddRange(new List<byte> { 0xDD, 0x6A, 0x01, 0x00, 0x06, 0x01 });
                                events.AddRange(Common.IntToBerList(ValuePlayerNum));
                                events.AddRange(new List<byte> { 0x00 });
                                events.AddRange(Common.IntToBerList(data.EquipPermission[j]));
                                events.AddRange(new List<byte> { 0x00, 0x00 });

                                // 装備不可スイッチをOFFに
                                events.AddRange(new List<byte> { 0xCF, 0x62, 0x02, 0x00, 0x04, 0x00 });
                                events.AddRange(Common.IntToBerList(SwitchEquipPermission));
                                events.AddRange(Common.IntToBerList(SwitchEquipPermission));
                                events.AddRange(new List<byte> { 0x00 });

                                // exit
                                events.AddRange(new List<byte> { 0xE0, 0x16, 0x02, 0x00, 0x00 });

                                // 条件分岐の終わり側
                                events.AddRange(new List<byte> { 0x0A, 0x02, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x01, 0x00, 0x00 });
                            }
                        }
                        else
                        {
                            // 装備以外の場合はまとめて0にする
                            events.AddRange(new List<byte> { 0xCF, 0x6C, 0x01, 0x00, 0x07, 0x01 });
                            events.AddRange(new List<byte> { 0x8C, 0x79 });
                            events.AddRange(new List<byte> { 0x8C, 0x7C });
                            events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });
                        }

                        // exit
                        events.AddRange(new List<byte> { 0xE0, 0x16, 0x01, 0x00, 0x00 });

                        // 条件分岐の終わり側
                        events.AddRange(new List<byte> { 0x0A, 0x01, 0x00, 0x00, 0x81, 0xAB, 0x7B, 0x00, 0x00, 0x00 });
                    }
                }

                // 終了
                events.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

                // 注釈メッセージの挿入
                InsertEditCaptionMessage(ref events);
                // コモンイベント情報の結合
                cs_max = cs + CommonSplit;
                List<byte> d2 = MergeCommonHeader(commonNum.ToString() + "ｱｲﾃﾑ情報取得" + (cs + 1).ToString() + "~" + cs_max.ToString(), events);
                dicCommonEvents.Add(commonNum, d2);

                commonNum++;
            }
        }

        private static List<byte> CreatePlainCommon(int commonNum)
        {
            List<byte> temp = new List<byte>();
            temp.AddRange(new List<byte> { 0x00, 0x00, 0x00, 0x00 });

            // 注釈メッセージの挿入
            InsertEditCaptionMessage(ref temp);

            return MergeCommonHeader(commonNum.ToString(), temp);
        }

        /// <summary>
        /// コモンの情報をまとめて追加します
        /// とりあえず現状値を指定する必要があるのは名前だけ
        /// </summary>
        /// <param name="name">関数名</param>
        /// <param name="addCode">追加するbyte配列</param>
        private static List<byte> MergeCommonHeader(string name, List<byte> addCode)
        {
            List<byte> commonData = new List<byte>();

            // 名前 01,文字数,文字列
            commonData.Add(0x01);
            byte[] ba = System.Text.Encoding.GetEncoding("shift_jis").GetBytes(name);
            commonData.AddRange(Common.IntToBerList(ba.Length));
            foreach (byte b in ba)
            {
                commonData.Add(b);
            }

            // 開始条件 呼び出された時
            commonData.Add(0x0B);
            commonData.Add(0x01);
            commonData.Add(0x05);

            // 0x0C,0x0D スイッチ関連省略

            // イベントデータ/データサイズ
            commonData.Add(0x15);
            List<byte> temp = Common.IntToBerList(addCode.Count);
            // サイズ
            commonData.AddRange(Common.IntToBerList(temp.Count));
            // イベント
            commonData.AddRange(temp);

            // イベントデータ/データ
            commonData.Add(0x16);
            // サイズ
            commonData.AddRange(Common.IntToBerList(addCode.Count));
            commonData.AddRange(addCode);

            // 終端コマンド
            commonData.Add(0x00);

            // 書き込み
            return commonData;
        }

        /// <summary>
        /// 先頭に編集注意メッセージを挿入します
        /// </summary>
        /// <param name="code"></param>
        private static void InsertEditCaptionMessage(ref List<byte> code)
        {
            List<byte> temp = new List<byte>();
            string mes;
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            mes = "このコードは外部ツールによって自動生成されました。";
            temp.AddRange(new List<byte> { 0xE0, 0x7A, 0x00 });
            temp.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(mes)));
            temp.AddRange(sjisEnc.GetBytes(mes));
            temp.AddRange(new List<byte> { 0x00 });

            mes = "エディターで編集しないでください。";
            temp.AddRange(new List<byte> { 0x81, 0xAF, 0x0A, 0x00 });
            temp.AddRange(Common.IntToBerList(sjisEnc.GetByteCount(mes)));
            temp.AddRange(sjisEnc.GetBytes(mes));
            temp.AddRange(new List<byte> { 0x00 });

            code.InsertRange(0, temp);
        }

        private static void SetClipboardData(byte[] b)
        {
            try
            {
                Clipboard.Clear();

                ClipboardManager.OpenClipboard(IntPtr.Zero);

                byte[] array = b.ToArray();
                int size = Marshal.SizeOf(array[0]) * array.Length;
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(array, 0, ptr, size);

                ClipboardManager.SetClipboardData(584, ptr); // コモンイベントなので584

                //Marshal.FreeHGlobal(ptr);
            }
            catch
            {
                throw;
            }
            finally
            {
                ClipboardManager.CloseClipboard();
            }
        }
    }
}
