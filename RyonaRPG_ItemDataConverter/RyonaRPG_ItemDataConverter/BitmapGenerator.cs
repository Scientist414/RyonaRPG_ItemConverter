using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace RyonaRPG_ItemDataConverter
{
    class BitmapGenerator
    {
        public const int NameWidth = 136;
        public const int NameHeight = 16;
        public const int DescWidth = 304;
        public const int DescHeight = 16;

        const int WidthZenkaku = 12;
        const int WidthHankaku = 6;
        const int WidthIcon = 12;
        const int IconLeft = 2; // アイコンの後ろに入れる隙間
        const int IconRight = 2;

        const int DrawTop = 2;

        public static Bitmap CreateBitmapItemName(string systemPicture, ItemData itemData)
        {
            return CreateBitmap(systemPicture, itemData.Name, NameWidth, NameHeight, true, (itemData.SubType - 1) * 12, itemData.Type * 12);
        }

        public static Bitmap CreateBitmapItemDescription(string systemPicture, ItemData itemData)
        {
            return CreateBitmap(systemPicture, itemData.Description, DescWidth, DescHeight);
        }

        public static Bitmap CreateBitmap(string systemPicture, string text, int width, int height, bool addIcon = false, int iconLeft = -1, int iconTop = -1)
        {
            Bitmap bmpBase = null;
            Bitmap bmpIcon = null;
            Bitmap colShadow = null;
            Bitmap colLetter = null;
            Bitmap mask = null;

            try
            {
                int w = width;
                int h = height;

                // システム画像を読み込む
                string path = System.IO.Path.Combine(systemPicture);
                bmpBase = new Bitmap(path);

                // アイコン画像を読み込む
                if (addIcon == true && iconLeft >= 0 && iconTop >= 0)
                {
                    path = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"Icons.png");
                    FileInfo fiIcon = new FileInfo(path);
                    if (fiIcon.Exists == true)
                    { 
                        Rectangle r = new Rectangle(iconLeft, iconTop, 12, 12);
                        Bitmap bmpIconBase = new Bitmap(path);
                        bmpIcon = bmpIconBase.Clone(r, bmpIconBase.PixelFormat);
                        bmpIconBase.Dispose();
                    }
                }

                // 影の色範囲を切り抜く
                Rectangle rect = new Rectangle(16, 32, 16, 16);
                colShadow = bmpBase.Clone(rect, bmpBase.PixelFormat);

                // 文字の色範囲を切り抜く
                rect = new Rectangle(0, 48, 16, 16);
                colLetter = bmpBase.Clone(rect, bmpBase.PixelFormat);

                // maskの生成
                mask = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(mask);
                
                //フォントオブジェクトの作成
                Font fnt = new Font("ＭＳ ゴシック", 9);
                //文字列を位置(0,0)、青色で表示
                //1文字ずつ12pxずつずらして描画する
                int xp = -2; // 2pxくらいずらして置かないと左に隙間ができます
                if (addIcon) xp += WidthIcon + IconLeft + IconRight;
                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                for (int i = 0; i < text.Length; i++)
                {
                    string chr = text[i].ToString();
                    g.DrawString(chr, fnt, System.Drawing.Brushes.White, xp, DrawTop);

                    int num = sjisEnc.GetByteCount(chr);
                    if (num == 2) xp += WidthZenkaku;
                    else xp += WidthHankaku;
                }

                //リソースを解放する
                fnt.Dispose();
                g.Dispose();


                // 出力用
                Bitmap ibmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                // パレット作成
                System.Drawing.Imaging.ColorPalette palette = bmpBase.Palette;
                List<Color> colors = new List<Color>();
                colors.Add(palette.Entries[0]);
                MergeColorsDistinct(ref colors, GetColorsFromImage(colShadow));
                MergeColorsDistinct(ref colors, GetColorsFromImage(colLetter));
                if(bmpIcon != null) MergeColorsDistinct(ref colors, GetColorsFromImage(bmpIcon));

                var pal = ibmp.Palette;
                for(var i=0; i<colors.Count; i++)
                {
                    pal.Entries[i] = colors[i];
                }
                ibmp.Palette = pal;


                // 配列から画像生成
                byte[] data = new byte[w * h];

                // 背景色で塗りつぶし
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        data[y * w + x] = 0x00;
                    }
                }

                // 影を塗る
                for (int y = 0; y < h; y++)
                {
                    // yの高さで色を選出
                    Color pcol = colShadow.GetPixel(0, y);
                    int index = colors.IndexOf(pcol);
                    byte b = BitConverter.GetBytes(index)[0];
                    for (int x = 0; x < w; x++)
                    {
                        if (x == w - 1 || y == h - 1) continue;

                        Color color = mask.GetPixel(x, y);

                        // 閾値より大きい？
                        float fTemp = color.GetBrightness();
                        if (fTemp > 0.5)
                        {
                            data[(y+1) * w + (x+1)] = b;
                        }
                    }
                }

                // 文字を塗る
                for (int y = 0; y < h; y++)
                {
                    // yの高さで色を選出
                    Color pcol = colLetter.GetPixel(0, y);
                    int index = colors.IndexOf(pcol);
                    byte b = BitConverter.GetBytes(index)[0];
                    for (int x = 0; x < w; x++)
                    {
                        Color color = mask.GetPixel(x, y);

                        // 閾値より大きい？
                        float fTemp = color.GetBrightness();
                        if (fTemp > 0.5)
                        {
                            data[y * w + x] = b;
                        }
                    }
                }

                if (bmpIcon != null)
                {
                    palette = bmpIcon.Palette;
                    Color iconBack = palette.Entries[0];
                    // アイコンを描画する
                    for (int y = 0; y < bmpIcon.Size.Height; y++)
                    {
                        for (int x = 0; x < bmpIcon.Size.Width; x++)
                        {
                            Color pcol = bmpIcon.GetPixel(x, y);
                            if (pcol != iconBack)
                            {
                                int index = colors.IndexOf(pcol);
                                byte b = BitConverter.GetBytes(index)[0];
                                data[(y + DrawTop) * w + x + IconLeft] = b;
                            }
                        }
                    }
                    bmpIcon.Dispose();
                }

                rect = new Rectangle(0, 0, ibmp.Width, ibmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                ibmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                PixelFormat.Format8bppIndexed);

                // Bitmapの先頭アドレスを取得
                IntPtr ptr = bmpData.Scan0;
                Marshal.Copy(data, 0, ptr, data.Length);

                ibmp.UnlockBits(bmpData);

                return ibmp;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (bmpBase != null) bmpBase.Dispose();
                if (bmpIcon != null) bmpIcon.Dispose();
                if (colShadow != null) colShadow.Dispose();
                if (colLetter != null) colLetter.Dispose();
                if (mask != null) mask.Dispose();
            }
        }

        /// <summary>
        /// 画像内で使われている色を全て取得します
        /// </summary>
        /// <param name="img"></param>
        /// <returns>色リスト</returns>
        private static List<Color> GetColorsFromImage(Bitmap img)
        {
            List<Color> ret = new List<Color>();
            for (int y = 0; y < img.Size.Height; y++)
            {
                for (int x = 0; x < img.Size.Width; x++)
                {
                    Color color = img.GetPixel(x, y);
                    if(!ret.Contains(color))
                    {
                        ret.Add(color);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// list1にlist2を結合します
        /// 重複は削除します
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        private static void MergeColorsDistinct(ref List<Color> list1, List<Color> list2)
        {
            for(int i=0; i<list2.Count; i++)
            {
                var val = list2[i];
                if(!list1.Contains(val))
                {
                    list1.Add(val);
                }
            }
        }
    }
}
