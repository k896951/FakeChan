using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class EditParamsBefore
    {
        public int ChangedVoiceNo { get; private set; }

        public string ChangedTalkText {
            get { return sb.ToString(); }
            private set { }
        }

        private StringBuilder sb = new StringBuilder();

        public int CheckVoiceChange(int orgVoice, string talkText)
        {
            sb.Clear();

            ChangedVoiceNo = orgVoice;
            CheckSpeedTag(talkText);

            return ChangedVoiceNo;
        }

        private void CheckSpeedTag(string talkText)
        {
            string s = talkText.Substring(0, 2);
            sb.Append(talkText.Substring(2));

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
                    sb.Clear();
                    sb.Append(talkText);
                    break;
            }
        }
    }
}
