using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Windows.Threading;

namespace FakeChan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        WCFClient WcfClient;
        Dictionary<int, string> AvatorNameList;
        Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>> AvatorParamList;
        BlockingCollection<MessageData> MessQue;
        IPListenPoint ListenEndPoint;
        TcpListener Listener;
        Action BGListen;
        DispatcherTimer KickTalker;

        bool KeepListen;
        int SelectedCid;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WcfClient = new WCFClient();
                AvatorNameList = WcfClient.AvatorList2().ToDictionary(k => k.Key, v => string.Format(@"{0} : {1}({2})", v.Key, v.Value["name"], v.Value["prod"]));
                AvatorParamList = AvatorNameList.ToDictionary(k => k.Key, v => WcfClient.GetDefaultParams2(v.Key));
                MessQue = new BlockingCollection<MessageData>();

                KickTalker = new DispatcherTimer();
                KickTalker.Tick += new EventHandler(KickTalker_Tick);
                KickTalker.Interval = new TimeSpan(0, 0, 1);
                KickTalker.Start();

                ListenEndPoint = new IPListenPoint();
                ListenEndPoint.Address = IPAddress.Parse("127.0.0.1");
                ListenEndPoint.PortNum = 50001;
            }
            catch(Exception e1)
            {
                MessageBox.Show(e1.Message, "Sorry1");
                Application.Current.Shutdown();
            }

            Binding myBinding = new Binding("PortNum");
            myBinding.Source = ListenEndPoint;
            TextBoxListenPort.SetBinding(TextBox.TextProperty, myBinding);

            ComboBoxAvator.ItemsSource = null;
            ComboBoxMapAvator0.ItemsSource = null;
            ComboBoxMapAvator1.ItemsSource = null;
            ComboBoxMapAvator2.ItemsSource = null;
            ComboBoxMapAvator3.ItemsSource = null;
            ComboBoxMapAvator4.ItemsSource = null;
            ComboBoxMapAvator5.ItemsSource = null;
            ComboBoxMapAvator6.ItemsSource = null;
            ComboBoxMapAvator7.ItemsSource = null;
            ComboBoxMapAvator8.ItemsSource = null;
            if (AvatorNameList.Count != 0)
            {
                ComboBoxAvator.ItemsSource = AvatorNameList;
                ComboBoxAvator.SelectedIndex = 0;
                ComboBoxAvator.IsEnabled = true;

                ComboBoxMapAvator0.ItemsSource = AvatorNameList;
                ComboBoxMapAvator0.SelectedIndex = 0;
                ComboBoxMapAvator0.IsEnabled = true;
                ComboBoxMapAvator1.ItemsSource = AvatorNameList;
                ComboBoxMapAvator1.SelectedIndex = 0;
                ComboBoxMapAvator1.IsEnabled = true;
                ComboBoxMapAvator2.ItemsSource = AvatorNameList;
                ComboBoxMapAvator2.SelectedIndex = 0;
                ComboBoxMapAvator2.IsEnabled = true;
                ComboBoxMapAvator3.ItemsSource = AvatorNameList;
                ComboBoxMapAvator3.SelectedIndex = 0;
                ComboBoxMapAvator3.IsEnabled = true;
                ComboBoxMapAvator4.ItemsSource = AvatorNameList;
                ComboBoxMapAvator4.SelectedIndex = 0;
                ComboBoxMapAvator4.IsEnabled = true;
                ComboBoxMapAvator5.ItemsSource = AvatorNameList;
                ComboBoxMapAvator5.SelectedIndex = 0;
                ComboBoxMapAvator5.IsEnabled = true;
                ComboBoxMapAvator6.ItemsSource = AvatorNameList;
                ComboBoxMapAvator6.SelectedIndex = 0;
                ComboBoxMapAvator6.IsEnabled = true;
                ComboBoxMapAvator7.ItemsSource = AvatorNameList;
                ComboBoxMapAvator7.SelectedIndex = 0;
                ComboBoxMapAvator7.IsEnabled = true;
                ComboBoxMapAvator8.ItemsSource = AvatorNameList;
                ComboBoxMapAvator8.SelectedIndex = 0;
                ComboBoxMapAvator8.IsEnabled = true;
            }
            else
            {
                ComboBoxAvator.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KickTalker.Stop();
        }

        private void KickTalker_Tick(object sender, EventArgs e)
        {
            if (MessQue.Count != 0)
            {
                Task.Run(() =>
                {
                    foreach (var item in MessQue.GetConsumingEnumerable())
                    {
                        WcfClient.Talk(item.Cid, item.Message, "", item.Effects, item.Emotions);
                    }
                });
            }
        }

        private void ComboBoxMapAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

        }

        private void ComboBoxAvator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cid = Convert.ToInt32(ComboBoxAvator.SelectedValue);

            SelectedCid = cid;

            WrapPanelParams1.Children.Clear();
            WrapPanelParams2.Children.Clear();

            ReSetupParams(cid, AvatorParamList[cid]["effect"],  WrapPanelParams1.Children);
            ReSetupParams(cid, AvatorParamList[cid]["emotion"], WrapPanelParams2.Children);
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

        private void TextBoxListenPort_LostFocus(object sender, RoutedEventArgs e)
        {
            ButtonStart.IsEnabled = false;

            if (TextBoxListenPort.Text != "")
            {
                int p;

                if (Int32.TryParse(TextBoxListenPort.Text, out p))
                {
                    if ((p > 1023) && (p < 65536))
                    {
                        ButtonStart.IsEnabled = true;
                    }
                }
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonStart.IsEnabled = false;
                TextBoxListenPort.IsEnabled = false;
                Listener = new TcpListener(ListenEndPoint.Address, ListenEndPoint.PortNum);
                KeepListen = true;
                SetupBGTask();
                Task.Run(BGListen);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Sorry2");
                ButtonStart.IsEnabled = true;
                ButtonStop.IsEnabled = false;
            }

            ButtonStop.IsEnabled = true;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            KeepListen = false;
            Listener.Stop();
            ButtonStart.IsEnabled = true;
            ButtonStop.IsEnabled = false;
            TextBoxListenPort.IsEnabled = true;
        }

        private void SetupBGTask()
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

            BGListen = (()=>{

                int SkipSize = 2 * 4;

                while (KeepListen) // とりあえずの待ち受け構造
                {
                    try
                    {
                        Listener.Start();
                        TcpClient client = Listener.AcceptTcpClient();
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

                        Dispatcher.Invoke(() => {

                            int cid = SelectedCid;
                            
                            switch (iVoice)
                            {
                                case 0:
                                default:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator0.SelectedItem).Key;
                                    break;

                                case 1:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator1.SelectedItem).Key;
                                    break;

                                case 2:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator2.SelectedItem).Key;
                                    break;

                                case 3:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator3.SelectedItem).Key;
                                    break;

                                case 4:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator4.SelectedItem).Key;
                                    break;

                                case 5:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator5.SelectedItem).Key;
                                    break;

                                case 6:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator6.SelectedItem).Key;
                                    break;

                                case 7:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator7.SelectedItem).Key;
                                    break;

                                case 8:
                                    cid = ((KeyValuePair<int, string>)ComboBoxMapAvator8.SelectedItem).Key;
                                    break;
                            }

                            MessageData talk = new MessageData()
                            {
                                Cid          = cid,
                                Message      = TalkText,
                                BouyomiVoice = iVoice,  // 何かの機能で使うかもしれないので
                                Effects      = AvatorParamList[cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]),
                                Emotions     = AvatorParamList[cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"])
                            };

                            MessQue.TryAdd(talk, 500);
                        });

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
