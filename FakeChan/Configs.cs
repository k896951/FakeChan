using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class Configs
    {
        Dictionary<int, string> AvatorNameList;
        Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>> AvatorParamList;
        Dictionary<int, int> Bouyomi2AssistantSeika;
        public IPAddress Address { get; set; }
        public Int32 PortNum { get; set; }

        Dictionary<int, string> BouyomiVoiceList = new Dictionary<int, string>()
        {
            {  0, "IPC:ボイス0" },
            {  1, "IPC:女性1"},
            {  2, "IPC:女性2"},
            {  3, "IPC:男性1"},
            {  4, "IPC:男性2"},
            {  5, "IPC:中性" },
            {  6, "IPC:ロボット" },
            {  7, "IPC:機械1" },
            {  8, "IPC:機械2" },
            {  9, "Socket:ボイス0" },
            { 10, "Socket:女性1"},
            { 11, "Socket:女性2"},
            { 12, "Socket:男性1"},
            { 13, "Socket:男性2"},
            { 14, "Socket:中性" },
            { 15, "Socket:ロボット" },
            { 16, "Socket:機械1" },
            { 17, "Socket:機械2" }
        };

        Dictionary<methods, string> PlayMethodList = new Dictionary<methods, string>()
        {
            { methods.sync,  "同期" },
            { methods.async, "非同期"}
        };

        public Dictionary<int, string> AvatorNames
        {
            get
            {
                return AvatorNameList;
            }
        }

        public Dictionary<methods, string> PlayMethods
        {
            get
            {
                return PlayMethodList;
            }
        }

        public Dictionary<int, string> BouyomiVoices
        {
            get
            {
                return BouyomiVoiceList;
            }
        }

        public Dictionary<int, int> B2Amap
        {
            get
            {
                return Bouyomi2AssistantSeika;
            }

            set
            {
                Bouyomi2AssistantSeika = value;
            }
        }

        public Configs()
        {
            WCFClient api = new WCFClient();
            AvatorNameList = api.AvatorList2().ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
            AvatorParamList = AvatorNameList.ToDictionary(k => k.Key, v => api.GetDefaultParams2(v.Key));

            Address = IPAddress.Parse("127.0.0.1");
            PortNum = 50001;

            int cid = AvatorNameList.First().Key;
            Bouyomi2AssistantSeika = BouyomiVoiceList.ToDictionary(k => k.Key, v => cid);
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> AvatorParams(int cid)
        {
            if (!AvatorParamList.ContainsKey(cid))
            {
                throw new Exception("No Contains cid");
            }

            return AvatorParamList[cid];
        }

        public Dictionary<string, Dictionary<string, decimal>> AvatorEffectParams(int cid)
        {
            if (!AvatorParamList.ContainsKey(cid))
            {
                throw new Exception("No Contains cid");
            }

            return AvatorParamList[cid]["effect"];
        }

        public Dictionary<string, Dictionary<string, decimal>> AvatorEmotionParams(int cid)
        {
            if (!AvatorParamList.ContainsKey(cid))
            {
                throw new Exception("No Contains cid");
            }

            return AvatorParamList[cid]["emotion"];
        }

    }
}
