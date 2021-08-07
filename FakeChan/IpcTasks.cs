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
        UserDefData UserData;
        FNF.Utility.BouyomiChanRemoting ShareIpcObject;
        IpcServerChannel IpcCh = null;
        EditParams EditInputText = new EditParams();
        string ChannelName;
        Random r = new Random(Environment.TickCount);

        public delegate void CallEventHandlerCallAsyncTalk(MessageData talk);
        public event CallEventHandlerCallAsyncTalk OnCallAsyncTalk;

        public Methods PlayMethod { get; set; }

        public IpcTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref UserDefData UsrData)
        {
            Config = cfg;
            MessQue = mq;
            UserData = UsrData;
            WcfClient = wcf;
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
            int cid;
            List<int> CidList = Config.AvatorNames.Select(c => c.Key).ToList();
            int ListenIf = (int)ListenInterface.IPC1;
            int voice = EditInputText.EditInputString((vType > 8 || vType == -1 ? 0 : vType), TalkText);

            cid = UserData.SelectedCid[ListenIf][voice];
            switch (UserData.RandomVoiceMethod[ListenIf])
            {
                case 1:
                    voice = r.Next(0, 9);
                    cid = UserData.SelectedCid[ListenIf][voice];
                    break;

                case 2:
                    cid = CidList[r.Next(0, CidList.Count)];
                    break;
            }

            Dictionary<string, decimal> Effects;
            Dictionary<string, decimal> Emotions;
            if (EditInputText.Judge)
            {
                cid = UserData.VriAvator;
                Effects = UserData.VriAvators[cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                Emotions = UserData.VriAvators[cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);
            }
            else
            {
                Effects = UserData.VoiceParams[ListenIf][voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                Emotions = UserData.VoiceParams[ListenIf][voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);
            }

            MessageData talk = new MessageData()
            {
                Cid = cid,
                Message = EditInputText.ChangedTalkText,
                BouyomiVoice = voice,
                ListenInterface = ListenIf,
                TaskId = MessQue.count + 1,
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
