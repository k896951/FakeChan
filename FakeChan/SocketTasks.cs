using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FakeChan
{
    public class SocketTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;

        Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> ParamAssignList;

        bool KeepListen = false;
        TcpListener TcpIpListener;
        Action BGTcpListen;
        int ListenPort;

        public methods PlayMethod { get; set; }

        public SocketTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> Params)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            ParamAssignList = Params;
        }

        public void StartSocketTasks(IPAddress addr, int port)
        {
            PlayMethod = methods.sync;

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
            // パケット（プログラムでは先頭4項目をスキップ）
            //   Int16  iCommand = 0x0001;
            //   Int16  iSpeed = -1;
            //   Int16  iTone = -1;
            //   Int16  iVolume = -1;
            //   Int16  iVoice = 1;
            //   byte   bCode = 0;
            //   Int32  iLength = bMessage.Length;
            //   byte[] bMessage;

            Action BGTcpListen = (() => {

                int SkipSize = 2 * 4;

                while (KeepListen) // とりあえずの待ち受け構造
                {
                    try
                    {
                        TcpClient client = TcpIpListener.AcceptTcpClient();
                        Int16 iVoice;
                        byte bCode;
                        Int32 iLength;
                        string TalkText = "";

                        byte[] iLengthBuff;
                        byte[] iVoiceBuff;

                        using (NetworkStream ns = client.GetStream())
                        {
                            using (BinaryReader br = new BinaryReader(ns))
                            {
                                br.ReadBytes(SkipSize);

                                iVoiceBuff = br.ReadBytes(2);
                                iVoice = BitConverter.ToInt16(iVoiceBuff, 0); // うーん…

                                bCode = br.ReadByte();

                                iLengthBuff = br.ReadBytes(4);
                                iLength = BitConverter.ToInt32(iLengthBuff, 0); // うーん…

                                byte[] TalkTextBuff = new byte[iLength];

                                TalkTextBuff = br.ReadBytes(iLength);

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
                            }
                        }

                        iVoice = (short)(iVoice > 8 ? 9 : iVoice + 9);
                        if (ListenPort == Config.SocketPortNum2) iVoice += 18;

                        int cid = Config.B2Amap[iVoice];
                        int tid = MessQue.count + 1;

                        Dictionary<string, decimal> Effects = ParamAssignList[iVoice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                        Dictionary<string, decimal> Emotions = ParamAssignList[iVoice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                        MessageData talk = new MessageData()
                        {
                            Cid = cid,
                            Message = TalkText,
                            BouyomiVoice = iVoice,
                            TaskId = tid,
                            Effects = Effects,
                            Emotions = Emotions
                        };

                        switch(PlayMethod)
                        {
                            case methods.sync:
                                MessQue.AddQueue(talk);
                                break;

                            case methods.async:
                                WcfClient.TalkAsync(cid, TalkText, Effects, Emotions);
                                break;
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
