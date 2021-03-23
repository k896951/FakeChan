using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    class Configs
    {
        Dictionary<int, string> AvatorNameList;
        Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>> AvatorParamList;
        Dictionary<int, int> Bouyomi2AssistantSeika;

        IPListenPoint ListenEndPoint = new IPListenPoint()
        {
            Address = IPAddress.Parse("127.0.0.1"),
            PortNum = 50001
        };

        Dictionary<int, string> BouyomiVoiceList = new Dictionary<int, string>()
        {
            { 0, "ボイス0" },
            { 1, "女性1"},
            { 2, "女性2"},
            { 3, "男性1"},
            { 4, "男性2"},
            { 5, "中性" },
            { 6, "ロボット" },
            { 7, "機械1" },
            { 8, "機械2" }
        };

        public IPListenPoint EndPoint
        {
            get
            {
                return ListenEndPoint;
            }
            set
            {
                ListenEndPoint = value;
            }
        }

        public Dictionary<int, string> AvatorNames
        {
            get
            {
                return AvatorNameList;
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


        public Configs()
        {
            WCFClient api = new WCFClient();
            AvatorNameList = api.AvatorList2().ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
            AvatorParamList = AvatorNameList.ToDictionary(k => k.Key, v => api.GetDefaultParams2(v.Key));

            int cid = AvatorNameList.First().Key;
            Bouyomi2AssistantSeika = BouyomiVoiceList.ToDictionary(k => k.Key, v => cid);
        }
    }
}
