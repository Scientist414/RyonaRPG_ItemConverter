using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RyonaRPG_ItemDataConverter
{
    class Common
    {
        public static int BerToIntValue(byte[] byteArray)
        {
            int ret = 0;

            double val = 0;

            int index = 0;
            int length = byteArray.Length;
            int len = length - 1;
            while (len >= 0)
            {
                byte b = byteArray[index];
                if (len != 0) b -= 0x80;
                val += b * Math.Pow(128, len);

                len--;
                index++;
            }

            // 負数
            if (val >= 2147483648) val = val - 4294967296;

            ret = (int)val;

            return ret;
        }

        public static List<byte> IntToBerList(int value)
        {
            List<byte> ret = new List<byte>();

            // 128^nで割れる個数を保持します
            int cnt = 1;
            int temp;
            while (true)
            {
                temp = value % (int)Math.Pow(128, cnt);
                value -= temp;
                if (cnt != 1)
                {
                    temp /= (int)Math.Pow(128, cnt - 1);
                    temp += 128;
                }
                ret.Insert(0, Convert.ToByte(temp));
                if (value == 0) break;
                cnt++;
            }

            return ret;
        }
    }
}
