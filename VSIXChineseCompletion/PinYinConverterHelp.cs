using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VSIXChineseCompletion
{
    public class PinYinConverterHelp
    {
        public static List<string> GetTotalPingYin(string str, out bool hasChinese, bool pascal = false)
        {
            if (str == null)
            {
                hasChinese = false;
                return new List<string>() { str };
            }

            var chs = str.ToCharArray();

            bool[] isch = new bool[chs.Length];

            bool hasch = false;
            for (int i = 0; i < chs.Length; i++)
            {
                isch[i] = chs[i] > 255 && ChineseChar.IsValidChar(chs[i]);
                hasch |= isch[i];
            }

            if (!hasch)
            {
                hasChinese = false;
                return new List<string>() { str };
            }

            Dictionary<int, List<string>> totalPingYins = new Dictionary<int, List<string>>();
            for (int i = 0; i < chs.Length; i++)
            {
                var pinyins = new List<string>();
                var ch = chs[i];
                if (isch[i])
                {
                    ChineseChar cc = new ChineseChar(ch);
                    pinyins = cc.Pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

                    pinyins = pinyins.ConvertAll(p => p.Substring(0, p.Length - 1));
                    if (pascal) pinyins = pinyins.ConvertAll(p => p[0] + p.Substring(1).ToLower());
                    else pinyins = pinyins.ConvertAll(p => p.ToLower());

                    pinyins = pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                }
                else
                {
                    pinyins.Add(ch.ToString());
                }

                if (pinyins.Any())
                {
                    totalPingYins[i] = pinyins;
                }
            }

            List<string> TotalPingYin = new List<string>();
            foreach (var pinyins in totalPingYins)
            {
                var items = pinyins.Value;
                if (TotalPingYin.Count <= 0)
                {
                    TotalPingYin = items;
                }
                else
                {
                    var newTotalPingYins = new List<string>();
                    foreach (var totalPingYin in TotalPingYin)
                    {
                        newTotalPingYins.AddRange(items.Select(item => totalPingYin + item));
                    }
                    newTotalPingYins = newTotalPingYins.Distinct().ToList();
                    TotalPingYin = newTotalPingYins;
                }
            }
            hasChinese = true;
            return TotalPingYin;
        }
    }
}
