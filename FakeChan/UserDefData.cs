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
    }
}
