using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows;

namespace FakeChan
{
    public class IpcTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;
        Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> ParamAssignList;
        FNF.Utility.BouyomiChanRemoting ShareIpcObject;
        IpcServerChannel IpcCh = null;
        EditParamsBefore EditEffect = new EditParamsBefore();
        string ChannelName;

        public delegate void CallEventHandlerCallAsyncTalk(MessageData talk);
        public event CallEventHandlerCallAsyncTalk OnCallAsyncTalk;

        public Methods PlayMethod { get; set; }

        public IpcTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> Params)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            ParamAssignList = Params;
            PlayMethod = Methods.sync;

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

        public bool StartIpcTasks(string chName)
        {
            ChannelName = chName;
            try
            {
                if (IpcCh is null)
                {
                    IpcCh = new IpcServerChannel(ChannelName);
                }
                IpcCh.IsSecured = false;
                ChannelServices.RegisterChannel(IpcCh, false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "IPC Warning");
                return false;
            }
            RemotingServices.Marshal(ShareIpcObject, "Remoting", typeof(FNF.Utility.BouyomiChanRemoting));

            return true;
        }

        public bool StopIpcTasks()
        {
            if (IpcCh != null)
            {
                try
                {
                    RemotingServices.Disconnect(ShareIpcObject);
                    IpcCh.StopListening(null);
                    ChannelServices.UnregisterChannel(IpcCh);
                }
                catch (Exception)
                {
                    //MessageBox.Show(e.Message, "IPC error2");
                }
            }

            return true;
        }

        public void SetTaskId(int Id)
        {
            ShareIpcObject.taskId = Id;
        }

        private void IPCAddTalkTask01(string TalkText)
        {
            IPCAddTalkTask03(TalkText, -1, -1, -1, 0);
        }

        private void IPCAddTalkTask02(string TalkText, int iSpeed, int iVolume, int vType)
        {
            IPCAddTalkTask03(TalkText, iSpeed, -1, iVolume, vType);
        }

        private void IPCAddTalkTask03(string TalkText, int iSpeed, int iTone, int iVolume, int vType)
        {
            int voice = EditEffect.EditString((vType > 8 || vType == -1 ? 0 : vType), TalkText);
            int tid = MessQue.count + 1;
            int voiceIdx = Config.BouyomiVoiceIdx[VoiceIndex.IPC1] + voice;

            int cid = Config.B2Amap[voiceIdx];
            Dictionary<string, decimal> Effects = ParamAssignList[voiceIdx][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = ParamAssignList[voiceIdx][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

            MessageData talk = new MessageData()
            {
                Cid = cid,
                Message = EditEffect.ChangedTalkText,
                BouyomiVoice = voice,
                BouyomiVoiceIdx = voiceIdx,
                TaskId = tid,
                Effects = Effects,
                Emotions = Emotions
            };

            switch (PlayMethod)
            {
                case Methods.sync:
                    MessQue.AddQueue(talk);
                    break;

                case Methods.async:
                    OnCallAsyncTalk?.Invoke(talk);
                    break;
            }
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
