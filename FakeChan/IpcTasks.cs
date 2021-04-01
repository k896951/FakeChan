using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class IpcTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;
        Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> ParamAssignList;
        FNF.Utility.BouyomiChanRemoting ShareIpcObject;
        IpcServerChannel IpcCh;

        public methods PlayMethod { get; set; }

        public IpcTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> Params)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            ParamAssignList = Params;
            PlayMethod = methods.sync;

            ShareIpcObject = new FNF.Utility.BouyomiChanRemoting();
            ShareIpcObject.OnAddTalkTask01 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask01(IPCAddTalkTask01);
            ShareIpcObject.OnAddTalkTask02 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask02(IPCAddTalkTask02);
            ShareIpcObject.OnAddTalkTask03 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask03(IPCAddTalkTask03);
            ShareIpcObject.OnAddTalkTask21 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask21(IPCAddTalkTask21);
            ShareIpcObject.OnAddTalkTask23 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask23(IPCAddTalkTask23);
            ShareIpcObject.OnClearTalkTask += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerSimpleTask(IPCClearTalkTask);
            ShareIpcObject.OnSkipTalkTask  += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerSimpleTask(IPCSkipTalkTask);
            ShareIpcObject.MessQue = MessQue;
        }

        public void StartIpcTasks()
        {
            try
            {
                IpcCh = new IpcServerChannel("BouyomiChan");
                ChannelServices.RegisterChannel(IpcCh, false);
            }
            catch (Exception)
            {
                //
            }
            IpcCh.IsSecured = false;
            RemotingServices.Marshal(ShareIpcObject, "Remoting", typeof(FNF.Utility.BouyomiChanRemoting));
        }

        public void StopIpcTasks()
        {
            //ChannelServices.UnregisterChannel(IpcCh);
            RemotingServices.Disconnect(ShareIpcObject);
        }

        public void SetTaskId(int Id)
        {
            ShareIpcObject.taskId = Id;
        }

        private void IPCAddTalkTask01(string TalkText)
        {
            int cid = Config.B2Amap[0];
            int tid = MessQue.count + 1;
            Dictionary<string, decimal> Effects = ParamAssignList[0][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = ParamAssignList[0][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

            switch (PlayMethod)
            {
                case methods.sync:
                    MessageData talk = new MessageData()
                    {
                        Cid = cid,
                        Message = TalkText,
                        BouyomiVoice = 0,
                        TaskId = tid,
                        Effects = Effects,
                        Emotions = Emotions
                    };
                    MessQue.AddQueue(talk);
                    break;

                case methods.async:
                    WcfClient.TalkAsync(cid, TalkText, Effects, Emotions);
                    break;
            }
        }

        private void IPCAddTalkTask02(string TalkText, int iSpeed, int iVolume, int vType)
        {
            int voice = vType > 8 ? 0 : vType;
            int cid = Config.B2Amap[voice];
            int tid = MessQue.count + 1;
            Dictionary<string, decimal> Effects = ParamAssignList[voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = ParamAssignList[voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

            switch (PlayMethod)
            {
                case methods.sync:
                    MessageData talk = new MessageData()
                    {
                        Cid = cid,
                        Message = TalkText,
                        BouyomiVoice = voice,
                        TaskId = tid,
                        Effects = Effects,
                        Emotions = Emotions
                    };
                    MessQue.AddQueue(talk);
                    break;

                case methods.async:
                    WcfClient.TalkAsync(cid, TalkText, Effects, Emotions);
                    break;
            }
        }

        private void IPCAddTalkTask03(string TalkText, int iSpeed, int iTone, int iVolume, int vType)
        {
            IPCAddTalkTask02(TalkText, iSpeed, iVolume, vType);
        }

        private int IPCAddTalkTask21(string TalkText)
        {
            IPCAddTalkTask01(TalkText);
            return 0;
        }

        private int IPCAddTalkTask23(string TalkText, int iSpeed, int iTone, int iVolume, int vType)
        {
            IPCAddTalkTask02(TalkText, iSpeed, iVolume, vType);
            return 0;
        }

        private void IPCClearTalkTask()
        {
            MessQue.ClearQueue();
        }

        private void IPCSkipTalkTask()
        {
        }

    }
}
