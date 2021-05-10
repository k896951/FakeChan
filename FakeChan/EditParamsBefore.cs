using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FakeChan
{
    public class EditParamsBefore
    {
        public int ChangedVoiceNo { get; private set; }

        public string ChangedTalkText { get; private set; }

        public static int LimitTextLength { get; set; }
        public static bool IsUseSuffixString { get; set; }
        public static string SuffixString { get; set; }

        static ObservableCollection<ReplaceDefinition> Regexs;

        public static void CopyRegExs(ref ObservableCollection<ReplaceDefinition> rx)
        {
            Regexs = rx;
        }

        public int EditInputString(int orgVoice, string talkText)
        {
            string s = talkText;

            ChangedVoiceNo = orgVoice;

            s = CheckSpeedTag(s);
            s = ReplaceString(s);
            s = CutString(s);

            ChangedTalkText = s;

            return ChangedVoiceNo;
        }

        private string CheckSpeedTag(string talkText)
        {
            string s = talkText;

            if (s.Length > 2)
            {
                string s2 = s.Substring(0, 2);
                switch (s2)
                {
                    case "y)": ChangedVoiceNo = 1; s = s.Substring(2); break;
                    case "b)": ChangedVoiceNo = 2; s = s.Substring(2); break;
                    case "h)": ChangedVoiceNo = 3; s = s.Substring(2); break;
                    case "d)": ChangedVoiceNo = 4; s = s.Substring(2); break;
                    case "a)": ChangedVoiceNo = 5; s = s.Substring(2); break;
                    case "r)": ChangedVoiceNo = 6; s = s.Substring(2); break;
                    case "t)": ChangedVoiceNo = 7; s = s.Substring(2); break;
                    case "g)": ChangedVoiceNo = 8; s = s.Substring(2); break;
                    default: break;
                }
            }

            return s;
        }

        private string ReplaceString(string talkText)
        {
            string s = talkText;

            if (s.Length != 0)
            {
                for (int idx = 0; idx < Regexs.Count; idx++)
                {
                    if (Regexs[idx].Apply)
                    {
                        try { s = Regex.Replace(s, Regexs[idx].MatchingPattern, Regexs[idx].ReplaceText); } catch (Exception) { }
                    }
                }
            }

            return s;
        }

        private string CutString(string talkText)
        {
            StringBuilder sb = new StringBuilder(talkText);

            if (sb.Length > LimitTextLength)
            {
                sb.Remove(LimitTextLength, sb.Length - LimitTextLength);

                if (IsUseSuffixString)
                {
                    sb.Append(SuffixString);
                }
            }

            return sb.ToString();
        }
    }
}
