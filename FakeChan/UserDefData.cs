using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FakeChan
{
    [DataContract]
    public class UserDefData
    {
        [DataMember] public Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> ParamAssignList;
        [DataMember] public Dictionary<int, int> MethodAssignList;
        [DataMember] public Dictionary<int, int> Voice2Cid;
        [DataMember] public Dictionary<VoiceIndex, bool> LampSwitch;
        [DataMember] public int TextLength;
        [DataMember] public bool AddSuffix;
        [DataMember] public string AddSuffixStr;

        [DataMember] public bool IsRandomVoice;

        [DataMember] public string MatchPattern1;
        [DataMember] public string MatchPattern2;
        [DataMember] public string MatchPattern3;
        [DataMember] public string MatchPattern4;
        [DataMember] public string MatchPattern5;
        [DataMember] public string MatchPattern6;
        [DataMember] public string MatchPattern7;

        [DataMember] public string ReplcaeStr1;
        [DataMember] public string ReplcaeStr2;
        [DataMember] public string ReplcaeStr3;
        [DataMember] public string ReplcaeStr4;
        [DataMember] public string ReplcaeStr5;
        [DataMember] public string ReplcaeStr6;
        [DataMember] public string ReplcaeStr7;

        [DataMember] public bool IsUseReplcae1;
        [DataMember] public bool IsUseReplcae2;
        [DataMember] public bool IsUseReplcae3;
        [DataMember] public bool IsUseReplcae4;
        [DataMember] public bool IsUseReplcae5;
        [DataMember] public bool IsUseReplcae6;
        [DataMember] public bool IsUseReplcae7;
    }
}
