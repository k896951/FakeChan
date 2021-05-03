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

        public string ChangedTalkText {
            get { return sb1.ToString(); }
            private set { }
        }

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

        private StringBuilder sb1 = new StringBuilder();

        public int EditString(int orgVoice, string talkText)
        {
            ChangedVoiceNo = orgVoice;

            sb1.Clear();
            CheckSpeedTag(talkText);

            string text2 = sb1.ToString();
            if (IsUseReplcae1)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern1, ReplcaeStr1);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae2)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern2, ReplcaeStr2);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae3)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern3, ReplcaeStr3);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae4)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern4, ReplcaeStr4);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae5)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern5, ReplcaeStr5);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae6)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern6, ReplcaeStr6);
                }
                catch (Exception)
                {
                }
            }
            if (IsUseReplcae7)
            {
                try
                {
                    text2 = Regex.Replace(text2, MatchPattern7, ReplcaeStr7);
                }
                catch (Exception)
                {
                }
            }

            sb1.Clear();
            sb1.Append(text2);

            if (sb1.Length > LimitTextLength)
            {
                sb1.Remove(LimitTextLength, sb1.Length - LimitTextLength);

                if (IsUseSuffixString)
                {
                    sb1.Append(SuffixString);
                }
            }

            return ChangedVoiceNo;
        }

        private void CheckSpeedTag(string talkText)
        {
            string s = talkText;

            if (sb1.Length > 2)
            {
                s = talkText.Substring(0, 2);
                sb1.Append(talkText.Substring(2));
                switch (s)
                {
                    case "y)": ChangedVoiceNo = 1; break;
                    case "b)": ChangedVoiceNo = 2; break;
                    case "h)": ChangedVoiceNo = 3; break;
                    case "d)": ChangedVoiceNo = 4; break;
                    case "a)": ChangedVoiceNo = 5; break;
                    case "r)": ChangedVoiceNo = 6; break;
                    case "t)": ChangedVoiceNo = 7; break;
                    case "g)": ChangedVoiceNo = 8; break;
                    default:
                        sb1.Clear();
                        sb1.Append(talkText);
                        break;
                }
            }
            else
            {
                sb1.Append(talkText);
            }
        }
    }
}
