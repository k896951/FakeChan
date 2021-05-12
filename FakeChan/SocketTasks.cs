using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FakeChan
{
    public class SocketTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;
        UserDefData UserData;
        EditParamsBefore EditInputText = new EditParamsBefore();
        Random r = new Random(Environment.TickCount);

        bool KeepListen = false;
        TcpListener TcpIpListener;
        Action BGTcpListen;
        int ListenPort;

        public delegate void CallEventHandlerCallAsyncTalk(MessageData talk);
        public event CallEventHandlerCallAsyncTalk OnCallAsyncTalk;

        public Methods PlayMethod { get; set; }

        public SocketTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref UserDefData UsrData)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            UserData = UsrData;
        }

        public void StartSocketTasks(IPAddress addr, int port)
        {
            PlayMethod = Methods.sync;

            // TCP/IP リスナタスク起動
            TcpIpListener = new TcpListener(addr, port);
            TcpIpListener.Start();
            KeepListen = true;
            BGTcpListen = SetupBGTcpListenerTask();
            Task.Run(BGTcpListen);
            ListenPort = port;
        }

        public void StopSocketTasks()
        {
            // TCP/IP リスナタスク停止
            KeepListen = false;
            TcpIpListener?.Stop();
        }

        private Action SetupBGTcpListenerTask()
        {
            // パケット（プログラムではiSpeed～iVolumeをスキップ）
            //   Int16  iCommand = 0x0001;
            //   Int16  iSpeed = -1;
            //   Int16  iTone = -1;
            //   Int16  iVolume = -1;
            //   Int16  iVoice = 1;
            //   byte   bCode = 0;
            //   Int32  iLength = bMessage.Length;
            //   byte[] bMessage;

            Action BGTcpListen = (() => {

                while (KeepListen) // とりあえずの待ち受け構造
                {
                    try
                    {
                        TcpClient client = TcpIpListener.AcceptTcpClient();
                        Int16 iCommand;
                        Int16 iVoice;
                        int voice;
                        int ListenIf;
                        byte bCode;
                        Int32 iLength;
                        string TalkText = "";

                        byte[] iCommandBuff;
                        byte[] iVoiceBuff;
                        byte[] iLengthBuff;

                        List<int> CidList = Config.AvatorNames.Select(c => c.Key).ToList();
                        int cnt = CidList.Count;
                        int cid;

                        using (NetworkStream ns = client.GetStream())
                        {
                            using (BinaryReader br = new BinaryReader(ns))
                            {
                                iCommandBuff = br.ReadBytes(2);
                                iCommand = BitConverter.ToInt16(iCommandBuff, 0); // コマンド

                                switch(iCommand)
                                {
                                    case 0x0001: // 読み上げ指示

                                        br.ReadBytes(2 * 3); // スキップ

                                        iVoiceBuff = br.ReadBytes(2);
                                        iVoice = BitConverter.ToInt16(iVoiceBuff, 0); // 音声

                                        bCode = br.ReadByte(); // 文字列エンコーディング

                                        iLengthBuff = br.ReadBytes(4);
                                        iLength = BitConverter.ToInt32(iLengthBuff, 0); // 文字列サイズ

                                        byte[] TalkTextBuff = new byte[iLength];

                                        TalkTextBuff = br.ReadBytes(iLength);  // 文字列データ

                                        switch (bCode)
                                        {
                                            case 0: // UTF8
                                                TalkText = System.Text.Encoding.UTF8.GetString(TalkTextBuff, 0, iLength);
                                                break;

                                            case 2: // CP932
                                                TalkText = System.Text.Encoding.GetEncoding(932).GetString(TalkTextBuff, 0, iLength);
                                                break;

                                            case 1: // 暫定で書いた
                                                TalkText = System.Text.Encoding.Unicode.GetString(TalkTextBuff, 0, iLength);
                                                break;
                                        }

                                        if (ListenPort == Config.SocketPortNum2)
                                        {
                                            ListenIf = (int)ListenInterface.Socket2;
                                        }
                                        else
                                        {
                                            ListenIf = (int)ListenInterface.Socket1;
                                        }

                                        voice = EditInputText.EditInputString((iVoice > 8 || iVoice == -1 ? 0 : iVoice), TalkText);
                                        cid = UserData.SelectedCid[ListenIf][voice];
                                        switch (UserData.RandomVoiceMethod[ListenIf])
                                        {
                                            case 1:
                                                voice = r.Next(0, 9);
                                                cid = UserData.SelectedCid[ListenIf][voice];
                                                break;

                                            case 2:
                                                cid = CidList[r.Next(0, cnt)];
                                                if (Config.AvatorNames.ContainsKey(cid))
                                                {
                                                    UserData.VoiceParams[ListenIf][voice][cid] = Config.AvatorParams(cid);
                                                }
                                                break;
                                        }

                                        Dictionary<string, decimal> Effects = UserData.VoiceParams[ListenIf][voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                                        Dictionary<string, decimal> Emotions = UserData.VoiceParams[ListenIf][voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

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

                                        break;

                                    case 0x0040: // 読み上げキャンセル
                                        MessQue.ClearQueue();
                                        break;

                                    case 0x0110: // 一時停止状態の取得
                                        byte data1 = 0;
                                        using (BinaryWriter bw = new BinaryWriter(ns))
                                        {
                                            bw.Write(data1);
                                        }
                                        break;

                                    case 0x0120: // 音声再生状態の取得
                                        byte data2 = MessQue.count == 0 ? (byte)0 : (byte)1;
                                        using (BinaryWriter bw = new BinaryWriter(ns))
                                        {
                                            bw.Write(data2);
                                        }
                                        break;

                                    case 0x0130: // 残りタスク数の取得
                                        byte[] data3 = BitConverter.GetBytes(MessQue.count);
                                        using (BinaryWriter bw = new BinaryWriter(ns))
                                        {
                                            bw.Write(data3);
                                        }
                                        break;

                                    case 0x0010: // 一時停止
                                    case 0x0020: // 一時停止の解除
                                    case 0x0030: // 現在の行をスキップし次の行へ
                                    default:
                                        break;
                                }

                            }
                        }

                    }
                    catch (Exception)
                    {
                        //Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show(e.Message, "sorry3");
                        //});
                    }
                }
            });

            return BGTcpListen;
        }

    }
}
