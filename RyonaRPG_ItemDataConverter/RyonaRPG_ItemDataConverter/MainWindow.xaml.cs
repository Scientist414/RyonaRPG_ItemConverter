using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RyonaRPG_ItemDataConverter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ItemData> ItemDatas = new List<ItemData>();

        public MainWindow()
        {
            InitializeComponent();
            textBox_SystemPicture.Text = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"システムB.png");

#if DEBUG
            button_outputCSV.Visibility = Visibility.Visible;
#endif
        }

        private void button_output_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] byteData;
                try
                {
                    // クリップボードチェック
                    ClipboardManager.OpenClipboard(IntPtr.Zero);
                    IntPtr handle = ClipboardManager.GetClipboardData(576);
                    if (handle == IntPtr.Zero)
                    {
                        System.Windows.MessageBox.Show("RPGツクール2000のデータベースで\r\nアイテムデータをコピーしてから実行してください", "警告");
                        return;
                    }

                    IntPtr pointer = IntPtr.Zero;
                    pointer = ClipboardManager.GlobalLock(handle);
                    if (pointer == IntPtr.Zero)
                    {
                        System.Windows.MessageBox.Show("アイテムデータの読み込みに失敗しました", "エラー");
                        return;
                    }
                    int size = ClipboardManager.GlobalSize(handle);

                    byteData = new byte[size];
                    Marshal.Copy(pointer, byteData, 0, size);

                    ClipboardManager.GlobalUnlock(pointer);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ClipboardManager.CloseClipboard();
                }

                System.Windows.MessageBox.Show("アイテム画像の出力、コモンイベントの作成を開始します\r\n量が多いと時間が掛かります", "情報");

                //byte[] byteData = data.ToArray();
                ItemDatas = ByteArrayConverter.Convert(byteData);

                // フォルダ作成
                string pathName = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"Picture\Menu\Item", @"Name");
                DirectoryInfo di = new DirectoryInfo(pathName);
                if (di.Exists == false) di.Create();
                string pathDesc = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"Picture\Menu\Item", @"Desc");
                di = new DirectoryInfo(pathDesc);
                if (di.Exists == false) di.Create();

                string systemPicture = textBox_SystemPicture.Text;
                // ピクチャの生成
                for (var i = 0; i < ItemDatas.Count; i++)
                {
                    ItemData item = ItemDatas[i];
                    // ID
                    string id = item.GetIDString();

                    Bitmap bmp;
                    // 名前
                    bmp = BitmapGenerator.CreateBitmapItemName(systemPicture, item);
                    bmp.Save(System.IO.Path.Combine(pathName, id) + ".png");
                    bmp.Dispose();

                    // 説明
                    bmp = BitmapGenerator.CreateBitmapItemDescription(systemPicture, item);
                    bmp.Save(System.IO.Path.Combine(pathDesc, id) + ".png");
                    bmp.Dispose();
                }
                // TKCodeの生成
                CommonEventGenerator.CodeToClipboard(ItemDatas);

                System.Windows.MessageBox.Show("コンバートが完了しました\r\nコモンイベント「1301」に貼り付けてください", "情報");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "エラー");
            }
            finally
            {
                //this.Close();
                ClipboardManager.CloseClipboard();
            }
        }

        private void button_outputCSV_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteData;
            try
            {
                // クリップボードチェック
                ClipboardManager.OpenClipboard(IntPtr.Zero);
                IntPtr handle = ClipboardManager.GetClipboardData(576);
                if (handle == IntPtr.Zero)
                {
                    System.Windows.MessageBox.Show("RPGツクール2000のデータベースで\r\nアイテムデータをコピーしてから実行してください", "警告");
                    return;
                }

                IntPtr pointer = IntPtr.Zero;
                pointer = ClipboardManager.GlobalLock(handle);
                if (pointer == IntPtr.Zero)
                {
                    System.Windows.MessageBox.Show("アイテムデータの読み込みに失敗しました", "エラー");
                    return;
                }
                int size = ClipboardManager.GlobalSize(handle);

                byteData = new byte[size];
                Marshal.Copy(pointer, byteData, 0, size);

                ClipboardManager.GlobalUnlock(pointer);
            }
            catch
            {
                throw;
            }
            finally
            {
                ClipboardManager.CloseClipboard();
            }

            System.Windows.MessageBox.Show("アイテム画像の出力、コモンイベントの作成を開始します\r\n量が多いと時間が掛かります", "情報");

            ItemDatas = ByteArrayConverter.Convert(byteData);

            // フォルダ作成
            string path = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"ItemInfo.csv");
            {
                FileInfo fi = new FileInfo(path);
                DirectoryInfo di = fi.Directory;
                if (di.Exists == false) di.Create();
            }

            // csv文字列生成
            string str = "";
            StreamWriter file = new StreamWriter(path, false, Encoding.GetEncoding("Shift_JIS"));
            for (var i = 0; i < ItemDatas.Count; i++)
            {
                ItemData data = ItemDatas[i];
                str = data.Name + ",";
                str += data.Description;
                file.WriteLine(str);
            }
            file.Close();

            System.Windows.MessageBox.Show("CSV出力が完了しました", "情報");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            dialog.Filter = "画像ファイル (*.png) | *.png";

            if (dialog.ShowDialog() == true)
            {
                textBox_SystemPicture.Text = dialog.FileName;
            }
        }
    }
}
