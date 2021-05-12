﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FakeChan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string titleStr = "偽装ちゃん";
        string versionStr = "Ver 1.3.3";
        MessQueueWrapper MessQueWrapper = new MessQueueWrapper();
        Configs Config;
        IpcTasks IpcTask = null;
        SocketTasks SockTask = null;
        HttpTasks HttpTask = null;
        SocketTasks SockTask2 = null;
        HttpTasks HttpTask2 = null;
        EditParamsBefore TestEdit = new EditParamsBefore();
        Random r = new Random(Environment.TickCount);
        ObservableCollection<ReplaceDefinition> Regexs;

        WCFClient WcfClient;
        DispatcherTimer KickTalker;

        Dictionary<int, string> LonelyAvatorNames;
        List<int> LonelyCidList;
        int LonelyCount = 0;
        int QuietMessageKeyMax;

        bool ReEntry;
        object lockObj = new object();

        public UserDefData UserData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = titleStr + " " + versionStr;

            try
            {
                // AssistantSeikaとの接続
                // シンプルな例は
                // https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/interface/wcf/wcf-004
                // を見てください。
                WcfClient = new WCFClient();

                if (WcfClient.AvatorList().Count == 0)
                {
                    throw new Exception("No Avators detected from AssistantSeika");
                }

                // 設定色々
                Config = new Configs(ref WcfClient);
            }
            catch (Exception e0)
            {
                MessageBox.Show("AssistantSeikaを起動していないか、AssistantSeikaが音声合成製品を認識していない可能性があります。" + "\n" + e0.Message, "AssistantSeikaの状態");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                // 古いバージョンの設定値のバージョンアップを試みる
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeRequired = false;
                    //Properties.Settings.Default.Save();
                }

            }
            catch (Exception e0)
            {
                MessageBox.Show(e0.Message, "設定値読み込みの問題1");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                // 設定値を取り込むよ！
                UserData = new UserDefData();
                if (Properties.Settings.Default.UserSettings != "")
                {
                    DataContractJsonSerializer uds = new DataContractJsonSerializer(typeof(UserDefData));
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Settings.Default.UserSettings));
                    UserData = (UserDefData)uds.ReadObject(ms);
                    ms.Close();
                }
            }
            catch (Exception e0)
            {
                MessageBox.Show(e0.Message, "設定値読み込みの問題2");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                // 設定値を取り込むよ！
                UserData.QuietMessages = Config.MessageLoader();
            }
            catch (Exception e0)
            {
                MessageBox.Show(e0.Message, "設定値読み込みの問題3");
                Application.Current.Shutdown();
                return;
            }

            try
            {
                // 古い版のデータだったら補正
                // 設定値が取り込めない環境がある模様だ。対策する

                if (UserData is null)
                {
                    UserData = new UserDefData();
                }

                if (UserData.VoiceParams is null)
                {
                    UserData.VoiceParams = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>>>();
                    UserData.SelectedCid = new Dictionary<int, Dictionary<int, int>>();
                    UserData.SelectedCallMethod = new Dictionary<int, int>();

                    foreach (ListenInterface InterfaceIdx in Enum.GetValues(typeof(ListenInterface)))
                    {
                        UserData.VoiceParams[(int)InterfaceIdx] = new Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>>();
                        UserData.SelectedCid[(int)InterfaceIdx] = new Dictionary<int, int>();
                        UserData.SelectedCallMethod[(int)InterfaceIdx] = (int)Methods.sync;

                        foreach (BouyomiVoice BouIdx in Enum.GetValues(typeof(BouyomiVoice)))
                        {
                            UserData.VoiceParams[(int)InterfaceIdx][(int)BouIdx] = new Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>();
                            UserData.SelectedCid[(int)InterfaceIdx][(int)BouIdx] = Config.AvatorNames.First().Key;

                            foreach (int cid in Config.AvatorNames.Keys)
                            {
                                UserData.VoiceParams[(int)InterfaceIdx][(int)BouIdx][cid] = Config.AvatorParams(cid);
                            }
                        }
                    }
                }

                if (UserData.InterfaceSwitch is null)
                {
                    UserData.InterfaceSwitch = new Dictionary<int, bool>()
                        {
                            {0, true },
                            {1, true },
                            {2, true },
                            {3, false },
                            {4, false }
                        };
                }

                if(UserData.RandomVoiceMethod is null)
                {
                    UserData.RandomVoiceMethod = new Dictionary<int, int>()
                    {
                        { 0, 0 },
                        { 1, 0 },
                        { 2, 0 },
                        { 3, 0 },
                        { 4, 0 }
                    };
                }

                if(UserData.QuietMessages is null)
                {
                    UserData.QuietMessages = new Dictionary<int, List<string>>();
                }

                if (UserData.AddSuffixStr is null)
                {
                    UserData.AddSuffix = false;
                    UserData.AddSuffixStr = "(以下略";
                    UserData.TextLength = 96;
                }

                if (UserData.ReplaceDefs is null)
                {
                    UserData.ReplaceDefs = new List<ReplaceDefinition>();
                    UserData.ReplaceDefs.Add(new ReplaceDefinition() { Apply = true, MatchingPattern = @"([^0-9０-９])[8８]{3,}",            ReplaceText = @"$1パチパチパチ" });
                    UserData.ReplaceDefs.Add(new ReplaceDefinition() { Apply = true, MatchingPattern = @"^[8８]{3,}",                        ReplaceText = @"パチパチパチ" });
                    UserData.ReplaceDefs.Add(new ReplaceDefinition() { Apply = true, MatchingPattern = @"([^a-zA-Zａ-ｚＡ-Ｚ])[WwＷｗ]{1,}", ReplaceText = @"$1わらわら" });
                    UserData.ReplaceDefs.Add(new ReplaceDefinition() { Apply = true, MatchingPattern = @"^[WwＷｗ]{2,}",                     ReplaceText = @"わらわら" });
                    UserData.ReplaceDefs.Add(new ReplaceDefinition() { Apply = true, MatchingPattern = @"https*:\/\/[^\t 　]{1,}",           ReplaceText = @"URL省略" });
                }
            }
            catch (Exception e0)
            {
                MessageBox.Show(e0.Message, "設定値読み込みの問題4");
                Application.Current.Shutdown();
                return;
            }

            // サイレントメッセージ最大待ち時間
            if (UserData.QuietMessages.Count != 0)
            {
                QuietMessageKeyMax = UserData.QuietMessages.Max(c => c.Key);
                if (QuietMessageKeyMax > (2 * 24 * 60 * 60))
                {
                    QuietMessageKeyMax = 2 * 24 * 60 * 60; // 2日間
                    UserData.QuietMessages = UserData.QuietMessages.Where(c => c.Key <= QuietMessageKeyMax).ToDictionary(c =>c.Key, v=>v.Value);
                }

                CheckBoxIsSilent.IsChecked = UserData.IsSilentAvator;
            }
            else
            {
                QuietMessageKeyMax = 0;
                CheckBoxIsSilent.IsChecked = false;
                CheckBoxIsSilent.IsEnabled = false;
            }

            // バックグラウンドタスク用オブジェクト
            IpcTask = new IpcTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask2 = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask2 = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);

            // 非同期発声時のGUI操作用
            IpcTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask2.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask2.OnCallAsyncTalk += TalkAsyncCall;

            // 話者設定 コンボボックス設定
            ComboBoxInterface.ItemsSource = null;
            ComboBoxInterface.ItemsSource = ConstClass.BouyomiInterface;
            ComboBoxInterface.DisplayMemberPath = "LabelData";
            ComboBoxInterface.SelectedValuePath = "ValueData";

            ComboBoxCallMethod.ItemsSource = null;
            ComboBoxCallMethod.ItemsSource = ConstClass.BouyomiCallMethod;
            ComboBoxCallMethod.DisplayMemberPath = "LabelData";
            ComboBoxCallMethod.SelectedValuePath = "ValueData";

            List<ComboBox> MapAvatorsComboBoxList = new List<ComboBox>()
            {
                ComboBoxMapVoice0,
                ComboBoxMapVoice1,
                ComboBoxMapVoice2,
                ComboBoxMapVoice3,
                ComboBoxMapVoice4,
                ComboBoxMapVoice5,
                ComboBoxMapVoice6,
                ComboBoxMapVoice7,
                ComboBoxMapVoice8
            };
            for (int idx = 0; idx < MapAvatorsComboBoxList.Count; idx++)
            {
                MapAvatorsComboBoxList[idx].ItemsSource = null;
                MapAvatorsComboBoxList[idx].ItemsSource = Config.AvatorNames;
                MapAvatorsComboBoxList[idx].IsEnabled = true;
                MapAvatorsComboBoxList[idx].Tag = idx;
            }

            ComboBoxRandomAssignVoice.ItemsSource = null;
            ComboBoxRandomAssignVoice.ItemsSource = ConstClass.RandomAssignMethod;
            ComboBoxRandomAssignVoice.SelectedIndex = UserData.RandomVoiceMethod[(int)ListenInterface.IPC1];

            ComboBoxInterface.SelectedIndex = (int)ListenInterface.IPC1;

            // 音声設定 コンボボックス設定
            ComboBoxEditInterface.ItemsSource = null;
            ComboBoxEditInterface.DisplayMemberPath = "LabelData";
            ComboBoxEditInterface.SelectedValuePath = "ValueData";
            ComboBoxEditInterface.ItemsSource = ConstClass.BouyomiInterface;

            ComboBoxEditBouyomiVoice.ItemsSource = null;
            ComboBoxEditBouyomiVoice.DisplayMemberPath = "LabelData";
            ComboBoxEditBouyomiVoice.SelectedValuePath = "ValueData";
            ComboBoxEditBouyomiVoice.ItemsSource = ConstClass.BouyomiVoiceName;

            ComboBoxEditInterface.SelectedIndex = 0;
            ComboBoxEditBouyomiVoice.SelectedIndex = 0;

            // 置換設定 テキストボックス設定
            TextBoxTextLength.Text           = UserData.TextLength.ToString();
            EditParamsBefore.LimitTextLength = UserData.TextLength;
            CheckBoxAddSuffix.IsChecked      = EditParamsBefore.IsUseSuffixString = UserData.AddSuffix;
            TextBoxAddSuffixStr.Text         = EditParamsBefore.SuffixString = UserData.AddSuffixStr;
            Regexs = new ObservableCollection<ReplaceDefinition>(UserData.ReplaceDefs);
            DataGridRepDefs.DataContext = null;
            DataGridRepDefs.DataContext = Regexs;
            if (UserData.ReplaceDefs.Count != 0) DataGridRepDefs.SelectedIndex = 0;
            EditParamsBefore.CopyRegExs(ref Regexs);

            // 状態 受信インタフェース
            EllipseIpc.Tag     = 0;
            EllipseSocket.Tag  = 1;
            EllipseHTTP.Tag    = 2;
            EllipseSocket2.Tag = 3;
            EllipseHTTP2.Tag   = 4;

            try
            {
                // 読み上げバックグラウンドタスク起動
                LonelyAvatorNames = Config.AvatorNames;
                LonelyCidList = LonelyAvatorNames.Select(c => c.Key).ToList();
                LonelyCount = 0;
                KickTalker = new DispatcherTimer();
                KickTalker.Tick += new EventHandler(KickTalker_Tick);
                KickTalker.Interval = new TimeSpan(0, 0, 1);
                ReEntry = true;
                KickTalker.Start();

                // 受信インタフェース バックグラウンドタスク起動

                if ((UserData.InterfaceSwitch[(int)ListenInterface.IPC1] == true) && IpcTask.StartIpcTasks(Config.IPCChannelName))
                {
                    EllipseIpc.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.IPC1] = true;
                }
                else
                {
                    EllipseIpc.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.IPC1] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Socket1] == true)
                {
                    SockTask.StartSocketTasks(Config.SocketAddress, Config.SocketPortNum);
                    EllipseSocket.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket1] = true;
                }
                else
                {
                    EllipseSocket.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket1] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Http1] == true)
                {
                    HttpTask.StartHttpTasks(Config.HttpAddress, Config.HttpPortNum);
                    EllipseHTTP.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http1] = true;
                }
                else
                {
                    EllipseHTTP.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http1] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Socket2] == true)
                {
                    SockTask2.StartSocketTasks(Config.SocketAddress2, Config.SocketPortNum2);
                    EllipseSocket2.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket2] = true;
                }
                else
                {
                    EllipseSocket2.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket2] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Http2] == true)
                {
                    HttpTask2.StartHttpTasks(Config.HttpAddress2, Config.HttpPortNum2);
                    EllipseHTTP2.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http2] = true;
                }
                else
                {
                    EllipseHTTP2.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http2] = false;
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, titleStr + " 1");
                Application.Current.Shutdown();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserData.ReplaceDefs = new List<ReplaceDefinition>(Regexs);
            DataContractJsonSerializer uds = new DataContractJsonSerializer(typeof(UserDefData));
            MemoryStream ms = new MemoryStream();
            uds.WriteObject(ms, UserData);
            
            Properties.Settings.Default.UserSettings = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)(ms.Length));
            Properties.Settings.Default.Save();
            ms.Close();

            KickTalker?.Stop();
            SockTask?.StopSocketTasks();
            IpcTask?.StopIpcTasks();
            HttpTask?.StopHttpTasks();
            SockTask2?.StopSocketTasks();
            HttpTask2?.StopHttpTasks();
        }

        private void KickTalker_Tick(object sender, EventArgs e)
        {
            if (MessQueWrapper.count != 0)
            {
                lock (lockObj)
                {
                    if (ReEntry)
                    {
                        ReEntry = false;

                        Task.Run(() =>
                        {
                            Dictionary<int, string> an = Config.AvatorNames;
                            foreach (var item in MessQueWrapper.QueueRef().GetConsumingEnumerable())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    IpcTask?.SetTaskId(item.TaskId);
                                    HttpTask?.SetTaskId(item.TaskId);
                                    HttpTask2?.SetTaskId(item.TaskId);
                                    TextBlockTweetCounter.Text = "0";
                                    TextBlockReceveText.Text = item.Message;
                                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1} ⇒ {2}", ConstClass.ListenInterfaceMap[item.ListenInterface], ConstClass.BouyomiVoiceMap[item.BouyomiVoice], an[item.Cid] );
                                });

                                WcfClient.Talk(item.Cid, item.Message, "", item.Effects, item.Emotions);
                                LonelyCount = 0;
                            }

                            IpcTask?.SetTaskId(0);
                            HttpTask?.SetTaskId(0);
                            HttpTask2?.SetTaskId(0);

                            ReEntry = true;
                        });

                    }
                }
            }
            else
            {
                if (QuietMessageKeyMax != 0)
                {
                    int cnt = LonelyCount;
                    Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            string s = (!UserData.IsSilentAvator && UserData.QuietMessages.ContainsKey(cnt)) ? "match" : "";
                            TextBlockTweetCounter.Text = string.Format(@"{0} ({1} Sec) {2}", TimeSpan.FromSeconds(cnt), cnt, s);
                            TextBlockTweetMuteStatus.Text = UserData.IsSilentAvator ? "はい" : "いいえ";
                        });
                    });
                    if (!UserData.IsSilentAvator && UserData.QuietMessages.ContainsKey(cnt))
                    {
                        int cid = LonelyCidList[r.Next(0, LonelyCidList.Count)];
                        int idx = r.Next(0, UserData.QuietMessages[cnt].Count);
                        Task.Run(() =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                TextBlockReceveText.Text = UserData.QuietMessages[cnt][idx];
                                TextBlockAvatorText.Text = string.Format(@"⇒ {0}", LonelyAvatorNames[cid]);
                                //Console.WriteLine(@"{0}-{1} : {2}", cnt, idx, UserData.QuietMessages[cnt][idx]);
                            });
                            WcfClient.Talk(cid, UserData.QuietMessages[cnt][idx], "", null, null);
                        });
                    }
                    LonelyCount++;
                    if (LonelyCount > QuietMessageKeyMax) LonelyCount = 0;
                }
            }

        }

        private void TalkAsyncCall(MessageData talk)
        {
            Task.Run(() =>
            {
                Dictionary<int, string> an = Config.AvatorNames;
                Dispatcher.Invoke(() =>
                {
                    IpcTask?.SetTaskId(talk.TaskId);
                    HttpTask?.SetTaskId(talk.TaskId);
                    HttpTask2?.SetTaskId(talk.TaskId);
                    TextBoxReceveText.Text = talk.Message;
                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1} ⇒ {2}", ConstClass.ListenInterfaceMap[talk.ListenInterface], ConstClass.BouyomiVoiceMap[talk.BouyomiVoice], an[talk.Cid]);
                });

                WcfClient.TalkAsync(talk.Cid, talk.Message, talk.Effects, talk.Emotions);
            });
        }

        private void EllipseConnect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Ellipse ep = sender as Ellipse;
            int swNo = (int)ep.Tag;
            bool sw = UserData.InterfaceSwitch[swNo];

            ep.Fill = sw ? Brushes.Black : Brushes.LightGreen;

            if (sw)
            {
                UserData.InterfaceSwitch[swNo] = false;
                switch (swNo)
                {
                    case 0: IpcTask.StopIpcTasks();      break;
                    case 1: SockTask.StopSocketTasks();  break;
                    case 2: HttpTask.StopHttpTasks();    break;
                    case 3: SockTask2.StopSocketTasks(); break;
                    case 4: HttpTask2.StopHttpTasks();   break;
                }
            }
            else
            {
                UserData.InterfaceSwitch[swNo] = true;
                switch (swNo)
                {
                    case 0:if (!IpcTask.StartIpcTasks(Config.IPCChannelName)) { ep.Fill = Brushes.Black; UserData.InterfaceSwitch[swNo] = false; } break;
                    case 1: SockTask.StartSocketTasks(Config.SocketAddress, Config.SocketPortNum);    break;
                    case 2: HttpTask.StartHttpTasks(Config.HttpAddress, Config.HttpPortNum);          break;
                    case 3: SockTask2.StartSocketTasks(Config.SocketAddress2, Config.SocketPortNum2); break;
                    case 4: HttpTask2.StartHttpTasks(Config.HttpAddress2, Config.HttpPortNum2);       break;
                }
            }
        }

        private void TextBox_MaxTextSizePreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void TextBox_MaxTextSizePreviewExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = !new Regex("[0-9]").IsMatch(tb.Text);
            }
        }

        private void TextBoxTextLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int len;
            if (int.TryParse(tb.Text, out len))
            {
                if (len < 1) len = 1;
            }
            else
            {
                len = 1;
            }

            EditParamsBefore.LimitTextLength = UserData.TextLength = len;
        }

        private void TextBoxTextLength_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if ((tb.Text == "0")|| (tb.Text == "")) tb.Text = "1";
        }

        private void TextBoxAddSuffixStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            EditParamsBefore.SuffixString = UserData.AddSuffixStr = tb.Text;
        }

        private void CheckBoxAddSuffix_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            EditParamsBefore.IsUseSuffixString = UserData.AddSuffix = (bool)cb.IsChecked;
        }

        private void ComboBoxInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxInterface.SelectedIndex == -1) return;

            int ListenIf = (int)ComboBoxInterface.SelectedValue;
            ComboBoxCallMethod.SelectedIndex = UserData.SelectedCallMethod[ListenIf];
            ComboBoxRandomAssignVoice.SelectedIndex = UserData.RandomVoiceMethod[ListenIf];

            SetupVoiceMapGUI();
        }

        private void ComboBoxCallMethod_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCallMethod.SelectedIndex == -1) return;

            int ListenIf = (int)ComboBoxInterface.SelectedValue;
            UserData.SelectedCallMethod[ListenIf] = (int)ComboBoxCallMethod.SelectedValue;
            switch(ListenIf)
            {
                case 0: IpcTask.PlayMethod   = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 1: SockTask.PlayMethod  = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 2: HttpTask.PlayMethod  = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 3: SockTask2.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 4: HttpTask2.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
            }
        }

        private void ComboBoxMapVoice0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if (ComboBoxInterface.SelectedIndex == -1) return;
            if (cb.SelectedIndex == -1) return;
            if (cb.Tag is null) return;

            int ListenIf = (int)ComboBoxInterface.SelectedValue;
            UserData.SelectedCid[ListenIf][(int)cb.Tag] = (int)cb.SelectedValue;
        }

        private void SetupVoiceMapGUI()
        {
            int ListenIf = (int)ComboBoxInterface.SelectedValue;
            List<int> ListCid = Config.AvatorNames.Select(v => v.Key).ToList();
            for (int idx = 0; idx < ListCid.Count; idx++)
            {
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.voice0] == ListCid[idx]) ComboBoxMapVoice0.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.female1] == ListCid[idx]) ComboBoxMapVoice1.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.female2] == ListCid[idx]) ComboBoxMapVoice2.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.male1] == ListCid[idx]) ComboBoxMapVoice3.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.male2] == ListCid[idx]) ComboBoxMapVoice4.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.nogender] == ListCid[idx]) ComboBoxMapVoice5.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.robot] == ListCid[idx]) ComboBoxMapVoice6.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.machine1] == ListCid[idx]) ComboBoxMapVoice7.SelectedIndex = idx;
                if (UserData.SelectedCid[ListenIf][(int)BouyomiVoice.machine2] == ListCid[idx]) ComboBoxMapVoice8.SelectedIndex = idx;
            }
            if (ComboBoxMapVoice0.SelectedIndex == -1) { ComboBoxMapVoice0.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.voice0] = ListCid[0]; }
            if (ComboBoxMapVoice1.SelectedIndex == -1) { ComboBoxMapVoice1.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.female1] = ListCid[0]; }
            if (ComboBoxMapVoice2.SelectedIndex == -1) { ComboBoxMapVoice2.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.female2] = ListCid[0]; }
            if (ComboBoxMapVoice3.SelectedIndex == -1) { ComboBoxMapVoice3.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.male1] = ListCid[0]; }
            if (ComboBoxMapVoice4.SelectedIndex == -1) { ComboBoxMapVoice4.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.male2] = ListCid[0]; }
            if (ComboBoxMapVoice5.SelectedIndex == -1) { ComboBoxMapVoice5.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.nogender] = ListCid[0]; }
            if (ComboBoxMapVoice6.SelectedIndex == -1) { ComboBoxMapVoice6.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.robot] = ListCid[0]; }
            if (ComboBoxMapVoice7.SelectedIndex == -1) { ComboBoxMapVoice7.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.machine1] = ListCid[0]; }
            if (ComboBoxMapVoice8.SelectedIndex == -1) { ComboBoxMapVoice8.SelectedIndex = 0; UserData.SelectedCid[ListenIf][(int)BouyomiVoice.machine2] = ListCid[0]; }
        }

        private void ComboBoxEditInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxEditInterface.SelectedIndex == -1) return;
            if (ComboBoxEditBouyomiVoice.SelectedIndex == -1) return;
            UpdateEditParamPanel();
        }

        private void ComboBoxEditBouyomiVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxEditBouyomiVoice.SelectedIndex == -1) return;
            UpdateEditParamPanel();
        }

        private void ButtonParamReset_Click(object sender, RoutedEventArgs e)
        {
            int ListenIf = (int)ComboBoxEditInterface.SelectedValue;
            int BouVoice = (int)ComboBoxEditBouyomiVoice.SelectedValue;
            int cid = UserData.SelectedCid[ListenIf][BouVoice];

            foreach (var item1 in Config.AvatorEffectParams(cid))
            {
                foreach (var item2 in item1.Value)
                {
                    UserData.VoiceParams[ListenIf][BouVoice][cid]["effect"][item1.Key][item2.Key] = item2.Value;
                }
            }
            foreach (var item1 in Config.AvatorEmotionParams(cid))
            {
                foreach (var item2 in item1.Value)
                {
                    UserData.VoiceParams[ListenIf][BouVoice][cid]["emotion"][item1.Key][item2.Key] = item2.Value;
                }
            }
            UpdateEditParamPanel();
        }

        private void ButtonTestTalk_Click(object sender, RoutedEventArgs e)
        {
            int ListenIf = (int)ComboBoxEditInterface.SelectedValue;
            int BouVoice = (int)ComboBoxEditBouyomiVoice.SelectedValue;
            int cid = UserData.SelectedCid[ListenIf][BouVoice];

            TextBoxReceveText.IsEnabled = false;
            ComboBoxEditInterface.IsEnabled = false;
            ComboBoxEditBouyomiVoice.IsEnabled = false;
            ButtonParamReset.IsEnabled = false;
            ButtonTestTalk.IsEnabled = false;

            // See https://gist.github.com/pinzolo/2814091
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);

            string text = TextBoxReceveText.Text;

            TestEdit.EditInputString(BouVoice, text);

            Task.Run(() =>
            {
                Dictionary<string, decimal> Effects = UserData.VoiceParams[ListenIf][BouVoice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                Dictionary<string, decimal> Emotions = UserData.VoiceParams[ListenIf][BouVoice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                WcfClient.Talk(cid, TestEdit.ChangedTalkText, "", Effects, Emotions);

                Dispatcher.Invoke(() =>
                {
                    TextBoxReceveText.IsEnabled = true;
                    ComboBoxEditInterface.IsEnabled = true;
                    ComboBoxEditBouyomiVoice.IsEnabled = true;
                    ButtonParamReset.IsEnabled = true;
                    ButtonTestTalk.IsEnabled = true;
                });
            });
        }

        private void CheckBoxIsSilent_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            UserData.IsSilentAvator = (bool)cb.IsChecked;
        }

        private void UpdateEditParamPanel()
        {
            int ListenIf = (int)ComboBoxEditInterface.SelectedValue;
            int BouVoice = (int)ComboBoxEditBouyomiVoice.SelectedValue;
            int cid = UserData.SelectedCid[ListenIf][BouVoice];

            TextBlockAvatorName.Text = string.Format(@"⇒ {0}", Config.AvatorNames[cid]);

            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();
            Dictionary<string, Dictionary<string, decimal>> effect = UserData.VoiceParams[ListenIf][BouVoice][cid]["effect"];
            Dictionary<string, Dictionary<string, decimal>> emotion = UserData.VoiceParams[ListenIf][BouVoice][cid]["emotion"];

            ReSetupParams(cid, ref effect,  WrapPanelParams1.Children);
            ReSetupParams(cid, ref emotion, WrapPanelParams2.Children);
        }

        private void ReSetupParams(int cid, ref Dictionary<string, Dictionary<string, decimal>> @params, UIElementCollection panel)
        {
            foreach (var param in @params)
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

        private void ButtonInsert_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = DataGridRepDefs;

            Regexs.Insert(dg.SelectedIndex, new ReplaceDefinition());
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = DataGridRepDefs;

            Regexs.Remove(dg.SelectedItem as ReplaceDefinition);
        }

        private void ButtonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = DataGridRepDefs;

            if (dg.SelectedIndex > 0)
            {
                var x1 = Regexs[dg.SelectedIndex - 1] as ReplaceDefinition;
                var x2 = x1.Clone();

                Regexs.Insert(dg.SelectedIndex + 1, x2);
                Regexs.Remove(x1);
            }
        }

        private void ButtonMoveDn_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = DataGridRepDefs;

            if (dg.SelectedIndex < (dg.Items.Count - 1))
            {
                var x1 = Regexs[dg.SelectedIndex + 1] as ReplaceDefinition;
                var x2 = x1.Clone();
                Regexs.Insert(dg.SelectedIndex, x2);
                Regexs.Remove(x1);
            }
        }

        private void ComboBoxRandomAssignVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            UserData.RandomVoiceMethod[ComboBoxInterface.SelectedIndex] = cb.SelectedIndex;
        }
    }
}
