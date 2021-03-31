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
        public IPAddress SocketAddress { get; set; }
        public Int32 SocketPortNum { get; set; }
        public IPAddress HttpAddress { get; set; }
        public Int32 HttpPortNum { get; set; }
        public IPAddress SocketAddress2 { get; set; }
        public Int32 SocketPortNum2 { get; set; }
        public IPAddress HttpAddress2 { get; set; }
        public Int32 HttpPortNum2 { get; set; }

        Dictionary<int, string> BouyomiVoiceListHttp = new Dictionary<int, string>()
        {
            {  0, "ボイス0" },
            {  1, "女性1"},
            {  2, "女性2"},
            {  3, "男性1"},
            {  4, "男性2"},
            {  5, "中性" },
            {  6, "ロボット" },
            {  7, "機械1" },
            {  8, "機械2" }
        };

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
            { 17, "Socket:機械2" },
            { 18, "HTTP:ボイス0" },
            { 19, "HTTP:女性1"},
            { 20, "HTTP:女性2"},
            { 21, "HTTP:男性1"},
            { 22, "HTTP:男性2"},
            { 23, "HTTP:中性" },
            { 24, "HTTP:ロボット" },
            { 25, "HTTP:機械1" },
            { 26, "HTTP:機械2" }
        };

        Dictionary<int, string> PlayMethodList = new Dictionary<int, string>()
        {
            { 0,  "同期" },
            { 1, "非同期"}
        };

        Dictionary<int, methods> PlayMethodMap = new Dictionary<int, methods>()
        {
            { 0, methods.sync },
            { 1, methods.async}
        };

        public Dictionary<int, string> AvatorNames
        {
            get
            {
                return AvatorNameList;
            }
        }

        public Dictionary<int, string> PlayMethods
        {
            get
            {
                return PlayMethodList;
            }
        }
        public Dictionary<int, methods> PlayMethodsMap
        {
            get
            {
                return PlayMethodMap;
            }
        }

        public Dictionary<int, string> BouyomiVoices
        {
            get
            {
                return BouyomiVoiceList;
            }
        }

        public Dictionary<int, string> BouyomiVoicesHttp
        {
            get
            {
                return BouyomiVoiceListHttp;
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

            SocketAddress = IPAddress.Parse("127.0.0.1");
            SocketPortNum = 50001;

            HttpAddress = IPAddress.Parse("127.0.0.1");
            HttpPortNum = 50080;

            SocketAddress2 = IPAddress.Parse("127.0.0.1");
            SocketPortNum2 = 50002;

            HttpAddress2 = IPAddress.Parse("127.0.0.1");
            HttpPortNum2 = 50081;

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
