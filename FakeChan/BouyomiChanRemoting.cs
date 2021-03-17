using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace FNF.Utility
{
    class BouyomiChanRemoting : MarshalByRefObject
    {
        public delegate void CallEventHandlerAddTalkTask01(string sTalkText);
        public delegate void CallEventHandlerAddTalkTask02(string sTalkText, int iSpeed, int iVolume, int vType);
        public delegate void CallEventHandlerAddTalkTask03(string sTalkText, int iSpeed, int iTone, int iVolume, int vType);
        public delegate int  CallEventHandlerAddTalkTask21(string sTalkText);
        public delegate int  CallEventHandlerAddTalkTask23(string sTalkText, int iSpeed, int iTone, int iVolume, int vType);
        public delegate void CallEventHandlerSimpleTask();

        public event CallEventHandlerAddTalkTask01 OnAddTalkTask01;
        public event CallEventHandlerAddTalkTask02 OnAddTalkTask02;
        public event CallEventHandlerAddTalkTask03 OnAddTalkTask03;
        public event CallEventHandlerAddTalkTask21 OnAddTalkTask21;
        public event CallEventHandlerAddTalkTask23 OnAddTalkTask23;
        public event CallEventHandlerSimpleTask OnClearTalkTask;
        public event CallEventHandlerSimpleTask OnSkipTalkTask;

        public BlockingCollection<FakeChan.MessageData> MessQue;

        public int TalkTaskCount { get { return MessQue.Count; } }
        public int NowTaskId { get { return 0; } }
        public bool NowPlaying { get { return MessQue.Count != 0; } }
        public bool Pause { get; set; }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void AddTalkTask(string sTalkText)
        {
            if (OnAddTalkTask01 != null) OnAddTalkTask01(sTalkText);
        }

        public void AddTalkTask(string sTalkText, int iSpeed, int iVolume, int vType)
        {
            if (OnAddTalkTask02 != null) OnAddTalkTask02(sTalkText, iSpeed, iVolume, vType);
        }

        public void AddTalkTask(string sTalkText, int iSpeed, int iTone, int iVolume, int vType)
        {
            if (OnAddTalkTask03 != null) OnAddTalkTask03(sTalkText, iSpeed, iTone, iVolume, vType);
        }

        public int AddTalkTask2(string sTalkText)
        {
            if (OnAddTalkTask21 != null) OnAddTalkTask21(sTalkText);
            return 0;
        }

        public int AddTalkTask2(string sTalkText, int iSpeed, int iTone, int iVolume, int vType)
        {
            if (OnAddTalkTask23 != null) OnAddTalkTask23(sTalkText, iSpeed, iTone, iVolume, vType);
            return 0;
        }

        public void ClearTalkTasks()
        {
            if (OnClearTalkTask != null) OnClearTalkTask();
        }

        public void SkipTalkTask()
        {
            if (OnSkipTalkTask != null) OnSkipTalkTask();
        }

    }

}
