using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace FakeChan
{
    public class WCFClient
    {
        IScAPIs ServiceAc;
        ChannelFactory<IScAPIs> ChannelAc;

        string BaseAddr = "net.pipe://localhost/EchoSeika/CentralGate/ApiEntry";

        public WCFClient()
        {
            ChannelAc = new ChannelFactory<IScAPIs>(new NetNamedPipeBinding(), new EndpointAddress(BaseAddr));
            ServiceAc = ChannelAc.CreateChannel();

            while (ChannelAc.State != CommunicationState.Opened)
            {
                Thread.Sleep(100);
            }
        }

        public string Version()
        {
            return ServiceAc.Verson();
        }

        public Dictionary<int, string> AvatorList()
        {
            return ServiceAc.AvatorList().ToDictionary(k => k.Key, v => v.Key + " : " + v.Value);
        }

        public Dictionary<int, Dictionary<string, string>> AvatorList2()
        {
            return ServiceAc.AvatorList2();
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetDefaultParams2(int cid)
        {
            return ServiceAc.GetDefaultParams2(cid);
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, decimal>>> GetCurrentParams2(int cid)
        {
            return ServiceAc.GetCurrentParams2(cid);
        }

        public double Talk(int cid, string talktext, string filepath, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions)
        {
            return ServiceAc.Talk(cid, talktext, filepath, effects, emotions);
        }
        public void TalkAsync(int cid, string talktext, Dictionary<string, decimal> effects, Dictionary<string, decimal> emotions)
        {
            ServiceAc.TalkAsync(cid, talktext, effects, emotions);
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
