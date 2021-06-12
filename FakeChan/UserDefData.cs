using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FakeChan
{
    [DataContract]
    public class UserDefData
    {
        [DataMember] public Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>>> VoiceParams;
        [DataMember] public Dictionary<int, Dictionary<int, int>> SelectedCid;
        [DataMember] public Dictionary<int, int> SelectedCallMethod;
        [DataMember] public Dictionary<int, bool> InterfaceSwitch;
        [DataMember] public Dictionary<int, List<string>> QuietMessages;

        [DataMember] public Dictionary<int, int> RandomVoiceMethod;

        //[DataMember] public bool IsRandomVoice;
        //[DataMember] public bool IsRandomAvator;
        [DataMember] public bool IsSilentAvator;
        [DataMember] public int TextLength;
        [DataMember] public bool AddSuffix;
        [DataMember] public string AddSuffixStr;

        [DataMember] public List<ReplaceDefinition> ReplaceDefs;

        [DataMember] public string FakeChanAppName;

    }
}
