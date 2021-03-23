using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Windows.Threading;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace FakeChan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Configs Config;
        WCFClient WcfClient;
        BlockingCollection<MessageData> MessQue;
        TcpListener TcpIpListener;
        Action BGTcpListen;
        DispatcherTimer KickTalker;
        FNF.Utility.BouyomiChanRemoting ShareIpcObject;
        List<ComboBox> MapAvatorsComboBoxList;
        bool ReEntry;
        bool KeepListen;
        object lockObj = new object();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Config = new Configs();

                // AssistantSeikaとの接続
                // シンプルな例は
                // https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/interface/wcf/wcf-004
                // を見てください。
                WcfClient = new WCFClient();

                // メッセージキューを使うよ！
                MessQue = new BlockingCollection<MessageData>();
                ReEntry = true;

                // 読み上げバックグラウンドタスク起動
                KickTalker = new DispatcherTimer();
                KickTalker.Tick += new EventHandler(KickTalker_Tick);
                KickTalker.Interval = new TimeSpan(0, 0, 1);
                KickTalker.Start();

                // TCP/IP リスナタスク起動
                TcpIpListener = new TcpListener(Config.EndPoint.Address, Config.EndPoint.PortNum);
                TcpIpListener.Start();
                KeepListen = true;
                SetupBGTcpListenerTask();
                Task.Run(BGTcpListen);

                // IPCサービス起動（棒読みちゃんのフリをします！）
                ShareIpcObject = new FNF.Utility.BouyomiChanRemoting();
                ShareIpcObject.OnAddTalkTask01 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask01(IPCAddTalkTask01);
                ShareIpcObject.OnAddTalkTask02 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask02(IPCAddTalkTask02);
                ShareIpcObject.OnAddTalkTask03 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask03(IPCAddTalkTask03);
                ShareIpcObject.OnAddTalkTask21 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask21(IPCAddTalkTask21);
                ShareIpcObject.OnAddTalkTask23 += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerAddTalkTask23(IPCAddTalkTask23);
                ShareIpcObject.OnClearTalkTask += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerSimpleTask(IPCClearTalkTask);
                ShareIpcObject.OnSkipTalkTask  += new FNF.Utility.BouyomiChanRemoting.CallEventHandlerSimpleTask(IPCSkipTalkTask);
                ShareIpcObject.MessQue = MessQue;
                IpcServerChannel IpcCh = new IpcServerChannel("BouyomiChan");
                IpcCh.IsSecured = false;
                
                ChannelServices.RegisterChannel(IpcCh, false);
                RemotingServices.Marshal(ShareIpcObject, "Remoting", typeof(FNF.Utility.BouyomiChanRemoting));

            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Sorry1");
                Application.Current.Shutdown();
            }

            MapAvatorsComboBoxList = new List<ComboBox>()
            {
                ComboBoxMapAvator0,
                ComboBoxMapAvator1,
                ComboBoxMapAvator2,
                ComboBoxMapAvator3,
                ComboBoxMapAvator4,
                ComboBoxMapAvator5,
                ComboBoxMapAvator6,
                ComboBoxMapAvator7,
                ComboBoxMapAvator8,
            };

            if (Config.AvatorNames.Count != 0)
            {
                foreach (var item in MapAvatorsComboBoxList)
                {
                    item.ItemsSource = null;
                    item.ItemsSource = Config.AvatorNames;
                    item.SelectedIndex = 0;
                    item.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("No Avators detected from AssistantSeika", "Sorry2");
                Application.Current.Shutdown();
                return;
            }

            ComboBoxBouyomiVoice.ItemsSource = null;
            ComboBoxBouyomiVoice.ItemsSource = Config.BouyomiVoices;
            ComboBoxBouyomiVoice.SelectedIndex = 0;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KickTalker.Stop();
        }

        private void IPCAddTalkTask01(string TalkText)
        {
            int cid = Config.B2Amap.First().Value;
            int tid = MessQue.Count + 1;

            Dictionary<string, decimal> Effects = Config.AvatorEffectParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = Config.AvatorEmotionParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);

            MessageData talk = new MessageData()
            {
                Cid = cid,
                Message = TalkText,
                BouyomiVoice = 0,
                TaskId = tid,
                Effects = Effects,
                Emotions = Emotions
            };

            lock (lockObj)
            {
                MessQue.TryAdd(talk, 1000);
            }
        }

        private void IPCAddTalkTask02(string TalkText, int iSpeed, int iVolume, int vType)
        {
            int vt = vType > 8 ? 0 : vType;
            int cid = Config.B2Amap[vt];
            int tid = MessQue.Count + 1;

            Dictionary<string, decimal> Effects = Config.AvatorEffectParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);
            Dictionary<string, decimal> Emotions = Config.AvatorEmotionParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);

            MessageData talk = new MessageData()
            {
                Cid = cid,
                Message = TalkText,
                BouyomiVoice = vt,
                TaskId = tid,
                Effects = Effects,
                Emotions = Emotions
            };

            lock (lockObj)
            {
                MessQue.TryAdd(talk, 1000);
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
            // キューを空にする
            BlockingCollection<MessageData>[] t = { MessQue };
            BlockingCollection<MessageData>.TryTakeFromAny(t, out MessageData item);
        }
        private void IPCSkipTalkTask()
        {
        }

        private void KickTalker_Tick(object sender, EventArgs e)
        {
            if (MessQue.Count != 0)
            {
                lock (lockObj)
                {
                    if (ReEntry)
                    {
                        ReEntry = false;

                        Task.Run(() =>
                        {
                            Dictionary<int, string> bv = Config.BouyomiVoices;
                            Dictionary<int, string> an = Config.AvatorNames;
                            foreach (var item in MessQue.GetConsumingEnumerable())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    ShareIpcObject.taskId = item.TaskId;
                                    TextBoxReceveText.Text = item.Message;
                                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1}", bv[item.BouyomiVoice], an[item.Cid] );
                                });

                                WcfClient.Talk(item.Cid, item.Message, "", item.Effects, item.Emotions);
                            }

                            ShareIpcObject.taskId = 0;

                            ReEntry = true;
                        });

                    }
                }

            }
        }

        private void ComboBoxMapAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int voice = 0;
            int cid;
            ComboBox cb = sender as ComboBox;

            switch(cb.Name)
            {
                case "ComboBoxMapAvator0": voice = 0; break;
                case "ComboBoxMapAvator1": voice = 1; break;
                case "ComboBoxMapAvator2": voice = 2; break;
                case "ComboBoxMapAvator3": voice = 3; break;
                case "ComboBoxMapAvator4": voice = 4; break;
                case "ComboBoxMapAvator5": voice = 5; break;
                case "ComboBoxMapAvator6": voice = 6; break;
                case "ComboBoxMapAvator7": voice = 7; break;
                case "ComboBoxMapAvator8": voice = 8; break;
                default: voice = 0; break;
            }

            cid = ((KeyValuePair<int, string>)cb.SelectedItem).Key;
            Config.B2Amap[voice] = cid;

            if (ComboBoxBouyomiVoice.SelectedIndex != voice)
            {
                ComboBoxBouyomiVoice.SelectedIndex = voice;
            }
            else
            {
                UpdateEditParamPanel(Config.B2Amap[voice]);
            }
        }

        private void ComboBoxBouyomiVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int voice = Convert.ToInt32(ComboBoxBouyomiVoice.SelectedValue);
            int cid = Config.B2Amap[voice];

            UpdateEditParamPanel(cid);
        }

        private void UpdateEditParamPanel(int cid)
        {
            LabelSelectedAvator.Content = string.Format(@"⇒ {0}", Config.AvatorNames[cid]);
            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();

            ReSetupParams(cid, Config.AvatorEffectParams(cid), WrapPanelParams1.Children);
            ReSetupParams(cid, Config.AvatorEmotionParams(cid), WrapPanelParams2.Children);
        }

        private void ReSetupParams(int cid, Dictionary<string, Dictionary<string, decimal>> @params, UIElementCollection panel)
        {
            foreach(var param in @params)
            {
                StackPanel sp = new StackPanel();
                Label lb = new Label();
                TextBlock tb = new TextBlock();

                sp.Orientation = Orientation.Vertical;
                sp.Margin = new Thickness(2, 0, 2, 2);
                sp.Background = new SolidColorBrush(Color.FromArgb(0xff, 253, 252, 227));

                // パラメタ名
                lb.HorizontalAlignment = HorizontalAlignment.Left;
                lb.Content = param.Key;

                // スライダー
                Slider sl = new Slider();
                sl.Name = param.Key;
                sl.Width = 100;
                sl.Minimum = Convert.ToDouble(param.Value["min"]);
                sl.Maximum = Convert.ToDouble(param.Value["max"]);
                sl.SelectionStart = Convert.ToDouble(param.Value["min"]);
                sl.SelectionEnd = Convert.ToDouble(param.Value["max"]);
                sl.Value = Convert.ToDouble(param.Value["value"]);
                sl.TickFrequency = Convert.ToDouble(param.Value["step"]);
                sl.LargeChange = Convert.ToDouble(param.Value["step"]);
                sl.SmallChange = Convert.ToDouble(param.Value["step"]);
                sl.IsSnapToTickEnabled = true;

                sl.ValueChanged += (sender, args) => {
                    param.Value["value"] = Convert.ToDecimal(sl.Value);
                };

                // 数値（表示）
                Binding myBinding = new Binding("Value");
                myBinding.Source = sl;
                tb.SetBinding(TextBlock.TextProperty, myBinding);
                tb.HorizontalAlignment = HorizontalAlignment.Center;

                sp.Children.Add(lb);
                sp.Children.Add(sl);
                sp.Children.Add(tb);

                panel.Add(sp);
            }
        }

        private void ButtonTestTalk_Click(object sender, RoutedEventArgs e)
        {
            TextBoxReceveText.IsEnabled = false;
            ButtonTestTalk.IsEnabled = false;
            ComboBoxBouyomiVoice.IsEnabled = false;

            string text = TextBoxReceveText.Text;
            int voice = ((KeyValuePair<int, string>)ComboBoxBouyomiVoice.SelectedItem).Key;

            // See https://gist.github.com/pinzolo/2814091
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);

            Task.Run(() =>
            {
                int cid =  Config.B2Amap[voice];
                Dictionary<string, decimal> Effects = Config.AvatorEffectParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);
                Dictionary<string, decimal> Emotions = Config.AvatorEmotionParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);

                WcfClient.Talk(cid, text, "", Effects, Emotions);

                Dispatcher.Invoke(() =>
                {
                    TextBoxReceveText.IsEnabled = true;
                    ButtonTestTalk.IsEnabled = true;
                    ComboBoxBouyomiVoice.IsEnabled = true;
                });
            });

        }

        private void SetupBGTcpListenerTask()
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

            BGTcpListen = (()=>{

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

                        int cid = Config.B2Amap.First().Value;
                        int tid = MessQue.Count + 1;

                        iVoice = (short)(iVoice > 8 ? 0 : iVoice);

                        cid = Config.B2Amap[iVoice];
                        Dictionary<string, decimal> Effects = Config.AvatorEffectParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);
                        Dictionary<string, decimal> Emotions = Config.AvatorEmotionParams(cid).ToDictionary(k => k.Key, v => v.Value["value"]);

                        MessageData talk = new MessageData()
                        {
                            Cid = cid,
                            Message = TalkText,
                            BouyomiVoice = iVoice,
                            TaskId = tid,
                            Effects = Effects,
                            Emotions = Emotions
                        };

                        lock (lockObj)
                        {
                            MessQue.TryAdd(talk, 1000);
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

        }

    }
}
