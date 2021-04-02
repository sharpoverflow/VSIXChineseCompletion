using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXChineseCompletion
{
    public static class Utils
    {
        public static void Log(this string s)
        {
            File.AppendAllText("D:/visx1.txt", s + "\r\n");
        }

        public static bool IsChinese(this char c)
        {
            return 0x4e00 <= c && c <= 0x9fbb;
        }

        public static bool HasChinese(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            for(int i = 0; i < s.Length; i++)
            {
                if (s[i].IsChinese()) return true;
            }
            return false;
        }
    }
}
