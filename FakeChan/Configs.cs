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
        WCFClient api = null;

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
        public string IPCChannelName { get; set; }

        public int TextLength { get; set; } = 20;
        public bool AddSuffix { get; set; } = false;
        public string AddSuffixStr { get; set; } = "(以下略";
        public bool IsRandomVoice { get; set; } = false;

        public readonly int BouyomiVoiceWidth = 9;

        public Dictionary<VoiceIndex, int> BouyomiVoiceIdx = new Dictionary<VoiceIndex, int>()
        {
            { VoiceIndex.IPC1,     0 },
            { VoiceIndex.Socket1,  9 },
            { VoiceIndex.Http1,   18 },
            { VoiceIndex.Socket2, 27 },
            { VoiceIndex.Http2,   36 }
        };

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
            {  0, "IPC(BouyomiChan):ボイス0" },
            {  1, "IPC(BouyomiChan):女性1"},
            {  2, "IPC(BouyomiChan):女性2"},
            {  3, "IPC(BouyomiChan):男性1"},
            {  4, "IPC(BouyomiChan):男性2"},
            {  5, "IPC(BouyomiChan):中性" },
            {  6, "IPC(BouyomiChan):ロボット" },
            {  7, "IPC(BouyomiChan):機械1" },
            {  8, "IPC(BouyomiChan):機械2" },
            {  9, "Socket(50001):ボイス0" },
            { 10, "Socket(50001):女性1"},
            { 11, "Socket(50001):女性2"},
            { 12, "Socket(50001):男性1"},
            { 13, "Socket(50001):男性2"},
            { 14, "Socket(50001):中性" },
            { 15, "Socket(50001):ロボット" },
            { 16, "Socket(50001):機械1" },
            { 17, "Socket(50001):機械2" },
            { 18, "HTTP(50080):ボイス0" },
            { 19, "HTTP(50080):女性1"},
            { 20, "HTTP(50080):女性2"},
            { 21, "HTTP(50080):男性1"},
            { 22, "HTTP(50080):男性2"},
            { 23, "HTTP(50080):中性" },
            { 24, "HTTP(50080):ロボット" },
            { 25, "HTTP(50080):機械1" },
            { 26, "HTTP(50080):機械2" },
            { 27, "Socket(50002):ボイス0" },
            { 28, "Socket(50002):女性1"},
            { 29, "Socket(50002):女性2"},
            { 30, "Socket(50002):男性1"},
            { 31, "Socket(50002):男性2"},
            { 32, "Socket(50002):中性" },
            { 33, "Socket(50002):ロボット" },
            { 34, "Socket(50002):機械1" },
            { 35, "Socket(50002):機械2" },
            { 36, "HTTP(50081):ボイス0" },
            { 37, "HTTP(50081):女性1"},
            { 38, "HTTP(50081):女性2"},
            { 39, "HTTP(50081):男性1"},
            { 40, "HTTP(50081):男性2"},
            { 41, "HTTP(50081):中性" },
            { 42, "HTTP(50081):ロボット" },
            { 43, "HTTP(50081):機械1" },
            { 44, "HTTP(50081):機械2" }
        };

        Dictionary<int, string> PlayMethodList = new Dictionary<int, string>()
        {
            { 0, "同期(共通キューに入れて順次発声)" },
            { 1, "非同期(すぐに発声)"}
        };

        Dictionary<int, Methods> PlayMethodMap = new Dictionary<int, Methods>()
        {
            { 0, Methods.sync },
            { 1, Methods.async}
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
        public Dictionary<int, Methods> PlayMethodsMap
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

        public Configs(ref WCFClient wcf)
        {
            api = wcf;
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

            IPCChannelName  = "BouyomiChan";

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
