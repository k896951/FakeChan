using System;
using System.Collections.Generic;
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

        public static string MatchPattern1 { get; set; }
        public static string MatchPattern2 { get; set; }
        public static string MatchPattern3 { get; set; }
        public static string MatchPattern4 { get; set; }
        public static string MatchPattern5 { get; set; }
        public static string MatchPattern6 { get; set; }
        public static string MatchPattern7 { get; set; }

        public static string ReplcaeStr1 { get; set; }
        public static string ReplcaeStr2 { get; set; }
        public static string ReplcaeStr3 { get; set; }
        public static string ReplcaeStr4 { get; set; }
        public static string ReplcaeStr5 { get; set; }
        public static string ReplcaeStr6 { get; set; }
        public static string ReplcaeStr7 { get; set; }

        public static bool IsUseReplcae1 { get; set; }
        public static bool IsUseReplcae2 { get; set; }
        public static bool IsUseReplcae3 { get; set; }
        public static bool IsUseReplcae4 { get; set; }
        public static bool IsUseReplcae5 { get; set; }
        public static bool IsUseReplcae6 { get; set; }
        public static bool IsUseReplcae7 { get; set; }

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
                if (IsUseReplcae1) try { s = Regex.Replace(s, MatchPattern1, ReplcaeStr1); } catch (Exception) { }
                if (IsUseReplcae2) try { s = Regex.Replace(s, MatchPattern2, ReplcaeStr2); } catch (Exception) { }
                if (IsUseReplcae3) try { s = Regex.Replace(s, MatchPattern3, ReplcaeStr3); } catch (Exception) { }
                if (IsUseReplcae4) try { s = Regex.Replace(s, MatchPattern4, ReplcaeStr4); } catch (Exception) { }
                if (IsUseReplcae5) try { s = Regex.Replace(s, MatchPattern5, ReplcaeStr5); } catch (Exception) { }
                if (IsUseReplcae6) try { s = Regex.Replace(s, MatchPattern6, ReplcaeStr6); } catch (Exception) { }
                if (IsUseReplcae7) try { s = Regex.Replace(s, MatchPattern7, ReplcaeStr7); } catch (Exception) { }
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
