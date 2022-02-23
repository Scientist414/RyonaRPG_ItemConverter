using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace RyonaRPG_ItemDataConverter
{
    class ItemData
    {
        /// <summary>
        /// アイテム種別
        /// </summary>
        public enum TypeEnum
        {
            Normal = 0,
            Weapon = 1,
            Shield = 2,
            Armor = 3,
            Helmet = 4,
            Accessory = 5,
            Healing = 6,
            Book = 7,
            Seed = 8,
            Special = 9,
            Switch = 10
        }

        /// <summary>種別、サブ種別の組み合わせパターン</summary>
        Dictionary<int, Dictionary<string, int>> DicTypeSubTypeColection = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>アイテムID 1から始まる</summary>
        public int Number = 0;
        public string NameNumber
        {
            get { return Number.ToString("0000"); }
        }

        /// <summary>名前</summary>
        public string Name = "";

        /// <summary>説明</summary>
        public string Description = "";

        /// <summary>種別</summary>
        public int Type = 0;

        /// <summary>サブ種別</summary>
        public int SubType = 0;

        /// <summary>検索フィルタ値</summary>
        public int SearchFilter
        {
            get { return Type * 100 + SubType; }
        }

        /// <summary>種別+サブ種別</summary>
        public string MergedType
        {
            get { return Type.ToString("00") + SubType.ToString("00"); }
        }

        /// <summary>値段</summary>
        public int Price = 0;

        /// <summary>装備専用 - 能力変化</summary>
        public int ATK = 0;
        public int DEF = 0;
        public int MAT = 0;
        public int MDF = 0;

        /// <summary>薬専用 - 回復量</summary>
        public int HealingHPper = 0;
        public int HealingHPpt = 0;
        public int HealingMPper = 0;
        public int HealingMPpt = 0;

        /// <summary>種専用 - 能力値増加量</summary>
        public int SeedIncHP = 0;
        public int SeedIncMP = 0;
        public int SeedIncATK = 0;
        public int SeedIncDEF = 0;
        public int SeedIncMAT = 0;
        public int SeedIncMDF = 0;

        /// <summary>消耗品専用 - 使うとなくなるか否か</summary>
        public bool Broken = false;

        /// <summary>スイッチ専用 - 使用時ONにするスイッチ</summary>
        public int UseEnableSwitchNumber = 0;

        /// <summary>装備専用 - 装備不可能なキャラクターのIDが入ります 最低1</summary>
        public List<int> EquipPermission = new List<int>();

        /// <summary> 状態異常ID - チェックが入っている物がここに入る
        /// 武器時 - 状態異常付与
        /// 防具時 - 状態異常耐性 
        /// 薬時 - 状態異常回復  
        /// </summary>
        public List<int> Debuff = new List<int>();

        /// <summary>装備専用 - 装備時にONにするスイッチ番号、変数等の管理</summary>
        public List<int> EquipEnableSwitchNum = new List<int>();

        /// <summary>
        /// 種別、サブ種別の組み合わせパターンを作成します
        /// </summary>
        private void MakeTypeSubTypeCollection()
        {
            // 00通常物品
            Dictionary<string, int> dic;
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"換金", 1},
                {"素", 2},
                {"依頼", 3},
                {"大事", 4},
                {"ﾋﾟｱｽ", 5},
                {"他", 6},
                {"鍛冶", 7}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Normal, dic);

            // 01武器
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"格闘", 1},
                {"ﾌﾚｲﾙ", 2},
                {"鉄球", 3},
                {"杖",   4},
                {"槌", 4},
                {"鞭",   5},
                {"短剣", 6},
                {"剣",   7},
                {"鎖鎌", 8},
                {"大剣", 9},
                {"斧",   10},
                {"大斧", 10},
                {"小剣", 11},
                {"槍",   12},
                {"弓",   13},
                {"銃",   14}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Weapon, dic);

            // 02盾/弾
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"矢", 1},
                {"弾", 2},
                {"盾", 3}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Shield, dic);

            // 03服
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"服", 1}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Armor, dic);

            // 04兜
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"兜", 1}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Helmet, dic);

            // 05装飾品
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"装飾", 1},
                {"下着", 2}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Accessory, dic);

            // 06薬
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"HP回復", 1},
                {"MP回復", 2},
                {"治療",   3}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Healing, dic);

            // 07本
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Book, dic);

            // 08種
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"強化", 1},
                {"通",   2},
                {"付",   3},
                {"反",   4},
                {"補",   5}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Seed, dic);

            // 09特殊
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Special, dic);

            // 10スイッチ
            dic = new Dictionary<string, int>()
            {
                {"", 0},
                {"未指定", 0},
                {"道具", 1},
                {"無限", 2},
                {"壺", 3},
                {"回復", 4},
                {"ｱﾋﾞﾘﾃｨ", 5}
            };
            DicTypeSubTypeColection.Add((int)TypeEnum.Switch, dic);
        }

        public ItemData(int number, string name)
        {
            // ID
            Number = number;
            // 名前
            Name = name;
        }
        public ItemData(Dictionary<int, byte[]> primalData, int number)
        {
            try
            {
                MakeTypeSubTypeCollection();

                // ID
                Number = number;
                // 名前
                if (primalData.ContainsKey(0x01)) Name = Encoding.GetEncoding("shift_jis").GetString(primalData[0x01]);
                // 説明
                if (primalData.ContainsKey(0x02)) Description = Encoding.GetEncoding("shift_jis").GetString(primalData[0x02]);
                // 種別
                if (primalData.ContainsKey(0x03)) Type = Common.BerToIntValue(primalData[0x03]);

                // 値段
                if (primalData.ContainsKey(0x05)) Price = Common.BerToIntValue(primalData[0x05]);
                // 消耗品専用 - 使うとなくなるか否か
                if (primalData.ContainsKey(0x06))
                {
                    if(Common.BerToIntValue(primalData[0x06]) != 0)
                    {
                        Broken = true;
                    }
                }
                else
                {
                    // 含んでいない時はデフォルト（1）
                    Broken = true;
                }

                // 各能力変化
                if (primalData.ContainsKey(0x0B)) ATK = Common.BerToIntValue(primalData[0x0B]);
                if (primalData.ContainsKey(0x0C)) DEF = Common.BerToIntValue(primalData[0x0C]);
                if (primalData.ContainsKey(0x0D)) MAT = Common.BerToIntValue(primalData[0x0D]);
                if (primalData.ContainsKey(0x0E)) MDF = Common.BerToIntValue(primalData[0x0E]);

                // 回復量
                if (primalData.ContainsKey(0x20)) HealingHPper = Common.BerToIntValue(primalData[0x20]);
                if (primalData.ContainsKey(0x21)) HealingHPpt  = Common.BerToIntValue(primalData[0x21]);
                if (primalData.ContainsKey(0x22)) HealingMPper = Common.BerToIntValue(primalData[0x22]);
                if (primalData.ContainsKey(0x23)) HealingMPpt  = Common.BerToIntValue(primalData[0x23]);

                // 種 能力値増加量
                if (primalData.ContainsKey(0x29)) SeedIncHP = Common.BerToIntValue(primalData[0x29]);
                if (primalData.ContainsKey(0x2A)) SeedIncMP = Common.BerToIntValue(primalData[0x2A]);
                if (primalData.ContainsKey(0x2B)) SeedIncATK = Common.BerToIntValue(primalData[0x2B]);
                if (primalData.ContainsKey(0x2C)) SeedIncDEF = Common.BerToIntValue(primalData[0x2C]);
                if (primalData.ContainsKey(0x2D)) SeedIncMAT = Common.BerToIntValue(primalData[0x2D]);
                if (primalData.ContainsKey(0x2E)) SeedIncMDF = Common.BerToIntValue(primalData[0x2E]);

                // スイッチ専用 - 使用時ONにするスイッチ
                if (primalData.ContainsKey(0x37)) UseEnableSwitchNumber = Common.BerToIntValue(primalData[0x37]);

                // 装備専用 - 装備不可能なキャラクターのIDが入ります 最低1
                if (primalData.ContainsKey(0x3E))
                {
                    int index = 0;
                    while(index < primalData[0x3E].Length)
                    {
                        if (primalData[0x3E][index] == 0) EquipPermission.Add(index + 1);

                        index++;
                    }
                }

                // 状態異常ID
                if (primalData.ContainsKey(0x40))
                {
                    int index = 0;
                    while (index < primalData[0x40].Length)
                    {
                        if (primalData[0x40][index] == 1) Debuff.Add(index + 1);

                        index++;
                    }
                }

                // 装備専用 - 装備時にONにするスイッチ番号、変数等の管理
                if (primalData.ContainsKey(0x42))
                {
                    
                }

                // サブ種別
                SetSubType();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// サブ種別の設定
        /// 他のパラメータを参照して自動的に決定します
        /// </summary>
        private void SetSubType()
        {

            string StrSubType = "";
            // デフォルトの設定
            switch (Type)
            {
                case (int)TypeEnum.Normal   : StrSubType = "大事"; break;
                case (int)TypeEnum.Weapon   : StrSubType = "未指定"; break;
                case (int)TypeEnum.Shield   : StrSubType = "盾"; break;
                case (int)TypeEnum.Armor    : StrSubType = "服"; break;
                case (int)TypeEnum.Helmet   : StrSubType = "兜"; break;
                case (int)TypeEnum.Accessory: StrSubType = "装飾"; break;
                case (int)TypeEnum.Healing  : StrSubType = "HP回復";
                                              if (HealingHPper + HealingHPpt < HealingMPper + HealingMPpt) StrSubType = "MP回復";
                                              if (HealingHPper + HealingHPpt + HealingMPper + HealingMPpt == 0 && Debuff.Count >= 1) StrSubType = "治療";
                                              break;
                case (int)TypeEnum.Seed     : StrSubType = "強化"; break;
                case (int)TypeEnum.Switch   : StrSubType = "道具"; 
                                              if(Broken == false) StrSubType = "無限";
                                              break;
            }

            // 説明文の先頭のサブ種別IDを参照します
            if (Description != "")
            {
                if (Description[0] == '[')
                {
                    string st = "";
                    bool end = false;
                    for (var i = 1; i < Description.Length; i++)
                    {
                        if (Description[i] == ']')
                        {
                            end = true;
                            break;
                        }
                        st += Description[i];
                    }

                    if (end == true)
                    {
                        StrSubType = st;
                    }
                }
            }

            if(DicTypeSubTypeColection.ContainsKey(Type))
            {
                Dictionary<string, int> dic = DicTypeSubTypeColection[Type];
                if(dic.ContainsKey(StrSubType))
                {
                    SubType = dic[StrSubType];
                }
            }
        }

        /// <summary>
        /// アイテムのIDの文字列(4桁0埋め)を取得します
        /// </summary>
        /// <returns>4桁の番号(文字列)</returns>
        public string GetIDString()
        {
            return Number.ToString("0000");
        }
    }
}
