using System;
using System.Collections.Generic;
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
        string versionStr = "Ver 1.2.1";
        Configs Config;
        MessQueueWrapper MessQueWrapper;
        IpcTasks IpcTask = null;
        SocketTasks SockTask = null;
        HttpTasks HttpTask = null;
        SocketTasks SockTask2 = null;
        HttpTasks HttpTask2 = null;
        EditParamsBefore TestEdit = new EditParamsBefore();

        WCFClient WcfClient;
        DispatcherTimer KickTalker;
        List<ComboBox> MapAvatorsComboBoxList;
        List<Ellipse> LampList;
        List<ComboBox> MethodList;

        public UserDefData UserData;

        bool ReEntry;
        object lockObj = new object();

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
                // 古い版のデータだったら補正
                // 設定値が取り込めない環境がある模様だ。対策する

                if (UserData is null)
                {
                    UserData = new UserDefData();
                }

                if (UserData.AddSuffixStr is null)
                {
                    UserData.AddSuffix = false;
                    UserData.AddSuffixStr = "(以下略";
                    UserData.TextLength = 96;
                }

                if (UserData.MatchPattern1 is null)
                {
                    UserData.MatchPattern1 = @"([^0-9０-９])[8８]{3,}";
                    UserData.ReplcaeStr1 = @"$1パチパチパチ";
                }

                if (UserData.MatchPattern2 is null)
                {
                    UserData.MatchPattern2 = @"([^a-zA-Zａ-ｚＡ-Ｚ])[WwＷｗ]{1,}";
                    UserData.ReplcaeStr2 = @"$1ワラワラ";
                }

                if ((UserData.ParamAssignList is null) || (UserData.ParamAssignList.Count == 54))
                {
                    UserData.ParamAssignList = UserData.ParamAssignList = new Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>>();
                }

                if ((UserData.Voice2Cid is null) || UserData.Voice2Cid.ContainsKey(45))
                {
                    UserData.Voice2Cid = Config.B2Amap;
                }

                if ((UserData.LampSwitch is null)||(UserData.LampSwitch.Count == 6))
                {
                    UserData.LampSwitch = new Dictionary<VoiceIndex, bool>()
                    {
                        {VoiceIndex.IPC1, true },
                        {VoiceIndex.Socket1, true },
                        {VoiceIndex.Http1, true },
                        {VoiceIndex.Socket2, false },
                        {VoiceIndex.Http2, false },
                    };
                }

                if ((UserData.MethodAssignList is null)|| (UserData.MethodAssignList.Count == 6))
                {
                    UserData.MethodAssignList = new Dictionary<int, int>()
                    {
                        { 0, 0 },
                        { 1, 0 },
                        { 2, 0 },
                        { 3, 0 },
                        { 4, 0 }
                    };
                }
            }
            catch (Exception e0)
            {
                MessageBox.Show(e0.Message, "設定値読み込みの問題3");
                Application.Current.Shutdown();
                return;
            }

            // メッセージキューを使うよ！
            MessQueWrapper = new MessQueueWrapper();

            // 設定2用
            Config.TextLength = UserData.TextLength;
            Config.AddSuffix = UserData.AddSuffix;
            Config.AddSuffixStr = UserData.AddSuffixStr;

            TextBoxTextLength.Text = Config.TextLength.ToString();
            CheckBoxAddSuffix.IsChecked = Config.AddSuffix;
            TextBoxAddSuffixStr.Text = Config.AddSuffixStr;
            EditParamsBefore.LimitTextLength = Config.TextLength;
            EditParamsBefore.IsUseSuffixString = Config.AddSuffix;
            EditParamsBefore.SuffixString = Config.AddSuffixStr;

            TextBoxMatchPatternStr1.Text = EditParamsBefore.MatchPattern1 = UserData.MatchPattern1;
            TextBoxMatchPatternStr2.Text = EditParamsBefore.MatchPattern2 = UserData.MatchPattern2;
            TextBoxMatchPatternStr3.Text = EditParamsBefore.MatchPattern3 = UserData.MatchPattern3;
            TextBoxMatchPatternStr4.Text = EditParamsBefore.MatchPattern4 = UserData.MatchPattern4;
            TextBoxMatchPatternStr5.Text = EditParamsBefore.MatchPattern5 = UserData.MatchPattern5;
            TextBoxMatchPatternStr6.Text = EditParamsBefore.MatchPattern6 = UserData.MatchPattern6;
            TextBoxMatchPatternStr7.Text = EditParamsBefore.MatchPattern7 = UserData.MatchPattern7;

            TextBoxReplaceStr1.Text = EditParamsBefore.ReplcaeStr1 = UserData.ReplcaeStr1;
            TextBoxReplaceStr2.Text = EditParamsBefore.ReplcaeStr2 = UserData.ReplcaeStr2;
            TextBoxReplaceStr3.Text = EditParamsBefore.ReplcaeStr3 = UserData.ReplcaeStr3;
            TextBoxReplaceStr4.Text = EditParamsBefore.ReplcaeStr4 = UserData.ReplcaeStr4;
            TextBoxReplaceStr5.Text = EditParamsBefore.ReplcaeStr5 = UserData.ReplcaeStr5;
            TextBoxReplaceStr6.Text = EditParamsBefore.ReplcaeStr6 = UserData.ReplcaeStr6;
            TextBoxReplaceStr7.Text = EditParamsBefore.ReplcaeStr7 = UserData.ReplcaeStr7;

            CheckBoxIsReplace1.IsChecked = EditParamsBefore.IsUseReplcae1 = UserData.IsUseReplcae1;
            CheckBoxIsReplace2.IsChecked = EditParamsBefore.IsUseReplcae2 = UserData.IsUseReplcae2;
            CheckBoxIsReplace3.IsChecked = EditParamsBefore.IsUseReplcae3 = UserData.IsUseReplcae3;
            CheckBoxIsReplace4.IsChecked = EditParamsBefore.IsUseReplcae4 = UserData.IsUseReplcae4;
            CheckBoxIsReplace5.IsChecked = EditParamsBefore.IsUseReplcae5 = UserData.IsUseReplcae5;
            CheckBoxIsReplace6.IsChecked = EditParamsBefore.IsUseReplcae6 = UserData.IsUseReplcae6;
            CheckBoxIsReplace7.IsChecked = EditParamsBefore.IsUseReplcae7 = UserData.IsUseReplcae7;

            // バックグラウンドタスク用オブジェクト
            IpcTask = new IpcTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData.ParamAssignList);
            SockTask  = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData.ParamAssignList);
            SockTask2 = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData.ParamAssignList);
            HttpTask  = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData.ParamAssignList);
            HttpTask2 = new HttpTasks(ref Config, ref MessQueWrapper, ref WcfClient, ref UserData.ParamAssignList);

            // 非同期発声時のGUI操作用
            IpcTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask.OnCallAsyncTalk += TalkAsyncCall;
            SockTask2.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask.OnCallAsyncTalk += TalkAsyncCall;
            HttpTask2.OnCallAsyncTalk += TalkAsyncCall;

            LampList = new List<Ellipse>()
            {
                EllipseIpc,
                EllipseSocket,
                EllipseHTTP,
                EllipseSocket2,
                EllipseHTTP2
            };

            LampList[0].Tag = UserData.LampSwitch[VoiceIndex.IPC1];
            LampList[1].Tag = UserData.LampSwitch[VoiceIndex.Socket1];
            LampList[2].Tag = UserData.LampSwitch[VoiceIndex.Http1];
            LampList[3].Tag = UserData.LampSwitch[VoiceIndex.Socket2];
            LampList[4].Tag = UserData.LampSwitch[VoiceIndex.Http2];

            MethodList = new List<ComboBox>()
            {
                ComboBoxCallMethodIPC,
                ComboBoxCallMethodSocket,
                ComboBoxCallMethodHTTP,
                ComboBoxCallMethodSocket2,
                ComboBoxCallMethodHTTP2
            };

            for(int idx=0; idx < MethodList.Count; idx++)
            {
                MethodList[idx].ItemsSource = null;
                MethodList[idx].ItemsSource = Config.PlayMethods;
                MethodList[idx].SelectedIndex = -1;
                MethodList[idx].IsEnabled = true;
                MethodList[idx].Tag = idx;
                MethodList[idx].SelectedIndex = UserData.MethodAssignList[idx];
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
                ComboBoxMapAvator10,
                ComboBoxMapAvator11,
                ComboBoxMapAvator12,
                ComboBoxMapAvator13,
                ComboBoxMapAvator14,
                ComboBoxMapAvator15,
                ComboBoxMapAvator16,
                ComboBoxMapAvator17,
                ComboBoxMapAvator18,
                ComboBoxMapAvator20,
                ComboBoxMapAvator21,
                ComboBoxMapAvator22,
                ComboBoxMapAvator23,
                ComboBoxMapAvator24,
                ComboBoxMapAvator25,
                ComboBoxMapAvator26,
                ComboBoxMapAvator27,
                ComboBoxMapAvator28,
                ComboBoxMapAvator30,
                ComboBoxMapAvator31,
                ComboBoxMapAvator32,
                ComboBoxMapAvator33,
                ComboBoxMapAvator34,
                ComboBoxMapAvator35,
                ComboBoxMapAvator36,
                ComboBoxMapAvator37,
                ComboBoxMapAvator38,
                ComboBoxMapAvator40,
                ComboBoxMapAvator41,
                ComboBoxMapAvator42,
                ComboBoxMapAvator43,
                ComboBoxMapAvator44,
                ComboBoxMapAvator45,
                ComboBoxMapAvator46,
                ComboBoxMapAvator47,
                ComboBoxMapAvator48
            };

            for (int idx = 0; idx < MapAvatorsComboBoxList.Count; idx++)
            {
                MapAvatorsComboBoxList[idx].ItemsSource = null;
                MapAvatorsComboBoxList[idx].ItemsSource = Config.AvatorNames;
                MapAvatorsComboBoxList[idx].SelectedIndex = -1;
                MapAvatorsComboBoxList[idx].IsEnabled = true;
                MapAvatorsComboBoxList[idx].Tag = idx;
            }

            Dictionary<int, string> avators = Config.AvatorNames;
            Dictionary<int, int> v2c = UserData.Voice2Cid.ToDictionary(k => k.Key, v => v.Value);
            foreach (var v2cItem in v2c)
            {
                if (avators.ContainsKey(v2cItem.Value))
                {
                    int idx = 0;
                    foreach (KeyValuePair<int, string> listItem in MapAvatorsComboBoxList[v2cItem.Key].Items)
                    {
                        if (v2cItem.Value == listItem.Key) break;
                        idx++;
                    }
                    MapAvatorsComboBoxList[v2cItem.Key].SelectedIndex = idx;
                }
                else
                {
                    MapAvatorsComboBoxList[v2cItem.Key].SelectedIndex = 0;
                }
            }

            ComboBoxBouyomiVoice.ItemsSource = null;
            ComboBoxBouyomiVoice.ItemsSource = Config.BouyomiVoices;
            ComboBoxBouyomiVoice.SelectedIndex = 0;

            try
            {
                // 読み上げバックグラウンドタスク起動
                KickTalker = new DispatcherTimer();
                KickTalker.Tick += new EventHandler(KickTalker_Tick);
                KickTalker.Interval = new TimeSpan(0, 0, 1);
                ReEntry = true;
                KickTalker.Start();

                // 通信受付口バックグラウンドタスク起動
                for (int idx=0; idx < LampList.Count; idx++)
                {
                    bool sw = (bool)LampList[idx].Tag;
                    if (sw)
                    {
                        switch (idx)
                        {
                            case 0:
                                if (IpcTask.StartIpcTasks(Config.IPCChannelName))
                                {
                                    LampList[idx].Fill = Brushes.LightGreen;
                                    LampList[idx].Tag = true;
                                    UserData.LampSwitch[VoiceIndex.IPC1] = true;
                                }
                                else
                                {
                                    LampList[idx].Fill = Brushes.Black;
                                    LampList[idx].Tag = false;
                                    UserData.LampSwitch[VoiceIndex.IPC1] = false;
                                }
                                break;

                            case 1:
                                SockTask.StartSocketTasks(Config.SocketAddress, Config.SocketPortNum);
                                LampList[idx].Fill = Brushes.LightGreen;
                                LampList[idx].Tag = true;
                                UserData.LampSwitch[VoiceIndex.Socket1] = true;
                                break;

                            case 2:
                                HttpTask.StartHttpTasks(Config.HttpAddress, Config.HttpPortNum);
                                LampList[idx].Fill = Brushes.LightGreen;
                                LampList[idx].Tag = true;
                                UserData.LampSwitch[VoiceIndex.Http1] = true;
                                break;

                            case 3:
                                SockTask2.StartSocketTasks(Config.SocketAddress, Config.SocketPortNum);
                                LampList[idx].Fill = Brushes.LightGreen;
                                LampList[idx].Tag = true;
                                UserData.LampSwitch[VoiceIndex.Socket2] = true;
                                break;

                            case 4:
                                HttpTask2.StartHttpTasks(Config.HttpAddress, Config.HttpPortNum);
                                LampList[idx].Fill = Brushes.LightGreen;
                                LampList[idx].Tag = true;
                                UserData.LampSwitch[VoiceIndex.Http2] = true;
                                break;
                        }
                    }
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
                            Dictionary<int, string> bv = Config.BouyomiVoices;
                            Dictionary<int, string> an = Config.AvatorNames;
                            foreach (var item in MessQueWrapper.QueueRef().GetConsumingEnumerable())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    IpcTask?.SetTaskId(item.TaskId);
                                    HttpTask?.SetTaskId(item.TaskId);
                                    HttpTask2?.SetTaskId(item.TaskId);
                                    TextBlockReceveText.Text = item.Message;
                                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1}", bv[item.BouyomiVoiceIdx], an[item.Cid] );
                                });

                                WcfClient.Talk(item.Cid, item.Message, "", item.Effects, item.Emotions);
                            }

                            IpcTask.SetTaskId(0);

                            ReEntry = true;
                        });

                    }
                }

            }
        }

        private void TalkAsyncCall(MessageData talk)
        {
            Task.Run(() =>
            {
                Dictionary<int, string> bv = Config.BouyomiVoices;
                Dictionary<int, string> an = Config.AvatorNames;
                Dispatcher.Invoke(() =>
                {
                    IpcTask?.SetTaskId(talk.TaskId);
                    HttpTask?.SetTaskId(talk.TaskId);
                    HttpTask2?.SetTaskId(talk.TaskId);
                    TextBoxReceveText.Text = talk.Message;
                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1}", bv[talk.BouyomiVoiceIdx], an[talk.Cid]);
                });

                WcfClient.TalkAsync(talk.Cid, talk.Message, talk.Effects, talk.Emotions);
            });
        }

        private void ComboBoxMapAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            int voiceIdx = (int)cb.Tag;
            int cid = ((KeyValuePair<int, string>)cb.SelectedItem).Key;

            Config.B2Amap[voiceIdx] = cid;
            UserData.Voice2Cid[voiceIdx] = cid;

            if (!UserData.ParamAssignList.ContainsKey(voiceIdx))
            {
                UserData.ParamAssignList[voiceIdx] = new Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>();
            }

            if (!UserData.ParamAssignList[voiceIdx].ContainsKey(cid))
            {
                UserData.ParamAssignList[voiceIdx].Add(cid, new Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>());
                UserData.ParamAssignList[voiceIdx][cid].Add("effect", new Dictionary<string, Dictionary<string, decimal>>());
                UserData.ParamAssignList[voiceIdx][cid].Add("emotion", new Dictionary<string, Dictionary<string, decimal>>());
                foreach (var item1 in Config.AvatorEffectParams(cid))
                {
                    UserData.ParamAssignList[voiceIdx][cid]["effect"].Add(item1.Key, new Dictionary<string, decimal>());
                    foreach ( var item2 in item1.Value)
                    {
                        UserData.ParamAssignList[voiceIdx][cid]["effect"][item1.Key].Add(item2.Key, item2.Value);
                    }
                }
                foreach (var item1 in Config.AvatorEmotionParams(cid))
                {
                    UserData.ParamAssignList[voiceIdx][cid]["emotion"].Add(item1.Key, new Dictionary<string, decimal>());
                    foreach (var item2 in item1.Value)
                    {
                        UserData.ParamAssignList[voiceIdx][cid]["emotion"][item1.Key].Add(item2.Key, item2.Value);
                    }
                }
            }

            if (ComboBoxBouyomiVoice.SelectedIndex != voiceIdx)
            {
                ComboBoxBouyomiVoice.SelectedIndex = voiceIdx;
            }
            else
            {
                UpdateEditParamPanel(voiceIdx, Config.B2Amap[voiceIdx]);
            }
        }

        private void ComboBoxBouyomiVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int voiceIdx = Convert.ToInt32(ComboBoxBouyomiVoice.SelectedValue);

            UpdateEditParamPanel(voiceIdx, Config.B2Amap[voiceIdx]);
        }

        private void ComboBoxCallMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            int idx = (int)cb.Tag;

            Methods md = Config.PlayMethodsMap[cb.SelectedIndex];

            UserData.MethodAssignList[idx] = cb.SelectedIndex;
            switch (idx)
            {
                case 0 : IpcTask.PlayMethod   = md; break;
                case 1 : SockTask.PlayMethod  = md; break;
                case 2 : HttpTask.PlayMethod  = md; break;
                case 3 : SockTask2.PlayMethod = md; break;
                case 4 : HttpTask2.PlayMethod = md; break;
                default: break;
            }
        }

        private void ButtonParamReset_Click(object sender, RoutedEventArgs e)
        {
            int voiceIdx = Convert.ToInt32(ComboBoxBouyomiVoice.SelectedValue);
            int cid = Config.B2Amap[voiceIdx];
            foreach(var item1 in Config.AvatorEffectParams(cid))
            {
                foreach(var item2 in item1.Value)
                {
                    UserData.ParamAssignList[voiceIdx][cid]["effect"][item1.Key][item2.Key] = item2.Value;
                }
            }
            foreach (var item1 in Config.AvatorEmotionParams(cid))
            {
                foreach (var item2 in item1.Value)
                {
                    UserData.ParamAssignList[voiceIdx][cid]["emotion"][item1.Key][item2.Key] = item2.Value;
                }
            }
            UpdateEditParamPanel(voiceIdx, cid);
        }

        private void UpdateEditParamPanel(int voiceIdx, int cid)
        {
            LabelSelectedAvator.Content = string.Format(@"⇒ {0}", Config.AvatorNames[cid]);
            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();
            Dictionary<string, Dictionary<string, decimal>> effect = UserData.ParamAssignList[voiceIdx][cid]["effect"];
            Dictionary<string, Dictionary<string, decimal>> emotion = UserData.ParamAssignList[voiceIdx][cid]["emotion"];

            ReSetupParams(cid, ref effect,  WrapPanelParams1.Children);
            ReSetupParams(cid, ref emotion, WrapPanelParams2.Children);
        }

        private void ReSetupParams(int cid, ref Dictionary<string, Dictionary<string, decimal>> @params, UIElementCollection panel)
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
            int voiceIdx = ((KeyValuePair<int, string>)ComboBoxBouyomiVoice.SelectedItem).Key;
            int voice = 0;

            // See https://gist.github.com/pinzolo/2814091
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);

            TestEdit.EditInputString(voice, text);

            Task.Run(() =>
            {
                int cid =  Config.B2Amap[voiceIdx];
                Dictionary<string, decimal> Effects = UserData.ParamAssignList[voiceIdx][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                Dictionary<string, decimal> Emotions = UserData.ParamAssignList[voiceIdx][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                WcfClient.Talk(cid, TestEdit.ChangedTalkText, "", Effects, Emotions);

                Dispatcher.Invoke(() =>
                {
                    TextBoxReceveText.IsEnabled = true;
                    ButtonTestTalk.IsEnabled = true;
                    ComboBoxBouyomiVoice.IsEnabled = true;
                });
            });
        }

        private void EllipseConnect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int lampNo = 0;
            Ellipse ep = sender as Ellipse;

            for(lampNo = 0; lampNo < LampList.Count; lampNo++)
            {
                if (ep.Equals(LampList[lampNo])) break;
            }

            bool sw = (bool)LampList[lampNo].Tag;

            if (sw)
            {
                ep.Fill = Brushes.Black;
            }
            else
            {
                ep.Fill = Brushes.LightGreen;
            }

            LampList[lampNo].Tag = !sw;

            if (sw)
            {
                switch (lampNo)
                {
                    case 0: IpcTask.StopIpcTasks();      UserData.LampSwitch[VoiceIndex.IPC1]    = false; break;
                    case 1: SockTask.StopSocketTasks();  UserData.LampSwitch[VoiceIndex.Socket1] = false; break;
                    case 2: HttpTask.StopHttpTasks();    UserData.LampSwitch[VoiceIndex.Http1]   = false; break;
                    case 3: SockTask2.StopSocketTasks(); UserData.LampSwitch[VoiceIndex.Socket2] = false; break;
                    case 4: HttpTask2.StopHttpTasks();   UserData.LampSwitch[VoiceIndex.Http2]   = false; break;
                }
            }
            else
            {
                switch (lampNo)
                {
                    case 0:
                        if (!IpcTask.StartIpcTasks(Config.IPCChannelName))
                        {
                            ep.Fill = Brushes.Black;
                            LampList[lampNo].Tag = false;
                            UserData.LampSwitch[VoiceIndex.IPC1] = false;
                        }
                        else
                        {
                            UserData.LampSwitch[VoiceIndex.IPC1] = true;
                        }
                        break;
                    case 1: SockTask.StartSocketTasks(Config.SocketAddress, Config.SocketPortNum);    UserData.LampSwitch[VoiceIndex.Socket1] = true; break;
                    case 2: HttpTask.StartHttpTasks(Config.HttpAddress, Config.HttpPortNum);          UserData.LampSwitch[VoiceIndex.Http1]   = true; break;
                    case 3: SockTask2.StartSocketTasks(Config.SocketAddress2, Config.SocketPortNum2); UserData.LampSwitch[VoiceIndex.Socket2] = true; break;
                    case 4: HttpTask2.StartHttpTasks(Config.HttpAddress2, Config.HttpPortNum2);       UserData.LampSwitch[VoiceIndex.Http2]   = true; break;
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

        private void CheckBoxIsReplace_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            switch(cb.Name)
            {
                case "CheckBoxIsReplace1": EditParamsBefore.IsUseReplcae1 = UserData.IsUseReplcae1 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace2": EditParamsBefore.IsUseReplcae2 = UserData.IsUseReplcae2 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace3": EditParamsBefore.IsUseReplcae3 = UserData.IsUseReplcae3 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace4": EditParamsBefore.IsUseReplcae4 = UserData.IsUseReplcae4 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace5": EditParamsBefore.IsUseReplcae5 = UserData.IsUseReplcae5 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace6": EditParamsBefore.IsUseReplcae6 = UserData.IsUseReplcae6 = (bool)cb.IsChecked; break;
                case "CheckBoxIsReplace7": EditParamsBefore.IsUseReplcae7 = UserData.IsUseReplcae7 = (bool)cb.IsChecked; break;
            }
        }

        private void TextBoxMatchPatternStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            switch(tb.Name)
            {
                case "TextBoxMatchPatternStr1": EditParamsBefore.MatchPattern1 = UserData.MatchPattern1 = tb.Text; break;
                case "TextBoxMatchPatternStr2": EditParamsBefore.MatchPattern2 = UserData.MatchPattern2 = tb.Text; break;
                case "TextBoxMatchPatternStr3": EditParamsBefore.MatchPattern3 = UserData.MatchPattern3 = tb.Text; break;
                case "TextBoxMatchPatternStr4": EditParamsBefore.MatchPattern4 = UserData.MatchPattern4 = tb.Text; break;
                case "TextBoxMatchPatternStr5": EditParamsBefore.MatchPattern5 = UserData.MatchPattern5 = tb.Text; break;
                case "TextBoxMatchPatternStr6": EditParamsBefore.MatchPattern6 = UserData.MatchPattern6 = tb.Text; break;
                case "TextBoxMatchPatternStr7": EditParamsBefore.MatchPattern7 = UserData.MatchPattern7 = tb.Text; break;
            }
        }

        private void TextBoxReplaceStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            switch (tb.Name)
            {
                case "TextBoxReplaceStr1": EditParamsBefore.ReplcaeStr1 = UserData.ReplcaeStr1 = tb.Text; break;
                case "TextBoxReplaceStr2": EditParamsBefore.ReplcaeStr2 = UserData.ReplcaeStr2 = tb.Text; break;
                case "TextBoxReplaceStr3": EditParamsBefore.ReplcaeStr3 = UserData.ReplcaeStr3 = tb.Text; break;
                case "TextBoxReplaceStr4": EditParamsBefore.ReplcaeStr4 = UserData.ReplcaeStr4 = tb.Text; break;
                case "TextBoxReplaceStr5": EditParamsBefore.ReplcaeStr5 = UserData.ReplcaeStr5 = tb.Text; break;
                case "TextBoxReplaceStr6": EditParamsBefore.ReplcaeStr6 = UserData.ReplcaeStr6 = tb.Text; break;
                case "TextBoxReplaceStr7": EditParamsBefore.ReplcaeStr7 = UserData.ReplcaeStr7 = tb.Text; break;
            }
        }

    }
}
