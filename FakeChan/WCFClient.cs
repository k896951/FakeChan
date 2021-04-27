using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace FakeChan
{
    public class WCFClient
    {
        NetNamedPipeBinding Binding = new NetNamedPipeBinding();
        TimeSpan keeptime = new TimeSpan(99, 99, 99);

        string BaseAddr = "net.pipe://localhost/EchoSeika/CentralGate/ApiEntry";

        public WCFClient()
        {
        }

        private ChannelFactory<IScAPIs> CreateChannelFactory()
        {
            var ans = new ChannelFactory<IScAPIs>(Binding, new EndpointAddress(BaseAddr));

            while (ans.State != CommunicationState.Created)
            {
                Thread.Sleep(10);
            }

            return ans;
        }

        private IScAPIs CreateChannel(ChannelFactory<IScAPIs> ChannelSc)
        {
            var ans = ChannelSc.CreateChannel();
            (ans as IContextChannel).OperationTimeout = keeptime;

            while (ChannelSc.State != CommunicationState.Opened)
            {
                Thread.Sleep(10);
            }

            return ans;
        }

        public string Version()
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.Verson();
            cf.Close();
            return ans;
        }

        public Dictionary<int, string> AvatorList()
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.AvatorList().ToDictionary(k => k.Key, v => v.Key + " : " + v.Value);
            cf.Close();
            return ans;
        }

        public Dictionary<int, Dictionary<string, string>> AvatorList2()
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.AvatorList2();
            cf.Close();
            return ans;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetDefaultParams2(int cid)
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.GetDefaultParams2(cid);
            cf.Close();
            return ans;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetCurrentParams2(int cid)
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.GetCurrentParams2(cid);
            cf.Close();
            return ans;
        }

        public double Talk(int cid, string talktext, string filepath, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions)
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            var ans = api.Talk(cid, talktext, "", effects, emotions);
            cf.Close();
            return ans;
        }

        public void TalkAsync(int cid, string talktext, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions)
        {
            var cf = CreateChannelFactory();
            var api = CreateChannel(cf);
            api.TalkAsync(cid, talktext, effects, emotions);
            cf.Close();
        }

    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IScAPIs
    {
        [OperationContract]
        string Verson();

        [OperationContract]
        Dictionary<int, string> AvatorList();

        [OperationContract]
        Dictionary<int, Dictionary<string, string>> AvatorList2();

        [OperationContract]
        Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetDefaultParams2(int cid);

        [OperationContract]
        Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetCurrentParams2(int cid);

        [OperationContract]
        double Talk(int cid, string talktext, string filepath, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions);

        [OperationContract]
        void TalkAsync(int cid, string talktext, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions);

        [OperationContract]
        double Save(int cid, string talktext, string filepath, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions);
    }

}
