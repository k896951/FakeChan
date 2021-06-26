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

        public static bool VriEng { get; set; }
        public static bool VriNoRep { get; set; }
        public static int VriAvator { get; set; }

        public bool Judge { get; set; }


        static ObservableCollection<ReplaceDefinition> Regexs;
        static Regex Vrir1 = new Regex(@"[0-9 \t]");
        static Regex Vrir2 = new Regex(@"[a-zA-ZàÀèÈùÙéÉâÂêÊîÎûÛôÔäÄëËïÏüÜÿŸöÖñÑãÃõÕœŒçÇẞß!""#$%&'\(\)\-=^~|\\`@\{\[\]\};+:*,./?]");
        static Regex VoiceCommand1 = new Regex(@"^\([Vv][Oo][Ii][Cc][Ee][ \t]{1,}[^ \t\)]{1,}[ \t]{0,}\)");
        static char[] SepChars = new char[] { ' ', '\t' };


        public static void CopyRegExs(ref ObservableCollection<ReplaceDefinition> rx)
        {
            Regexs = rx;
        }

        public int EditInputString(int orgVoice, string talkText)
        {
            string s = talkText;

            ChangedVoiceNo = orgVoice;

            if ((s = CheckVoiceTag(s)) == talkText)
            {
                s = CheckVoiceTag2(s);
            }

            if (!VriEng)
            {
                s = ReplaceString(s);
            }
            else
            {
                Judge = JudgeStringNoJapanese(s);
                if (Judge)
                {
                    if (!VriNoRep)
                    {
                        s = ReplaceString(s);
                    }
                }
                else
                {
                    s = ReplaceString(s);
                }
            }

            s = CutString(s);
            ChangedTalkText = s;

            return ChangedVoiceNo;
        }

        private string CheckVoiceTag(string talkText)
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
        private string CheckVoiceTag2(string talkText)
        {
            string s = talkText;
            var r = VoiceCommand1.Match(s);

            if (r.Success)
            {
                string[] s2 = r.Value.Substring(1, r.Value.Length - 2).Split(SepChars);
                s = s.Substring(r.Index + r.Length);

                if (int.TryParse(s2[1], out int no))
                {
                    if ((no > 0) && (9 > no))
                    {
                        ChangedVoiceNo = no;
                    }
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

        private bool JudgeStringNoJapanese(string talkText)
        {

            string s1 = Vrir1.Replace(talkText, "");
            MatchCollection s2 = Vrir2.Matches(s1);

            float le = s2.Count;
            float ri = s1.Length;

            return ( le / ri) > 0.75;
        }
    }
}
