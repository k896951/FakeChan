using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;
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

        public bool StartIpcTasks()
        {
            try
            {
                if (IpcCh is null)
                {
                    IpcCh = new IpcServerChannel("BouyomiChan");
                }
                IpcCh.IsSecured = false;
                ChannelServices.RegisterChannel(IpcCh, false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "IPC error1");
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
            int voice = vType > 8 ? 0 : vType;
            int cid = Config.B2Amap[voice];
            int tid = MessQue.count + 1;
            Dictionary<string, decimal> Effects = ParamAssignList[voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = ParamAssignList[voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

            // 製品により「普通」が異なるので制御が難しい。止めておく。
            //
            //decimal minval_spd = ParamAssignList[voice][cid]["effect"]["speed"]["min"];
            //decimal maxval_spd = ParamAssignList[voice][cid]["effect"]["speed"]["max"];
            //decimal minval_pch = ParamAssignList[voice][cid]["effect"]["pitch"]["min"];
            //decimal maxval_pch = ParamAssignList[voice][cid]["effect"]["pitch"]["max"];
            //decimal minval_vol = ParamAssignList[voice][cid]["effect"]["volume"]["min"];
            //decimal maxval_vol = ParamAssignList[voice][cid]["effect"]["volume"]["max"];
            //
            //iSpeed  = iSpeed  == -1 ? 100 : iSpeed;
            //iTone   = iTone   == -1 ? 100 : iTone;
            //iVolume = iVolume == -1 ?  50 : iVolume;
            //
            //Effects["speed"]  = Bouyomi2Othor(iSpeed,  300, minval_spd, maxval_spd);
            //Effects["pitch"]  = Bouyomi2Othor(iTone,   200, minval_pch, maxval_pch);
            //Effects["volume"] = Bouyomi2Othor(iVolume, 100, minval_vol, maxval_vol);

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

        private decimal Bouyomi2Othor(int BoValue, int BoMaxValue, decimal OtherMinValue, decimal OtherMaxValue)
        {
            decimal otherdisp = OtherMinValue < 0.0m ? decimal.Negate(OtherMinValue) : 0.0m;
            decimal rate = (decimal)BoValue / BoMaxValue;

            return rate * (OtherMaxValue + otherdisp) - otherdisp;
        }

    }
}
