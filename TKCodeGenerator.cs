using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace RyonaRPG_ItemDataConverter
{
    class TKCodeGenerator
    {
        // アイテム番号が入っている変数番号
        const int ValueItemNum = 191;
        // ラベルの配置間隔
        const int LabelSpan = 50;
        // ピクチャ番号(負荷軽減のために22-44だけ使う設定になっています)
        const int PictureNumMin = 22;
        const int PictureNumMax = 44;
        const int PictureNumSpan = 2;
        // 開始コモン番号
        const int CommonNumberStart = 1301;
        // アイテム番号毎コモン区切り
        const int CommonSplit = 200;

        // ファイル名
        const string Directory_TKCode = "TKCode";
        const string FileName_Draw = "TKCode_{0}.txt";

        public static void OutputCode_Draw(List<ItemData> itemDatas)
        {
            string NL = Environment.NewLine;
            string code = "";

            int commonNum = CommonNumberStart;

            // 負荷軽減のためCommonSplitの番号毎に別コモン化します
            int cnt = 1;
            for (int i = 0; i < itemDatas.Count; i += CommonSplit)
            {
                code += string.Format("If(1, {0}, 0, {1}, 2, 0)", ValueItemNum, i + CommonSplit) + NL;
                code += string.Format("Call(0, {0}, 0)", commonNum + cnt) + NL;
                code += "Exit" + NL;
                code += "EndIf" + NL;
                cnt++;
            }
            Output(code, string.Format(FileName_Draw, commonNum.ToString("0000")));

            // CommonSplit区切りでファイル生成
            for (int cs = 0; cs < itemDatas.Count; cs += CommonSplit)
            {
                int cs_max = cs + CommonSplit;
                if (cs_max > itemDatas.Count) cs_max = itemDatas.Count;
                commonNum++;
                // 負荷軽減のために50区切りでラベルを設定し飛ぶ処理を作成
                code = "";
                int label = 1;
                for (int i = cs; i < cs_max; i += LabelSpan)
                {
                    code += string.Format("If(1, {0}, 0, {1}, 2, 0)", ValueItemNum, i + LabelSpan) + NL;
                    code += "\t" + string.Format("LabelJump({0})", label) + NL;
                    code += "EndIf" + NL;
                    label++;
                }

                // TKCodeの生成
                label = 1;
                for (int i = cs; i < cs_max; i++)
                {
                    ItemData data = itemDatas[i];

                    // 50毎にLabelを設定
                    if (i % LabelSpan == 0)
                    {
                        code += string.Format("Label({0})", label) + NL;
                        label++;
                    }

                    // アイテム番号
                    code += string.Format("If(1, {0}, 0, {1}, 0, 0)", ValueItemNum, i + 1) + NL;

                    // ピクチャ番号
                    for (int j = PictureNumMin; j <= PictureNumMax; j += PictureNumSpan)
                    {
                        code += "\t" + string.Format("If(1, 333, 0, {0}, 0, 0)", j) + NL;
                        code += "\t\t" + string.Format("Picture(\"Menu\\Item\\Name\\{0}\", {1}, 1, 166, 167, 0, 100, 0, 1, 100, 100, 100, 100, 0, 123317756)", data.NameNumber, j) + NL;
                        code += "\t" + "EndIf" + NL;
                    }
                    code += "Exit" + NL;
                    code += "EndIf" + NL;
                }

                // 出力
                Output(code, string.Format(FileName_Draw, commonNum.ToString("0000")));
            }
        }

        private static void Output(string code, string fileName)
        {
            // ファイル出力
            string path = System.IO.Path.Combine(System.Environment.CurrentDirectory, Directory_TKCode, fileName);
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Directory.Exists == false)
                {
                    fi.Directory.Create();
                    fi.Create();
                }
            }

            File.WriteAllText(path, code);
        }
    }
}
