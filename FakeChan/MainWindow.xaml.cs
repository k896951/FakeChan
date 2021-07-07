using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        string versionStr = "Ver 2.0.10.1";
        MessQueueWrapper MessQueWrapper = new MessQueueWrapper();
        Configs Config;
        IpcTasks IpcTask = null;
        SocketTasks SockTask = null;
        HttpTasks HttpTask = null;
        SocketTasks SockTask2 = null;
        HttpTasks HttpTask2 = null;
        SocketTasks SockTask3 = null;
        HttpTasks HttpTask3 = null;
        SocketTasks SockTask4 = null;
        HttpTasks HttpTask4 = null;
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
        WindowInteropHelper Whelper;
        EditParamsBefore EditInputText;
        Methods PlayMethodFromClipboard = Methods.sync;

        public UserDefData UserData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "初期化中"; // titleStr + " " + versionStr;

            Whelper = new WindowInteropHelper(this);
            EditInputText = new EditParamsBefore();
            var x = HwndSource.FromHwnd(Whelper.Handle);
            x.AddHook(WndProc);


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
                MessageBox.Show("前提ソフトウエアであるAssistantSeikaを起動していないか、AssistantSeikaが音声合成製品を認識していない可能性があります。" + "\n" + e0.Message, "AssistantSeikaの状態");
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
                }
                if (UserData.SelectedCid is null)
                {
                    UserData.SelectedCid = new Dictionary<int, Dictionary<int, int>>();
                }
                if (UserData.SelectedCallMethod is null)
                {
                    UserData.SelectedCallMethod = new Dictionary<int, int>();
                }
                if (UserData.InterfaceSwitch is null)
                {
                    UserData.InterfaceSwitch = new Dictionary<int, bool>();
                }
                if (UserData.RandomVoiceMethod is null)
                {
                    UserData.RandomVoiceMethod = new Dictionary<int, int>();
                }
                if (UserData.VriAvators is null)
                {
                    UserData.VriAvators = new Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>();
                }

                foreach (ListenInterface InterfaceIdx in Enum.GetValues(typeof(ListenInterface)))
                {
                    if (!UserData.VoiceParams.ContainsKey((int)InterfaceIdx))
                    {
                        UserData.VoiceParams.Add((int)InterfaceIdx, new Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>>());
                    }
                    if (!UserData.SelectedCid.ContainsKey((int)InterfaceIdx))
                    {
                        UserData.SelectedCid.Add((int)InterfaceIdx, new Dictionary<int, int>());
                    }
                    if (!UserData.SelectedCallMethod.ContainsKey((int)InterfaceIdx))
                    {
                        UserData.SelectedCallMethod.Add((int)InterfaceIdx, (int)Methods.sync);
                    }
                    if (!UserData.InterfaceSwitch.ContainsKey((int)InterfaceIdx))
                    {
                        switch(InterfaceIdx)
                        {
                            case ListenInterface.IPC1:
                            case ListenInterface.Http1:
                            case ListenInterface.Socket1:
                                UserData.InterfaceSwitch.Add((int)InterfaceIdx, true);
                                break;
                            default:
                                UserData.InterfaceSwitch.Add((int)InterfaceIdx, false);
                                break;
                        }
                    }
                    if (!UserData.RandomVoiceMethod.ContainsKey((int)InterfaceIdx))
                    {
                        UserData.RandomVoiceMethod.Add((int)InterfaceIdx, 0);
                    }

                    foreach (BouyomiVoice BouIdx in Enum.GetValues(typeof(BouyomiVoice)))
                    {
                        if (!UserData.VoiceParams[(int)InterfaceIdx].ContainsKey((int)BouIdx))
                        {
                            UserData.VoiceParams[(int)InterfaceIdx].Add((int)BouIdx, new Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>());
                            foreach (int cid in Config.AvatorNames.Keys)
                            {
                                UserData.VoiceParams[(int)InterfaceIdx][(int)BouIdx][cid] = Config.AvatorParams(cid);
                            }
                        }

                        if(!UserData.SelectedCid[(int)InterfaceIdx].ContainsKey((int)BouIdx))
                        {
                            UserData.SelectedCid[(int)InterfaceIdx][(int)BouIdx] = Config.AvatorNames.First().Key;
                        }
                    }

                }

                foreach (int cid in Config.AvatorNames.Keys)
                {
                    if (!UserData.VriAvators.ContainsKey(cid))
                    {
                        UserData.VriAvators.Add(cid, Config.AvatorParams(cid));
                    }
                }

                if (UserData.QuietMessages is null)
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

                if ((UserData.FakeChanAppName is null) || (UserData.FakeChanAppName == ""))
                {
                    UserData.FakeChanAppName = "偽装ちゃん";
                }

                if (UserData.VriAvator == 0)
                {
                    UserData.VriAvator = Config.AvatorNames.First().Key;
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

                GroupBoxTweetDisplay.Visibility = Visibility.Visible;
                GroupBoxTweetControl.Visibility = Visibility.Visible;
                CheckBoxIsSilent.IsEnabled = true;
                CheckBoxIsSilent.IsChecked = UserData.IsSilentAvator;
            }
            else
            {
                QuietMessageKeyMax = 0;
                GroupBoxTweetDisplay.Visibility = Visibility.Hidden;
                GroupBoxTweetControl.Visibility = Visibility.Collapsed;
                CheckBoxIsSilent.IsEnabled = false;
                CheckBoxIsSilent.IsChecked = false;
            }

            // バックグラウンドタスク用オブジェクト
            IpcTask = new IpcTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask2 = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask3 = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            SockTask4 = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask2 = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask3 = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);
            HttpTask4 = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData);

            // 非同期発声時のGUI操作用
            IpcTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask2.OnCallAsyncTalk += TalkAsyncCall;
            SockTask3.OnCallAsyncTalk += TalkAsyncCall;
            SockTask4.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask2.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask3.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask4.OnCallAsyncTalk += TalkAsyncCall;

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
            EllipseClipboard.Tag = 5;
            EllipseSocket3.Tag = 6;
            EllipseHTTP3.Tag   = 7;
            EllipseSocket4.Tag = 8;
            EllipseHTTP4.Tag   = 9;

            // アプリ設定
            TextBoxAppName.Text = UserData.FakeChanAppName;
            this.Title = UserData.FakeChanAppName + " " + this.versionStr;
            CheckBoxVriEng.IsChecked = EditParamsBefore.VriEng = UserData.VriEng;
            CheckBoxVriRep.IsChecked = EditParamsBefore.VriNoRep = UserData.VriRep;
            ComboBoxVriEngAvator.ItemsSource = null;
            ComboBoxVriEngAvator.ItemsSource = Config.AvatorNames;
            {
                List<int> virAvators = Config.AvatorNames.Select(v => v.Key).ToList();
                for (int idx = 0; idx < virAvators.Count; idx++)
                {
                    if (virAvators[idx] == UserData.VriAvator)
                    {
                        ComboBoxVriEngAvator.SelectedIndex = idx;
                        EditParamsBefore.VriAvator = idx;
                        break;
                    }
                }
                if (ComboBoxVriEngAvator.SelectedIndex < 0)
                {
                    ComboBoxVriEngAvator.SelectedIndex = 0;
                    UserData.VriAvator = virAvators[0];
                    EditParamsBefore.VriAvator = virAvators[0];
                }
            }

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

                if (UserData.InterfaceSwitch[(int)ListenInterface.Socket3] == true)
                {
                    SockTask3.StartSocketTasks(Config.SocketAddress3, Config.SocketPortNum3);
                    EllipseSocket3.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket3] = true;
                }
                else
                {
                    EllipseSocket3.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket3] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Http3] == true)
                {
                    HttpTask3.StartHttpTasks(Config.HttpAddress3, Config.HttpPortNum3);
                    EllipseHTTP3.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http3] = true;
                }
                else
                {
                    EllipseHTTP3.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http3] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Socket4] == true)
                {
                    SockTask4.StartSocketTasks(Config.SocketAddress4, Config.SocketPortNum4);
                    EllipseSocket4.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket4] = true;
                }
                else
                {
                    EllipseSocket4.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Socket4] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Http4] == true)
                {
                    HttpTask4.StartHttpTasks(Config.HttpAddress4, Config.HttpPortNum4);
                    EllipseHTTP4.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http4] = true;
                }
                else
                {
                    EllipseHTTP4.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Http4] = false;
                }

                if (UserData.InterfaceSwitch[(int)ListenInterface.Clipboard] == true)
                {
                    EllipseClipboard.Fill = Brushes.LightGreen;
                    UserData.InterfaceSwitch[(int)ListenInterface.Clipboard] = true;
                    SetClipboardListener();
                }
                else
                {
                    EllipseClipboard.Fill = Brushes.Black;
                    UserData.InterfaceSwitch[(int)ListenInterface.Clipboard] = false;
                    RemoveClipboardListener();
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
            SockTask3?.StopSocketTasks();
            HttpTask3?.StopHttpTasks();
            SockTask4?.StopSocketTasks();
            HttpTask4?.StopHttpTasks();

            RemoveClipboardListener();
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
                                    HttpTask3?.SetTaskId(item.TaskId);
                                    HttpTask4?.SetTaskId(item.TaskId);
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
                            HttpTask3?.SetTaskId(0);
                            HttpTask4?.SetTaskId(0);

                            ReEntry = true;
                        });

                    }
                }
            }
            else
            {
                int cnt = LonelyCount;

                if (QuietMessageKeyMax != 0)
                {
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
                }

                // アイコン変化にもつかう
                switch (LonelyCount)
                {
                    case 300:
                        this.Icon = BitmapFrame.Create(new Uri("pack://Application:,,,/Resources/icon2b.ico", UriKind.RelativeOrAbsolute));
                        break;

                    case 600:
                        this.Icon = BitmapFrame.Create(new Uri("pack://Application:,,,/Resources/icon3.ico", UriKind.RelativeOrAbsolute));
                        break;
                }

                LonelyCount++;
                if ((QuietMessageKeyMax == 0) && (LonelyCount > 600))
                {
                    LonelyCount = 0;
                }
                else if (LonelyCount > QuietMessageKeyMax)
                {
                    LonelyCount = 0;
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
                    HttpTask3?.SetTaskId(talk.TaskId);
                    HttpTask4?.SetTaskId(talk.TaskId);
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
                    case 5: RemoveClipboardListener();   break;
                    case 6: SockTask3.StopSocketTasks(); break;
                    case 7: HttpTask3.StopHttpTasks();   break;
                    case 8: SockTask4.StopSocketTasks(); break;
                    case 9: HttpTask4.StopHttpTasks();   break;
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
                    case 5: SetClipboardListener(); break;
                    case 6: SockTask3.StartSocketTasks(Config.SocketAddress3, Config.SocketPortNum3); break;
                    case 7: HttpTask3.StartHttpTasks(Config.HttpAddress3, Config.HttpPortNum3);       break;
                    case 8: SockTask4.StartSocketTasks(Config.SocketAddress4, Config.SocketPortNum4); break;
                    case 9: HttpTask4.StartHttpTasks(Config.HttpAddress4, Config.HttpPortNum4);       break;
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
                case 5: PlayMethodFromClipboard = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 6: SockTask3.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 7: HttpTask3.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 8: SockTask4.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
                case 9: HttpTask4.PlayMethod = UserData.SelectedCallMethod[ListenIf] == 0 ? Methods.sync : Methods.async; break;
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

            UpdateEditParamPanel();
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
            int ListenIf;
            int BouVoice;
            int cid;

            if (ComboBoxEditInterface.SelectedIndex == -1) return;

            ListenIf = (int)ComboBoxEditInterface.SelectedValue;
            BouVoice = (int)ComboBoxEditBouyomiVoice.SelectedValue;
            cid = UserData.SelectedCid[ListenIf][BouVoice];

            TextBlockAvatorName.Text = string.Format(@"⇒ {0}", Config.AvatorNames[cid]);

            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();
            Dictionary<string, Dictionary<string, decimal>> effect = UserData.VoiceParams[ListenIf][BouVoice][cid]["effect"];
            Dictionary<string, Dictionary<string, decimal>> emotion = UserData.VoiceParams[ListenIf][BouVoice][cid]["emotion"];

            ReSetupParams(cid, ref effect,  WrapPanelParams1.Children);
            ReSetupParams(cid, ref emotion, WrapPanelParams2.Children);
        }

        private void UpdateEditVirParamPanel()
        {
            int cid;

            cid = UserData.VriAvator;

            TextBlockAvatorName.Text = string.Format(@"⇒ {0}", Config.AvatorNames[cid]);

            WrapPanelVirParams1.Children.Clear();
            WrapPanelVirParams2.Children.Clear();
            Dictionary<string, Dictionary<string, decimal>> effect = UserData.VriAvators[cid]["effect"];
            Dictionary<string, Dictionary<string, decimal>> emotion = UserData.VriAvators[cid]["emotion"];

            ReSetupParams(cid, ref effect, WrapPanelVirParams1.Children);
            ReSetupParams(cid, ref emotion, WrapPanelVirParams2.Children);
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

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Hyperlink hl = sender as Hyperlink;
            Process.Start(hl.NavigateUri.ToString());
        }

        private void TextBoxAppName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserData != null)
            {
                UserData.FakeChanAppName = (sender as TextBox).Text;
                this.Title = UserData.FakeChanAppName + " " + this.versionStr;
            }
        }

        private void ComboBoxVriEngAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            UserData.VriAvator = EditParamsBefore.VriAvator = (int)cb.SelectedValue;
            UpdateEditVirParamPanel();
        }

        private void CheckBoxVriEng_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            UserData.VriEng = EditParamsBefore.VriEng = (bool)cb.IsChecked;
        }

        private void CheckBoxVriRep_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            UserData.VriRep = EditParamsBefore.VriNoRep = (bool)cb.IsChecked;
        }

        private void ButtonVirParamReset_Click(object sender, RoutedEventArgs e)
        {
            int cid = UserData.VriAvator;

            foreach (var item1 in Config.AvatorEffectParams(cid))
            {
                foreach (var item2 in item1.Value)
                {
                    UserData.VriAvators[cid]["effect"][item1.Key][item2.Key] = item2.Value;
                }
            }
            foreach (var item1 in Config.AvatorEmotionParams(cid))
            {
                foreach (var item2 in item1.Value)
                {
                    UserData.VriAvators[cid]["emotion"][item1.Key][item2.Key] = item2.Value;
                }
            }
            UpdateEditVirParamPanel();
        }

        //

        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hWnd);

        const int WM_DRAWCLIPBOARD = 0x0308;
        const int WM_CHANGECBCHAIN = 0x030D;
        const int WM_CLIPBOARDUPDATE = 0x031D;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    PlayFromClipboard(Clipboard.GetText());
                }
                catch(Exception)
                {
                    //
                }
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void SetClipboardListener()
        {
            AddClipboardFormatListener(Whelper.Handle);
        }

        private void RemoveClipboardListener()
        {
            RemoveClipboardFormatListener(Whelper.Handle);
        }

        private void PlayFromClipboard(string talkText)
        {
            Task.Run(() => {
                int cid;
                List<int> CidList = Config.AvatorNames.Select(c => c.Key).ToList();
                int ListenIf = (int)ListenInterface.Clipboard;
                int voice = EditInputText.EditInputString(0, talkText);

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
                    TaskId = MessQueWrapper.count + 1,
                    Effects = Effects,
                    Emotions = Emotions
                };

                switch (PlayMethodFromClipboard)
                {
                    case Methods.sync:
                        MessQueWrapper.AddQueue(talk);
                        break;

                    case Methods.async:
                        TalkAsyncCall(talk);
                        break;
                }
            });
        }
    }
}
