using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class Configs
    {
        WCFClient api = null;

        Dictionary<int, string> AvatorNameList;
        Dictionary<int, string> AvatorNameList2;
        Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>> AvatorParamList;
        string QuietMessageFilename = "QuietMessages.json";

        public IPAddress SocketAddress { get; set; }
        public Int32 SocketPortNum { get; set; }
        public IPAddress HttpAddress { get; set; }
        public Int32 HttpPortNum { get; set; }
        public IPAddress SocketAddress2 { get; set; }
        public Int32 SocketPortNum2 { get; set; }
        public IPAddress HttpAddress2 { get; set; }
        public Int32 HttpPortNum2 { get; set; }
        public string IPCChannelName { get; set; }

        public Dictionary<int, string> AvatorNames
        {
            get
            {
                return AvatorNameList;
            }
        }

        public Configs(ref WCFClient wcf)
        {
            api = wcf;
            //AvatorNameList = api.AvatorList2().ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
            AvatorNameList = api.AvatorList2().Where(v => (v.Value["prod"] != "SAPI") || ((v.Value["prod"] == "SAPI") && (!v.Value["name"].StartsWith("CeVIO-")))).ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
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

        public Dictionary<int, List<string>> MessageLoader()
        {
            Dictionary<int, List<string>> mess = new Dictionary<int, List<string>>();
            string filepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString() + @"\" + QuietMessageFilename;
            if (File.Exists(filepath))
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
                //settings.UseSimpleDictionaryFormat = true;

                using (Stream s = new FileStream(filepath, FileMode.Open))
                {
                    try
                    {
                        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Dictionary<int, List<string>>), settings);
                        mess = js.ReadObject(s) as Dictionary<int, List<string>>;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("{0}", e.Message));
                    }
                }
            }
            else
            {
                // throw new Exception(string.Format("{0} not found", QuietMessageFilename));
            }

            return mess;
        }

    }
}
