using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyonaRPG_ItemDataConverter
{
    class ByteArrayConverter
    {
        /// <summary>
        /// 1個のByteArrayDataを各アイテムデータに分割します
        /// </summary>
        public ByteArrayConverter()
        {

        }

        public static List<ItemData> Convert(byte[] byteData)
        {
            // 8byte目から参照します
            int index = 8;
            int num = 1;

            List<ItemData> itemDatas = new List<ItemData>();
            // 0x00に到達したらそのアイテムは終了
            while (index < byteData.Length)
            {
                // アイテムを一個一個のデータに切り分けます
                Dictionary<int, byte[]> primalData = new Dictionary<int, byte[]>();
                while (byteData[index] != 0x00)
                {
                    int id = byteData[index];
                    index++;
                    int length = byteData[index];
                    index++;
                    byte[] data = new byte[length];
                    Array.Copy(byteData, index, data, 0, length);

                    primalData.Add(id, data);

                    index+=length;
                }

                // ItemDataクラスに変換
                ItemData itemData = new ItemData(primalData, num);
                itemDatas.Add(itemData);

                num++;
                index++;
            }

            return itemDatas;
        }
    }
}
