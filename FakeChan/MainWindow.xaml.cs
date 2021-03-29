using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace FakeChan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Configs Config;
        MessQueueWrapper MessQueWrapper;
        SocketTasks SockTask = null;
        IpcTasks IpcTask = null;

        WCFClient WcfClient;
        DispatcherTimer KickTalker;
        List<ComboBox> MapAvatorsComboBoxList;
        bool ReEntry;
        object lockObj = new object();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // AssistantSeikaとの接続
                // シンプルな例は
                // https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/interface/wcf/wcf-004
                // を見てください。
                WcfClient = new WCFClient();

                // 設定色々
                Config = new Configs();

                // メッセージキューを使うよ！
                MessQueWrapper = new MessQueueWrapper();

                // 読み上げバックグラウンドタスク起動
                KickTalker = new DispatcherTimer();
                KickTalker.Tick += new EventHandler(KickTalker_Tick);
                KickTalker.Interval = new TimeSpan(0, 0, 1);
                ReEntry = true;
                KickTalker.Start();

                // ソケットサービス起動（棒読みちゃんのフリをします！）
                SockTask = new SocketTasks(ref Config, ref MessQueWrapper, ref WcfClient);
                SockTask.StartSocketTasks();
                EllipseSocket.Fill = Brushes.LightGreen;

                // IPCサービス起動（棒読みちゃんのフリをします！）
                IpcTask = new IpcTasks(ref Config, ref MessQueWrapper, ref WcfClient);
                IpcTask.StartIpcTasks();
                EllipseIpc.Fill = Brushes.LightGreen;

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
                ComboBoxMapAvator10,
                ComboBoxMapAvator11,
                ComboBoxMapAvator12,
                ComboBoxMapAvator13,
                ComboBoxMapAvator14,
                ComboBoxMapAvator15,
                ComboBoxMapAvator16,
                ComboBoxMapAvator17,
                ComboBoxMapAvator18,
            };

            ComboBoxCallMethodIPC.ItemsSource = null;
            ComboBoxCallMethodIPC.ItemsSource = Config.PlayMethods;
            ComboBoxCallMethodIPC.SelectedIndex = 0;

            ComboBoxCallMethodSocket.ItemsSource = null;
            ComboBoxCallMethodSocket.ItemsSource = Config.PlayMethods;
            ComboBoxCallMethodSocket.SelectedIndex = 0;

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
            SockTask.StopSocketTasks();
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
                                    IpcTask.SetTaskId(item.TaskId);
                                    TextBoxReceveText.Text = item.Message;
                                    TextBlockAvatorText.Text = string.Format(@"{0} ⇒ {1}", bv[item.BouyomiVoice], an[item.Cid] );
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

        private void ComboBoxMapAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int voice = 0;
            int cid;
            ComboBox cb = sender as ComboBox;

            switch(cb.Name)
            {
                case "ComboBoxMapAvator0":  voice = 0;  break;
                case "ComboBoxMapAvator1":  voice = 1;  break;
                case "ComboBoxMapAvator2":  voice = 2;  break;
                case "ComboBoxMapAvator3":  voice = 3;  break;
                case "ComboBoxMapAvator4":  voice = 4;  break;
                case "ComboBoxMapAvator5":  voice = 5;  break;
                case "ComboBoxMapAvator6":  voice = 6;  break;
                case "ComboBoxMapAvator7":  voice = 7;  break;
                case "ComboBoxMapAvator8":  voice = 8;  break;
                case "ComboBoxMapAvator10": voice = 9;  break;
                case "ComboBoxMapAvator11": voice = 10; break;
                case "ComboBoxMapAvator12": voice = 11; break;
                case "ComboBoxMapAvator13": voice = 12; break;
                case "ComboBoxMapAvator14": voice = 13; break;
                case "ComboBoxMapAvator15": voice = 14; break;
                case "ComboBoxMapAvator16": voice = 15; break;
                case "ComboBoxMapAvator17": voice = 16; break;
                case "ComboBoxMapAvator18": voice = 17; break;
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

        private void ComboBoxCallMethodIPC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            IpcTask.PlayMethod = ((KeyValuePair<methods, string>)cb.SelectedItem).Key;
        }

        private void ComboBoxCallMethodSocket_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            SockTask.PlayMethod = ((KeyValuePair<methods, string>)cb.SelectedItem).Key;
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

    }
}
